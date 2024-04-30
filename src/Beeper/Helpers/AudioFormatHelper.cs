namespace OldBit.Beeper.Helpers;

public static class AudioFormatHelper
{
    /// <summary>
    /// Gets the byte size of a sample for the specified audio format.
    /// </summary>
    /// <param name="format">The audio format.</param>
    /// <returns>The byte size of a sample for the specified audio format.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported audio format is provided.</exception>
    public static int GetByteSize(AudioFormat format) => format switch
    {
        AudioFormat.Unsigned8Bit => 1,
        AudioFormat.Signed16BitIntegerLittleEndian => 2,
        AudioFormat.Float32BitLittleEndian => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };

    /// <summary>
    /// The size of a float in bytes.
    /// </summary>
    internal const int FloatSizeInBytes = sizeof(float);
}
