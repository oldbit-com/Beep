namespace OldBit.Beeper.Helpers;

/// <summary>
/// Converts PCM data to floats.
/// </summary>
internal static class PcmDataConverter
{
    internal static IEnumerable<float> ToFloats(AudioFormat format, byte[] data)
    {
        var increment = AudioFormatHelper.GetByteSize(format);

        for (var i = 0; i < data.Length; i += increment)
        {
            var sampleValue = format switch
            {
                AudioFormat.Unsigned8Bit =>
                    data[i] - 128 / 128f,

                AudioFormat.Signed16BitIntegerLittleEndian =>
                    (short)(data[i] | data[i + 1] << 8) / 32768f,

                AudioFormat.Float32BitLittleEndian =>
                    BitConverter.ToSingle([data[i], data[i + 1], data[i + 2], data[i + 3]], 0),

                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };

            yield return sampleValue;
        }
    }
}