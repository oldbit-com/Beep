namespace OldBit.Beep.Filters;

internal sealed class VolumeFilter : IAudioFilter
{
    internal int Volume { get; set; }

    internal VolumeFilter(int volume) => Volume = volume;

    public float Apply(float value)
    {
        var normalizedVolume = Volume / 100.0f;

        var adjustedValue = value * normalizedVolume;
        adjustedValue = Math.Clamp(adjustedValue, -1.0f, 1.0f);

        return adjustedValue;
    }
}