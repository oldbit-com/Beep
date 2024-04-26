using Demo.Generator;
using OldBit.Beeper;

namespace Demo;

public class DemoPlayer
{
    private const float NoteC5 = 523.25f;
    private const float NoteE5 = 659.25f;
    private const float NoteG5 = 783.99f;

    public async Task Play(string audioFormatString, int sampleRate, int channelCount)
    {
        var audioFormat = GetAudioFormat(audioFormatString);

        var sinWaveGenerator = new SinWaveGenerator(audioFormat, sampleRate, channelCount);
        var audioData = sinWaveGenerator.Generate(440, TimeSpan.FromSeconds(1));

        var audioPlayer = new AudioPlayer(audioFormat, sampleRate, channelCount);

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