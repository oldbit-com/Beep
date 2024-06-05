using OldBit.Beep.Platforms.MacOS;
using OldBit.Beep.Platforms.Windows;
using OldBit.Beep.Readers;

namespace OldBit.Beep;

public class AudioPlayer : IDisposable
{
    private readonly AudioFormat _audioFormat;
    private readonly IAudioPlayer _audioPlayer;

    public AudioPlayer(AudioFormat audioFormat, int sampleRate = 44100, int channelCount = 2)
    {
        _audioFormat = audioFormat;

        IAudioPlayer audioPlayer;
        if (OperatingSystem.IsMacOS())
        {
            audioPlayer = new AudioQueuePlayer(sampleRate, channelCount);
        }
        else if (OperatingSystem.IsWindows())
        {
            audioPlayer = new CoreAudioPlayer(sampleRate, channelCount);
        }
        else
        {
            throw new PlatformNotSupportedException($"The current platform is not supported.");
        }

        _audioPlayer = audioPlayer;
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

    /// <summary>
    /// Plays the audio data.
    /// </summary>
    /// <param name="data">The audio data to play. This is an enumerable collection of bytes.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public Task PlayAsync(IEnumerable<byte> data, CancellationToken cancellationToken = default) =>
        PlayAsync(new ByteStream(data), cancellationToken);

    /// <summary>
    /// Plays the audio data from a stream.
    /// </summary>
    /// <param name="stream">The stream containing audio data to play. This is a Stream object.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. </param>
    public async Task PlayAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var pcmDataReader = new PcmDataReader(stream, _audioFormat);

        await _audioPlayer.PlayAsync(pcmDataReader, cancellationToken);
    }

    public void Dispose()
    {
        _audioPlayer.Dispose();
        GC.SuppressFinalize(this);
    }
}