namespace OldBit.Beep.Extensions;

/// <summary>
/// Extensions for the <see cref="AudioFormat"/> enum.
/// </summary>
public static class AudioFormatExtensions
{
    /// <summary>
    /// Gets the byte size of the specified <see cref="AudioFormat"/>.
    /// </summary>
    /// <param name="format">The audio format.</param>
    /// <returns>The byte size of the specified audio format.</returns>
    public static int GetByteSize(this AudioFormat format) => format switch
    {
        AudioFormat.Unsigned8Bit => 1,
        AudioFormat.Signed16BitIntegerLittleEndian => 2,
        AudioFormat.Float32BitLittleEndian => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}