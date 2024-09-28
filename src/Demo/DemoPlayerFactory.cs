using Demo.Generator;
using OldBit.Beep;

namespace Demo;

public class DemoPlayerFactory(AudioFormat audioFormat, int sampleRate, int channelCount, WaveType waveType, int volume)
{
    public IDemoPlayer CreateDemoPlayer(DemoType demoType)
    {
        return demoType switch
        {
            DemoType.Harmony => new DemoPlayer(audioFormat, sampleRate, channelCount, waveType, volume),
            DemoType.Enqueue => new EnqueueDemo(audioFormat, sampleRate, channelCount, waveType, volume),
            _ => throw new ArgumentOutOfRangeException(nameof(demoType), demoType, null)
        };
    }
}