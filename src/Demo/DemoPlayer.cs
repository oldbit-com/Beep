using Demo.Generator;
using OldBit.Beeper;

namespace Demo;

public static class DemoPlayer
{
    private const float NoteC5 = 523.25f;
    private const float NoteE5 = 659.25f;
    private const float NoteG5 = 783.99f;

    public static async Task Play(string audioFormatString, int sampleRate, int channelCount)
    {
        var audioFormat = GetAudioFormat(audioFormatString);

        var playerC5 = Task.Run(async() =>
        {
            await StartPlayer(audioFormat, sampleRate, channelCount, NoteC5);
        });

        await Task.Delay(TimeSpan.FromSeconds(1));
        var playerE5 = Task.Run(async() =>
        {
            await StartPlayer(audioFormat, sampleRate, channelCount, NoteE5);
        });

        await Task.Delay(TimeSpan.FromSeconds(1));
        var playeG5 = Task.Run(async() =>
        {
            await StartPlayer(audioFormat, sampleRate, channelCount, NoteG5);
        });

        await Task.WhenAll(playerC5, playerE5, playeG5);
    }

    private static async Task StartPlayer(AudioFormat audioFormat, int sampleRate, int channelCount, float note)
    {
        var sinWaveGenerator = new SinWaveGenerator(audioFormat, sampleRate, channelCount);
        var audioData = sinWaveGenerator.Generate(note, TimeSpan.FromSeconds(3));

        using var audioPlayer = new AudioPlayer(audioFormat, sampleRate, channelCount);

        audioPlayer.Start();
        await audioPlayer.Enqueue(audioData);
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