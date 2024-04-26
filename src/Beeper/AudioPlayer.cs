using System.Runtime.InteropServices;
using OldBit.Beeper.Helpers;
using OldBit.Beeper.MacOS;

namespace OldBit.Beeper;

public class AudioPlayer : IDisposable
{
    private readonly AudioFormat _audioFormat;
    private readonly IAudioPlayer _audioQueuePlayer;
    private readonly int _bufferSize;

    public AudioPlayer(AudioFormat audioFormat, int sampleRate = 44100, int channelCount = 2)
    {
        _audioFormat = audioFormat;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _bufferSize = 12288;
            _audioQueuePlayer = new AudioQueuePlayer(sampleRate, channelCount, _bufferSize);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // TODO: Implement Windows audio player
            throw new PlatformNotSupportedException("The current platform is not supported.");
        }
        else
        {
            throw new PlatformNotSupportedException("The current platform is not supported.");
        }
    }

    public void Start()
    {
        _audioQueuePlayer.Start();
    }

    public void Stop()
    {
        _audioQueuePlayer.Stop();
    }

    public async Task Enqueue(IEnumerable<byte> data, CancellationToken cancellationToken = default)
    {
        var chunks = data.Chunk(_bufferSize / 4);

        foreach (var chunk in chunks)
        {
            var floats = PcmDataConverter.ToFloats(_audioFormat, chunk).ToArray();
            await _audioQueuePlayer.Enqueue(floats, cancellationToken);
        }
    }

    void IDisposable.Dispose() => _audioQueuePlayer.Dispose();
}