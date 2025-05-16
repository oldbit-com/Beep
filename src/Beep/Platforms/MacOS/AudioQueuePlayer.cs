using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Channels;
using OldBit.Beep.Helpers;
using OldBit.Beep.Pcm;
using OldBit.Beep.Platforms.MacOS.AudioToolboxInterop;

namespace OldBit.Beep.Platforms.MacOS;

/// <summary>
/// Represents an audio player that uses the Audio Queue Services API to play audio data.
/// </summary>
[SupportedOSPlatform("macos")]
internal sealed class AudioQueuePlayer : IAudioPlayer
{
    private readonly IntPtr _audioQueue;
    private readonly AudioBufferPool _audioBufferPool;
    private readonly Channel<PcmDataReader> _samplesQueue;
    private readonly Thread _queueWorker;
    private readonly float[] _audioData;

    private GCHandle _gch;
    private bool _isQueueRunning;
    private bool _queueWorkerStopped = true;

    internal AudioQueuePlayer(int sampleRate, int channelCount, PlayerOptions playerOptions)
    {
        _gch = GCHandle.Alloc(this);
        _audioData = new float[playerOptions.BufferSizeInBytes / FloatType.SizeInBytes];

        _samplesQueue = Channel.CreateBounded<PcmDataReader>(new BoundedChannelOptions(playerOptions.BufferQueueSize)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        var audioStreamDescription = CreateAudioStreamBasicDescription(sampleRate, channelCount);

        _audioQueue = AudioQueueNewOutput(audioStreamDescription);

        _audioBufferPool = new AudioBufferPool(_audioQueue, playerOptions);
        _audioBufferPool.OnError += OnError;

        _queueWorker = CreateQueueWorkerThread();
    }

    public async ValueTask EnqueueAsync(PcmDataReader reader, CancellationToken cancellationToken) =>
        await _samplesQueue.Writer.WriteAsync(reader, cancellationToken);

    public bool TryEnqueue(PcmDataReader reader) => _samplesQueue.Writer.TryWrite(reader);

    public void Start()
    {
        StartAudioQueue();

        _isQueueRunning = true;
        _queueWorker.Start();
    }

    public void Stop()
    {
        _isQueueRunning = false;

        while (!_queueWorkerStopped)
        {
            Thread.Sleep(5);
        }
        _queueWorker.Join();

        StopAudioQueue();
    }

    public event EventHandler<PlayerErrorEventArgs>? OnError;

    private Thread CreateQueueWorkerThread() => new(RunWorker)
    {
        IsBackground = true,
        Priority = ThreadPriority.AboveNormal,
        Name = "AudioQueuePlayerWorker"
    };

    private void RunWorker() => RunWorkerTask().ContinueWith(task =>
    {
        if (task.IsFaulted)
        {
            OnError?.Invoke(this, new PlayerErrorEventArgs(task.Exception));
        }
    }, TaskContinuationOptions.OnlyOnFaulted);

    private async Task RunWorkerTask()
    {
        _queueWorkerStopped = false;

        while (_isQueueRunning)
        {
            var hasData = false;
            var hasSamplesToPlay = _samplesQueue.Reader.TryRead(out var samples);

            if (hasSamplesToPlay)
            {
                while (_isQueueRunning)
                {
                    var audioDataLength = samples!.ReadSamples(_audioData, _audioData.Length);

                    if (audioDataLength != 0)
                    {
                        await Enqueue(_audioData, audioDataLength, CancellationToken.None);
                        hasData = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (hasData)
            {
                continue;
            }

            // We need to keep sending empty buffers to the audio queue to keep it running
            // if there is no data to play
            Array.Fill(_audioData, 0);

            await Enqueue(_audioData, _audioData.Length / 4, CancellationToken.None);
        }

        _queueWorkerStopped = true;
    }

    private async ValueTask Enqueue(float[] audioData, int length,  CancellationToken cancellationToken)
    {
        var buffer = await _audioBufferPool.GetBufferAsync(cancellationToken);

        unsafe
        {
            var audioQueueBuffer = (AudioQueueBuffer*)buffer.Pointer;
            audioQueueBuffer->AudioDataByteSize = (uint)(length * FloatType.SizeInBytes);

            Marshal.Copy(audioData, 0, audioQueueBuffer->AudioData, length);

            var status = AudioToolbox.AudioQueueEnqueueBuffer(_audioQueue, audioQueueBuffer, 0, null);

            if (status != 0)
            {
                throw new AudioPlayerException($"Failed to enqueue buffer: {status}", status);
            }
        }
    }

    private void StartAudioQueue()
    {
        unsafe
        {
            var status = AudioToolbox.AudioQueueStart(_audioQueue, null);

            if (status != 0)
            {
                throw new AudioPlayerException($"Failed to start audio queue: {status}", status);
            }
        }
    }

    private void StopAudioQueue()
    {
        var status = AudioToolbox.AudioQueueStop(_audioQueue, true);

        if (status != 0)
        {
            throw new AudioPlayerException($"Failed to stop audio queue: {status}", status);
        }
    }

    private static AudioStreamBasicDescription CreateAudioStreamBasicDescription(int sampleRate, int channelCount) => new()
    {
        SampleRate = sampleRate,
        Format = AudioFormatType.LinearPcm,
        FormatFlags = AudioFormatFlags.AudioFormatFlagIsFloat,
        BytesPerPacket = (uint)(channelCount * FloatType.SizeInBytes),
        FramesPerPacket = 1,
        BytesPerFrame = (uint)(channelCount * FloatType.SizeInBytes),
        ChannelsPerFrame = (uint)channelCount,
        BitsPerChannel = FloatType.SizeInBytes * 8
    };

    private IntPtr AudioQueueNewOutput(AudioStreamBasicDescription description)
    {
        unsafe
        {
            var status = AudioToolbox.AudioQueueNewOutput(ref description, &AudioQueueOutputCallback,
                GCHandle.ToIntPtr(_gch), IntPtr.Zero, IntPtr.Zero, 0, out var audioQueue);

            if (status != 0)
            {
                throw new AudioPlayerException($"Failed to create audio queue: {status}", status);
            }

            return audioQueue;
        }
    }

    [UnmanagedCallersOnly]
    private static void AudioQueueOutputCallback(IntPtr userData, IntPtr audioQueue, IntPtr buffer)
    {
        var gch = GCHandle.FromIntPtr(userData);

        if (gch.Target is AudioQueuePlayer player)
        {
            player._audioBufferPool.MakeBufferAvailable(buffer);
        }
    }

    private void ReleaseUnmanagedResources()
    {
        if (_audioQueue == IntPtr.Zero)
        {
            return;
        }

        _audioBufferPool.Dispose();

        var status = AudioToolbox.AudioQueueDispose(_audioQueue, true);

        if (status != 0)
        {
            OnError?.Invoke(this, new PlayerErrorEventArgs(new AudioPlayerException($"Failed to release audio queue: {status}", status)));
        }
    }

    public void Dispose()
    {
        if (_gch.IsAllocated)
        {
            _gch.Free();
        }

        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~AudioQueuePlayer()
    {
        ReleaseUnmanagedResources();
    }
}