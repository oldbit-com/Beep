using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Channels;
using OldBit.Beep.Helpers;
using OldBit.Beep.Platforms.MacOS.AudioToolboxInterop;
using OldBit.Beep.Readers;

namespace OldBit.Beep.Platforms.MacOS;

/// <summary>
/// Represents an audio player that uses the Audio Queue Services API to play audio data.
/// </summary>
[SupportedOSPlatform("macos")]
internal sealed class AudioQueuePlayer: IAudioPlayer
{
    private const int MaxBuffers = 4;
    private const int BufferSizeInBytes = 12288;

    private readonly IntPtr _audioQueue;
    private readonly List<IntPtr> _allocatedAudioBuffers;
    private readonly Channel<IntPtr> _availableAudioBuffers;
    private GCHandle _gch;
    private bool _isStarted;

    internal AudioQueuePlayer(int sampleRate, int channelCount)
    {
        _gch = GCHandle.Alloc(this);

        _availableAudioBuffers = Channel.CreateBounded<IntPtr>(new BoundedChannelOptions(MaxBuffers)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        var audioStreamDescription = GetAudioStreamBasicDescription(sampleRate, channelCount);

        _audioQueue = AudioQueueNewOutput(audioStreamDescription);
        _allocatedAudioBuffers = AudioQueueAllocateBuffers(BufferSizeInBytes);

        foreach (var buffer in _allocatedAudioBuffers)
        {
            _availableAudioBuffers.Writer.TryWrite(buffer);
        }
    }

    public void Start()
    {
        if (_isStarted)
        {
            return;
        }

        unsafe
        {
            var status = AudioToolbox.AudioQueueStart(_audioQueue, null);

            if (status != 0)
            {
                throw new AudioPlayerException($"Failed to start audio queue: {status}", status);
            }
        }

        _isStarted = true;
    }

    public void Stop()
    {
        if (!_isStarted)
        {
            return;
        }

        AudioToolbox.AudioQueueStop(_audioQueue, true);

        _isStarted = false;
    }

    public async Task PlayAsync(PcmDataReader reader, CancellationToken cancellationToken)
    {
        if (_isStarted == false)
        {
            throw new InvalidOperationException("The audio player is not started. Use the Start method to start the player.");
        }

        var pcmData = new float[BufferSizeInBytes / FloatType.SizeInBytes];

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var length = reader.Read(pcmData);
            if (length == 0)
            {
                break;
            }

            await Enqueue(pcmData, length, cancellationToken);
        }
    }

    private async Task Enqueue(float[] pcmData, int length,  CancellationToken cancellationToken)
    {
        var buffer = await _availableAudioBuffers.Reader.ReadAsync(cancellationToken);

        unsafe
        {
            var audioQueueBuffer = (AudioQueueBuffer*)buffer;
            audioQueueBuffer->AudioDataByteSize = (uint)(length * FloatType.SizeInBytes);

            Marshal.Copy(pcmData, 0, audioQueueBuffer->AudioData, length);

            var status = AudioToolbox.AudioQueueEnqueueBuffer(_audioQueue, audioQueueBuffer, 0, null);

            if (status != 0)
            {
                throw new AudioPlayerException($"Failed to enqueue buffer: {status}", status);
            }
        }
    }



    private static AudioStreamBasicDescription GetAudioStreamBasicDescription(int sampleRate, int channelCount) => new()
    {
        SampleRate = sampleRate,
        Format = AudioFormatType.LinearPCM,
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

    private List<IntPtr> AudioQueueAllocateBuffers(int bufferSize)
    {
        var buffers = new List<IntPtr>();

        for (var i = 0; i < MaxBuffers; i++)
        {
            var status = AudioToolbox.AudioQueueAllocateBuffer(_audioQueue, (uint)bufferSize, out var buffer);

            if (status != 0)
            {
                throw new AudioPlayerException($"Failed to allocate buffer: {status}", status);
            }

            buffers.Add(buffer);
        }

        return buffers;
    }

    [UnmanagedCallersOnly]
    private static void AudioQueueOutputCallback(IntPtr userData, IntPtr audioQueue, IntPtr buffer)
    {
        var gch = GCHandle.FromIntPtr(userData);

        if (gch.Target is AudioQueuePlayer player)
        {
            player._availableAudioBuffers.Writer.TryWrite(buffer);
        }
    }

    private void ReleaseUnmanagedResources()
    {
        if (_audioQueue == IntPtr.Zero)
        {
            return;
        }

        if (_isStarted)
        {
            Stop();
        }

        foreach (var buffer in _allocatedAudioBuffers)
        {
            _ = AudioToolbox.AudioQueueFreeBuffer(_audioQueue, buffer);
        }

        _allocatedAudioBuffers.Clear();

        _ = AudioToolbox.AudioQueueDispose(_audioQueue, true);
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