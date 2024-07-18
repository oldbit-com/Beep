using OldBit.Beep;

namespace Demo.Generator;

public static class WaveGeneratorFactory
{
    public static IWaveGenerator CreateWaveGenerator(AudioFormat format, WaveType waveType, int sampleRate = 44100, int channelCount = 2)
    {
        return waveType switch
        {
            WaveType.Sine => new SineWaveGenerator(format, sampleRate, channelCount),
            WaveType.Square => new SquareWaveGenerator(format, sampleRate, channelCount),
            _ => throw new ArgumentOutOfRangeException(nameof(waveType), waveType, null)
        };
    }
}