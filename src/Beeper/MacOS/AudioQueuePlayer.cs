using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Channels;
using OldBit.Beeper.Helpers;
using OldBit.Beeper.MacOS.AudioToolboxInterop;

namespace OldBit.Beeper.MacOS;

/// <summary>
/// Represents an audio player that uses the Audio Queue Services API to play audio data.
/// </summary>
[SupportedOSPlatform("macos")]
internal sealed class AudioQueuePlayer: IAudioPlayer
{
    private const int MaxBuffers = 4;

    private readonly IntPtr _audioQueue;
    private readonly List<IntPtr> _allocatedAudioBuffers;
    private readonly Channel<IntPtr> _availableAudioBuffers;
    private GCHandle _gch;
    private bool _isStarted;

    internal static int BufferSize => 12288;

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
        _allocatedAudioBuffers = AudioQueueAllocateBuffers(BufferSize);

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

    public async Task Enqueue(float[] data, CancellationToken cancellationToken = default)
    {
        if (_isStarted == false)
        {
            throw new InvalidOperationException("The audio player is not started. Use the Start method to start the player.");
        }

        var buffer = await _availableAudioBuffers.Reader.ReadAsync(cancellationToken);

        unsafe
        {
            var audioQueueBuffer = (AudioQueueBuffer*)buffer;
            audioQueueBuffer->AudioDataByteSize = (uint)(data.Length * AudioFormatHelper.FloatSizeInBytes);

            Marshal.Copy(data, 0, audioQueueBuffer->AudioData, data.Length);

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
        BytesPerPacket = (uint)(channelCount * AudioFormatHelper.FloatSizeInBytes),
        FramesPerPacket = 1,
        BytesPerFrame = (uint)(channelCount * AudioFormatHelper.FloatSizeInBytes),
        ChannelsPerFrame = (uint)channelCount,
        BitsPerChannel = AudioFormatHelper.FloatSizeInBytes * 8
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