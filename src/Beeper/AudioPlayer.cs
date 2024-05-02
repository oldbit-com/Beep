using OldBit.Beeper.IO;
using OldBit.Beeper.MacOS;
using OldBit.Beeper.Windows;

namespace OldBit.Beeper;

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

    public async Task Play(Stream stream, CancellationToken cancellationToken = default)
    {
        using var pcmDataReader = new PcmDataReader(stream, _audioFormat);

        await _audioPlayer.Play(pcmDataReader, cancellationToken);
    }

    public Task Play(IEnumerable<byte> data, CancellationToken cancellationToken = default) =>
        Play(new ByteStream(data), cancellationToken);

    public void Dispose()
    {
        _audioPlayer.Dispose();
        GC.SuppressFinalize(this);
    }
}