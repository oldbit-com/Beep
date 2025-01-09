using OldBit.Beep.Filters;
using OldBit.Beep.Pcm;
using OldBit.Beep.Platforms.Linux;
using OldBit.Beep.Platforms.MacOS;
using OldBit.Beep.Platforms.Windows;

namespace OldBit.Beep;

/// <summary>
/// Represents a simple multiplatform audio player.
/// </summary>
public class AudioPlayer : IDisposable
{
    private readonly IAudioPlayer _audioPlayer;
    private readonly VolumeFilter _volumeFilter;
    private readonly PcmDataReaderPool _pcmDataReaderPool;
    private bool _isStarted;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioPlayer"/> class with specified audio format, sample rate, channel count, and player options.
    /// </summary>
    /// <param name="audioFormat">The audio format to be used by the audio player.</param>
    /// <param name="sampleRate">The sample rate of the audio data. Defaults to 44100 Hz.</param>
    /// <param name="channelCount">The number of audio channels. Defaults to 2 for stereo sound.</param>
    /// <param name="playerOptions">Optional player options to customize the behavior of the audio player. If not provided, default options are used.</param>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current platform is not supported. </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when arguments are not valid.</exception>
    public AudioPlayer(AudioFormat audioFormat, int sampleRate = 44100, int channelCount = 2, PlayerOptions? playerOptions = null)
    {
        playerOptions?.ThrowIfNotValid();
        playerOptions ??= PlayerOptions.Default;

        _volumeFilter = new VolumeFilter(50);
        _pcmDataReaderPool = new PcmDataReaderPool(playerOptions.BufferQueueSize, audioFormat, _volumeFilter);

        IAudioPlayer audioPlayer;

        if (OperatingSystem.IsMacOS())
        {
            audioPlayer = new AudioQueuePlayer(sampleRate, channelCount, playerOptions);
        }
        else if (OperatingSystem.IsWindows())
        {
            audioPlayer = new CoreAudioPlayer(sampleRate, channelCount, playerOptions);
        }
        else if (OperatingSystem.IsLinux())
        {
            audioPlayer = new AlsaPlayer(sampleRate, channelCount, playerOptions);
        }
        else
        {
            if (playerOptions.ThrowOnUnsupportedPlatform)
            {
                throw new PlatformNotSupportedException($"The {Environment.OSVersion.VersionString} platform is not supported.");
            }

            audioPlayer = new SilentAudioPlayer();
        }

        _audioPlayer = audioPlayer;
    }

    /// <summary>
    /// Gets or sets the volume of the audio player. The volume must be between 1 and 100.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int Volume
    {
        get => _volumeFilter.Volume;
        set
        {
            if (value is < 0 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(Volume), "The volume must be between 0 and 100.");
            }

            _volumeFilter.Volume = value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current platform is supported.
    /// </summary>
    public bool IsPlatformSupported => _audioPlayer is not SilentAudioPlayer;

    /// <summary>
    /// Enqueues the audio data to be played.
    /// </summary>
    /// <param name="data">The audio data to enqueue.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public async Task EnqueueAsync(IEnumerable<byte> data, CancellationToken cancellationToken = default)
    {
        var reader = _pcmDataReaderPool.GetReader(data);

        await _audioPlayer.EnqueueAsync(reader, cancellationToken);
    }

    /// <summary>
    /// Attempts to enqueue the audio data to be played.
    /// </summary>
    /// <param name="data">The audio data to enqueue.</param>
    /// <returns>
    /// true if the audio data was successfully enqueued; otherwise, false.
    /// </returns>
    public bool TryEnqueue(IEnumerable<byte> data)
    {
        var reader = _pcmDataReaderPool.GetReader(data);

        return _audioPlayer.TryEnqueue(reader);
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

    public void Dispose()
    {
        _audioPlayer.Dispose();
        GC.SuppressFinalize(this);
    }
}