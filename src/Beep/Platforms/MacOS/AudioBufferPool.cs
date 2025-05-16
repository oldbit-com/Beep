using System.Threading.Channels;
using OldBit.Beep.Platforms.MacOS.AudioToolboxInterop;

namespace OldBit.Beep.Platforms.MacOS;

internal sealed class AudioBufferPool : IDisposable
{
    private readonly IntPtr _audioQueue;
    private readonly PlayerOptions _playerOptions;
    private readonly Dictionary<IntPtr, AudioBuffer> _audioQueueBuffers;
    private readonly Channel<AudioBuffer> _availableAudioBuffersQueue;

    internal event EventHandler<PlayerErrorEventArgs>? OnError;

    internal AudioBufferPool(IntPtr audioQueue, PlayerOptions playerOptions)
    {
        _audioQueue = audioQueue;
        _playerOptions = playerOptions;

        _availableAudioBuffersQueue = Channel.CreateBounded<AudioBuffer>(new BoundedChannelOptions(playerOptions.MaxBuffers)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        _audioQueueBuffers = AudioQueueAllocateBuffers(_playerOptions.BufferSizeInBytes).ToDictionary(buffer => buffer.Pointer);

        foreach (var buffer in _audioQueueBuffers)
        {
            _availableAudioBuffersQueue.Writer.TryWrite(buffer.Value);
        }
    }

    internal async ValueTask<AudioBuffer> GetBufferAsync(CancellationToken cancellationToken)
    {
        var buffer = await _availableAudioBuffersQueue.Reader.ReadAsync(cancellationToken);

        while (buffer.IsStale)
        {
            ResetBuffer(buffer);

            buffer = await _availableAudioBuffersQueue.Reader.ReadAsync(cancellationToken);
        }

        return buffer;
    }

    internal void MakeBufferAvailable(IntPtr bufferPtr)
    {
        if (!_audioQueueBuffers.TryGetValue(bufferPtr, out var audioQueueBuffer))
        {
            return;
        }

        audioQueueBuffer.LastAccessTime = DateTimeOffset.UtcNow;
        _availableAudioBuffersQueue.Writer.TryWrite(audioQueueBuffer);
    }

    private void AudioQueueFreeBuffers()
    {
        foreach (var buffer in _audioQueueBuffers)
        {
            var status = AudioToolbox.AudioQueueFreeBuffer(_audioQueue, buffer.Value.Pointer);

            if (status != 0)
            {
                OnError?.Invoke(this, new PlayerErrorEventArgs(new AudioPlayerException($"Failed to free audio buffer: {status}", status)));
            }
        }

        _audioQueueBuffers.Clear();
    }

    private List<AudioBuffer> AudioQueueAllocateBuffers(int bufferSize)
    {
        var buffers = new List<AudioBuffer>();

        for (var i = 0; i < _playerOptions.MaxBuffers; i++)
        {
            var status = AudioToolbox.AudioQueueAllocateBuffer(_audioQueue, (uint)bufferSize, out var buffer);

            if (status != 0)
            {
                throw new AudioPlayerException($"Failed to allocate buffer: {status}", status);
            }

            buffers.Add(new AudioBuffer(buffer));
        }

        return buffers;
    }

    private void ResetBuffer(AudioBuffer staleBuffer)
    {
        var status = AudioToolbox.AudioQueueFreeBuffer(_audioQueue, staleBuffer.Pointer);

        if (status != 0)
        {
            OnError?.Invoke(this, new PlayerErrorEventArgs(new AudioPlayerException($"Failed to free audio buffer: {status}", status)));
        }

        status = AudioToolbox.AudioQueueAllocateBuffer(_audioQueue, (uint)_playerOptions.BufferSizeInBytes, out var buffer);

        if (status != 0)
        {
            OnError?.Invoke(this, new PlayerErrorEventArgs(new AudioPlayerException($"Failed to allocate buffer: {status}", status)));
            return;
        }

        var newBuffer = new AudioBuffer(buffer);

        _audioQueueBuffers.Remove(staleBuffer.Pointer);
        _audioQueueBuffers.Add(buffer, newBuffer);

        _availableAudioBuffersQueue.Writer.TryWrite(newBuffer);
    }

    public void Dispose() => AudioQueueFreeBuffers();
}