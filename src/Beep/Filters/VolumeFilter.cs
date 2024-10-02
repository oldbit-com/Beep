namespace OldBit.Beep.Filters;

internal class VolumeFilter : IAudioFilter
{
    internal int Volume { get; set; }

    public float Apply(float value)
    {
        var normalizedVolume = Volume / 100.0f;

        var adjustedSample = value * normalizedVolume;
        adjustedSample = Math.Clamp(adjustedSample, -1.0f, 1.0f);

        return adjustedSample;
    }
}