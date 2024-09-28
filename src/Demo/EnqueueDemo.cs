using Demo.Generator;
using OldBit.Beep;

namespace Demo;

public class EnqueueDemo(AudioFormat audioFormat, int sampleRate, int channelCount, WaveType waveType, int volume) : IDemoPlayer
{
    public async Task PlayAsync()
    {
        var waveGenerator = WaveGeneratorFactory.CreateWaveGenerator(audioFormat, waveType, sampleRate, channelCount);
        var audioData = waveGenerator.Generate(659.25f, TimeSpan.FromSeconds(3));

        using var audioPlayer = new AudioPlayer(audioFormat, sampleRate, channelCount);
        audioPlayer.Volume = volume;

        audioPlayer.Start();

        var chunks = audioData.Chunk(768);
        foreach (var chunk in chunks)
        {
            await audioPlayer.EnqueueAsync(chunk);
        }

        audioPlayer.Stop();
    }
}