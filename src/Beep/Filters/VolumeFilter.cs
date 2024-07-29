namespace OldBit.Beep.Filters;

public class VolumeFilter
{
    internal int Volume { get; set; }

    internal byte Apply(byte value) => AdjustUnsigned8BitVolume(value);

    internal short Apply(short value) => AdjustSigned16BitIntegerVolume(value);

    internal float Apply(float value) => AdjustFloat32BitVolume(value);

    private  byte AdjustUnsigned8BitVolume(byte sample)
    {
        var normalizedVolume = Volume / 100.0;

        var adjustedSample = sample - 128;
        adjustedSample = (int)(adjustedSample * normalizedVolume);
        adjustedSample = Math.Clamp(adjustedSample + 128, 0, 255);

        return (byte)adjustedSample;
    }

    private short AdjustSigned16BitIntegerVolume(short sample)
    {
        var normalizedVolume = Volume / 100.0;

        var adjustedSample = (int)(sample * normalizedVolume);
        adjustedSample = Math.Clamp(adjustedSample, short.MinValue, short.MaxValue);

        return (short)adjustedSample;
    }

    private float AdjustFloat32BitVolume(float sample)
    {
        var normalizedVolume = Volume / 100.0f;

        var adjustedSample = sample * normalizedVolume;
        adjustedSample = Math.Clamp(adjustedSample, -1.0f, 1.0f);

        return adjustedSample;
    }
}