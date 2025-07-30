using System.CommandLine;
using System.Diagnostics;
using Demo;
using Demo.Generator;
using OldBit.Beep;

var audioFormatOption = new Option<string>("--format", "-f")
    {
        Description = "The format of the output file",
        DefaultValueFactory = _ => "f32le"
    }
    .AcceptOnlyFromAmong("f32le", "s16le", "u8");

var sampleRateOption = new Option<int>("--rate", "-r")
{
    Description = "The sample rate",
    DefaultValueFactory = _ => 44100
};

var channelsOption = new Option<int>("--channels", "-c")
{
    Description = "The number of channels",
    DefaultValueFactory = _ => 2
};

var waveOption = new Option<string>("--wave", "-w")
    {
        Description = "The type of wave to generate",
        DefaultValueFactory = _ => "sine"
    }
    .AcceptOnlyFromAmong("sine", "square");

var volumeOption = new Option<int>("--vol", "-v")
{
    Description = "The volume level",
    DefaultValueFactory = _ => 50
};
volumeOption.Validators.Add(result =>
{
    var volume = result.GetValue(volumeOption);

    if (volume is < 0 or > 100)
    {
        result.AddError("Volume must be between 0 and 100");
    }
});

var rootCommand = new RootCommand("Plays a demo audio.");
rootCommand.Options.Add(audioFormatOption);
rootCommand.Options.Add(sampleRateOption);
rootCommand.Options.Add(channelsOption);
rootCommand.Options.Add(waveOption);
rootCommand.Options.Add(volumeOption);

rootCommand.SetAction(async parseResult =>
{
    var audioFormat = parseResult.GetRequiredValue(audioFormatOption);
    var sampleRate = parseResult.GetRequiredValue(sampleRateOption);
    var channels = parseResult.GetRequiredValue(channelsOption);
    var waveType = parseResult.GetRequiredValue(waveOption);
    var volume = parseResult.GetRequiredValue(volumeOption);

    Console.WriteLine("Playing demo audio...");
    Console.WriteLine($"Format: {audioFormat} | Sample rate: {sampleRate} | Channels: {channels} | WaveType: {waveType} | Volume: {volume}");

    var parsedAudioFormat = audioFormat switch
    {
        "u8" => AudioFormat.Unsigned8Bit,
        "s16le" => AudioFormat.Signed16BitIntegerLittleEndian,
        "f32le" => AudioFormat.Float32BitLittleEndian,
        _ => throw new NotSupportedException("The audio format is not supported.")
    };
    var parsedWaveType = Enum.Parse<WaveType>(waveType, ignoreCase: true);

    var demoPlayer = new DemoPlayer(parsedAudioFormat, sampleRate, channels, parsedWaveType, volume);

    var timer = Stopwatch.StartNew();
    await demoPlayer.PlayAsync();

    Console.WriteLine($"Finished playing audio in {timer.ElapsedMilliseconds}ms.");
});

var parseResult = rootCommand.Parse(args);

await parseResult.InvokeAsync();
