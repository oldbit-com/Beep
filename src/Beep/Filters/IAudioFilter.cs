namespace OldBit.Beep.Filters;

internal interface IAudioFilter
{
    float Apply(float value);
}