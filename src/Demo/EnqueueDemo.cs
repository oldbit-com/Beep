using Demo.Generator;
using OldBit.Beep;

namespace Demo;

public class EnqueueDemo(AudioFormat audioFormat, int sampleRate, int channelCount, WaveType waveType, int volume) : IDemoPlayer
{
    public async Task PlayAsync()
    {
        var waveGenerator = WaveGeneratorFactory.CreateWaveGenerator(audioFormat, waveType, sampleRate, channelCount);
        var audioData = waveGenerator.Generate(659.25f, TimeSpan.FromSeconds(1));

        var audioPlayer = new AudioPlayer(audioFormat, sampleRate, channelCount, new PlayerOptions { BufferSizeInBytes = 4096 });
        audioPlayer.Volume = volume;

        audioPlayer.Start();

        var sampleSize = audioFormat switch
        {
            AudioFormat.Signed16BitIntegerLittleEndian => 2,
            AudioFormat.Float32BitLittleEndian => 4,
            _ => 1
        };

        var chunks = audioData.Chunk(768 * sampleSize);
        foreach (var chunk in chunks)
        {
            await audioPlayer.EnqueueAsync(chunk);
        }

        await Task.Delay(2000);

        audioData = waveGenerator.Generate(800.25f, TimeSpan.FromSeconds(1));

        await audioPlayer.EnqueueAsync(audioData);

        await Task.Delay(20000);

        await audioPlayer.EnqueueAsync(audioData);

        audioPlayer.Stop();
    }
}