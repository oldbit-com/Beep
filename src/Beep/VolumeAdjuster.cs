namespace OldBit.Beep;

internal static class VolumeAdjuster
{
    internal static byte Adjust(byte value, int volume) => AdjustUnsigned8Bit(value, volume);

    internal static short Adjust(short value, int volume) => AdjustSigned16BitInteger(value, volume);

    internal static float Adjust(float value, int volume) => AdjustFloat32Bit(value, volume);

    private static byte AdjustUnsigned8Bit(byte sample, int volume)
    {
        var normalizedVolume = volume / 100.0;

        var adjustedSample = sample - 128;
        adjustedSample = (int)(adjustedSample * normalizedVolume);
        adjustedSample = Math.Clamp(adjustedSample + 128, 0, 255);

        return (byte)adjustedSample;
    }

    private static short AdjustSigned16BitInteger(short sample, int volume)
    {
        var normalizedVolume = volume / 100.0;

        var adjustedSample = (int)(sample * normalizedVolume);
        adjustedSample = Math.Clamp(adjustedSample, short.MinValue, short.MaxValue);

        return (short)adjustedSample;
    }

    private static float AdjustFloat32Bit(float sample, int volume)
    {
        var normalizedVolume = volume / 100.0f;

        var adjustedSample = sample * normalizedVolume;
        adjustedSample = Math.Clamp(adjustedSample, -1.0f, 1.0f);

        return adjustedSample;
    }
}