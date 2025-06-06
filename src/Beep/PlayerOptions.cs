namespace OldBit.Beep;

/// <summary>
/// Specifies advanced options for the audio player.
/// </summary>
public sealed class PlayerOptions
{
    internal static PlayerOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets the size of the audio buffer in bytes. Default is 1024 bytes.
    /// </summary>
    public int BufferSizeInBytes { get; set; } = 8192;

    /// <summary>
    /// Gets or sets the maximum number of audio buffers in the queue. Default is 4.
    /// </summary>
    public int BufferQueueSize { get; set; } = 4;

    /// <summary>
    /// Gets or sets the maximum number of audio buffers. Default is 4.
    /// </summary>
    /// <remarks>
    /// This setting applies to AudioToolbox (macOS) only.
    /// </remarks>
    public int MaxBuffers { get; set; } = 4;

    /// <summary>
    /// Gets or sets a value indicating whether to throw an exception when the platform is not supported.
    /// </summary>
    public bool ThrowOnUnsupportedPlatform { get; set; }

    internal void ThrowIfNotValid()
    {
        if (BufferSizeInBytes < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(BufferSizeInBytes), "The buffer size must be greater than 0.");
        }

        if (MaxBuffers < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxBuffers), "The maximum number of buffers must be greater than 0.");
        }
    }
}