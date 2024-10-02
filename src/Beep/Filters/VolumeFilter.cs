namespace OldBit.Beep.Filters;

internal sealed class VolumeFilter : IAudioFilter
{
    private readonly int _volume;

    internal VolumeFilter(int volume) => _volume = volume;

    public float Apply(float value)
    {
        var normalizedVolume = _volume / 100.0f;

        var adjustedValue = value * normalizedVolume;
        adjustedValue = Math.Clamp(adjustedValue, -1.0f, 1.0f);

        return adjustedValue;
    }
}