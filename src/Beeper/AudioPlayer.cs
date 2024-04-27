using OldBit.Beeper.Helpers;
using OldBit.Beeper.MacOS;

namespace OldBit.Beeper;

public class AudioPlayer : IDisposable
{
    private readonly AudioFormat _audioFormat;
    private readonly IAudioPlayer _audioPlayer;
    private readonly int _bufferSize;

    public AudioPlayer(AudioFormat audioFormat, int sampleRate = 44100, int channelCount = 2)
    {
        _audioFormat = audioFormat;

        if (OperatingPlatform.IsMacOS)
        {
            _bufferSize = 12288;
            _audioPlayer = new AudioQueuePlayer(sampleRate, channelCount, _bufferSize);
        }
        else if (OperatingPlatform.IsWindows)
        {
            // TODO: Implement Windows audio player
            throw new PlatformNotSupportedException("The current platform is not supported.");
        }
        else
        {
            throw new PlatformNotSupportedException($"The current platform is not supported.");
        }
    }

    /// <summary>
    /// Starts the audio player.
    /// </summary>
    public void Start()
    {
        _audioPlayer.Start();
    }

    /// <summary>
    /// Stops the audio player.
    /// </summary>
    public void Stop()
    {
        _audioPlayer.Stop();
    }

    public async Task Play(Stream stream, CancellationToken cancellationToken = default)
    {
        var buffer = new byte[_bufferSize];
        var count = await stream.ReadAsync(buffer, cancellationToken);

        while (count > 0)
        {
            await Play(buffer.Take(count), cancellationToken);
            count = await stream.ReadAsync(buffer, cancellationToken);
        }
    }

    public async Task Play(IEnumerable<byte> data, CancellationToken cancellationToken = default)
    {
        var chunks = data.Chunk(_bufferSize / 4);

        foreach (var chunk in chunks)
        {
            var floats = PcmDataConverter.ToFloats(_audioFormat, chunk).ToArray();
            await _audioPlayer.Enqueue(floats, cancellationToken);
        }
    }

    void IDisposable.Dispose() => _audioPlayer.Dispose();
}