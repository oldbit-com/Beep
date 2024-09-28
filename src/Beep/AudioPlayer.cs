using OldBit.Beep.Platforms.MacOS;
using OldBit.Beep.Platforms.Windows;
using OldBit.Beep.Readers;

namespace OldBit.Beep;

/// <summary>
/// Represents a simple multiplatform audio player.
/// </summary>
public class AudioPlayer : IDisposable
{
    private readonly AudioFormat _audioFormat;
    private readonly IAudioPlayer _audioPlayer;
    private int _volume = 50;
    private bool _isStarted;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioPlayer"/> class with specified audio format, sample rate, channel count, and player options.
    /// </summary>
    /// <param name="audioFormat">The audio format to be used by the audio player.</param>
    /// <param name="sampleRate">The sample rate of the audio data. Defaults to 44100 Hz.</param>
    /// <param name="channelCount">The number of audio channels. Defaults to 2 for stereo sound.</param>
    /// <param name="playerOptions">Optional player options to customize the behavior of the audio player. If not provided, default options are used.</param>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current platform is not supported.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when arguments are not valid.</exception>
    public AudioPlayer(AudioFormat audioFormat, int sampleRate = 44100, int channelCount = 2, PlayerOptions? playerOptions = null)
    {
        playerOptions?.ThrowIfNotValid();

        _audioFormat = audioFormat;

        IAudioPlayer audioPlayer;
        if (OperatingSystem.IsMacOS())
        {
            audioPlayer = new AudioQueuePlayer(sampleRate, channelCount, playerOptions ?? PlayerOptions.Default);
        }
        else if (OperatingSystem.IsWindows())
        {
            audioPlayer = new CoreAudioPlayer(sampleRate, channelCount, playerOptions ?? PlayerOptions.Default);
        }
        else
        {
            throw new PlatformNotSupportedException($"The current platform is not supported.");
        }

        _audioPlayer = audioPlayer;
    }

    /// <summary>
    /// Gets or sets the volume of the audio player. The volume must be between 1 and 100.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int Volume
    {
        get => _volume;
        set
        {
            if (value is < 0 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(Volume), "The volume must be between 0 and 100.");
            }
            _volume = value;
        }
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
        pcmDataReader.VolumeFilter.Volume = _volume;

        await _audioPlayer.PlayAsync(pcmDataReader, cancellationToken);
    }

    /// <summary>
    /// Enqueues the audio data to be played.
    /// </summary>
    /// <param name="data">The audio data to enqueue.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. </param>
    /// <exception cref="AudioPlayerException">Thrown if player is not started.</exception>
    public async Task EnqueueAsync(IEnumerable<byte> data, CancellationToken cancellationToken = default)
    {
        if (!_isStarted)
        {
            throw new AudioPlayerException("The audio player is not started. You need to call the Start method before enqueuing audio data.");
        }

        using var pcmDataReader = new PcmDataReader(new ByteStream(data), _audioFormat);
        pcmDataReader.VolumeFilter.Volume = _volume;

        await _audioPlayer.EnqueueAsync(pcmDataReader, cancellationToken);
    }

    /// <summary>
    /// Starts the audio player.
    /// </summary>
    public void Start()
    {
        if (_isStarted)
        {
            return;
        }

        _audioPlayer.Start();
        _isStarted = true;
    }

    /// <summary>
    /// Stops the audio player.
    /// </summary>
    public void Stop()
    {
        if (!_isStarted)
        {
            return;
        }

        _audioPlayer.Stop();
        _isStarted = false;
    }

    /// <summary>
    /// Pauses the audio player.
    /// </summary>
    public void Pause() => _audioPlayer.Pause();

    /// <summary>
    /// Resumes the audio player.
    /// </summary>
    public void Resume() => _audioPlayer.Resume();

    public void Dispose()
    {
        _audioPlayer.Dispose();
        GC.SuppressFinalize(this);
    }
}