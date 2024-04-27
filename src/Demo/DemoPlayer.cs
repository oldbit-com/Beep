using Demo.Generator;
using OldBit.Beeper;

namespace Demo;

/// <summary>
/// Demonstrates how to play a sequence of notes.
/// </summary>
public class DemoPlayer(string audioFormatString, int sampleRate, int channelCount)
{
    private const float NoteC5 = 523.25f;
    private const float NoteE5 = 659.25f;
    private const float NoteG5 = 783.99f;

    private readonly AudioFormat _audioFormat = GetAudioFormat(audioFormatString);

    public async Task Play()
    {
        var playerC5 = Task.Run(async () => { await StartPlayer(NoteC5); });

        await Task.Delay(TimeSpan.FromSeconds(1));
        var playerE5 = Task.Run(async () => { await StartPlayer(NoteE5); });

        await Task.Delay(TimeSpan.FromSeconds(1));
        var playerG5 = Task.Run(async () => { await StartPlayer(NoteG5); });

        await Task.WhenAll(playerC5, playerE5, playerG5);
    }

    private async Task StartPlayer(float note)
    {
        var sinWaveGenerator = new SinWaveGenerator(_audioFormat, sampleRate, channelCount);
        var audioData = sinWaveGenerator.Generate(note, TimeSpan.FromSeconds(3));

        using var audioPlayer = new AudioPlayer(_audioFormat, sampleRate, channelCount);

        audioPlayer.Start();
        await audioPlayer.Play(audioData);
        audioPlayer.Stop();
    }

    private static AudioFormat GetAudioFormat(string audioFormat) => audioFormat switch
    {
        "u8" => AudioFormat.Unsigned8Bit,
        "s16le" => AudioFormat.Signed16BitIntegerLittleEndian,
        "f32le" => AudioFormat.Float32BitLittleEndian,
        _ => throw new NotSupportedException("The audio format is not supported.")
    };
}