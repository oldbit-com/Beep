namespace OldBit.Beeper.Extensions;

public static class AudioFormatExtensions
{
    /// <summary>
    /// Gets the byte size of a sample for the specified audio format.
    /// </summary>
    /// <param name="format">The audio format.</param>
    /// <returns>The byte size of a sample for the specified audio format.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported audio format is provided.</exception>
    public static int GetByteSize(this AudioFormat format) => format switch
    {
        AudioFormat.Unsigned8Bit => 1,
        AudioFormat.Signed16BitIntegerLittleEndian => 2,
        AudioFormat.Float32BitLittleEndian => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}