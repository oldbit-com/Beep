using Demo.Generator;
using OldBit.Beep;
using System.Threading;

namespace Demo;

/// <summary>
/// Demonstrates how to play a sequence of notes.
/// </summary>
public class DemoPlayer(AudioFormat audioFormat, int sampleRate, int channelCount, WaveType waveType, int volume)
{
    private const float NoteC5 = 523.25f;
    private const float NoteE5 = 659.25f;
    private const float NoteG5 = 783.99f;

    public async Task PlayAsync()
    {
        var playerC5 = Task.Run(async () => { await StartPlayerAsync(NoteC5); });

        await Task.Delay(TimeSpan.FromSeconds(1));
        var playerE5 = Task.Run(async () => { await StartPlayerAsync(NoteE5); });

        await Task.Delay(TimeSpan.FromSeconds(1));
        var playerG5 = Task.Run(async () => { await StartPlayerAsync(NoteG5); });

        await Task.WhenAll(playerC5, playerE5, playerG5);
    }

    private async Task StartPlayerAsync(float note)
    {
        var waveGenerator = WaveGeneratorFactory.CreateWaveGenerator(audioFormat, waveType, sampleRate, channelCount);
        var audioData = waveGenerator.Generate(note, TimeSpan.FromSeconds(3));

        using var audioPlayer = new AudioPlayer(audioFormat, sampleRate, channelCount);
        audioPlayer.Volume = volume;

        audioPlayer.Start();

        await audioPlayer.PlayAsync(audioData);

        audioPlayer.Stop();
    }
}