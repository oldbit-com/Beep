using System.CommandLine;
using System.Diagnostics;
using Demo;
using Demo.Generator;
using OldBit.Beep;

var audioFormatOption = new Option<string>("--format", () => "f32le", "The format of the output file")
    .FromAmong("f32le", "s16le", "u8");
audioFormatOption.AddAlias("-f");

var sampleRateOption = new Option<int>("--rate", () => 44100, "The sample rate");
sampleRateOption.AddAlias("-r");

var channelsOption = new Option<int>("--channels", () => 2, "The number of channels");
channelsOption.AddAlias("-c");

var waveOption = new Option<string>("--wave", () => "sine", "The type of waive to generate")
    .FromAmong("sine", "square");
waveOption.AddAlias("-w");

var volumeOption = new Option<int>("--vol", () => 50, "The volume level");
volumeOption.AddValidator(result =>
{
    var volume = result.GetValueForOption(volumeOption);
    if (volume is < 1 or > 100)
    {
        result.ErrorMessage = "Volume must be between 1 and 100";
    }
});
volumeOption.AddAlias("-v");

var rootCommand = new RootCommand("Plays a demo audio.");
rootCommand.AddOption(audioFormatOption);
rootCommand.AddOption(sampleRateOption);
rootCommand.AddOption(channelsOption);
rootCommand.AddOption(waveOption);
rootCommand.AddOption(volumeOption);

rootCommand.SetHandler(async (audioFormat, sampleRate, channels, waveType, volume) =>
{
    Console.WriteLine("Playing demo audio...");
    Console.WriteLine($"Format: {audioFormat}  Sample rate: {sampleRate}  Channels: {channels}  WaveType: {waveType}  Volume: {volume}");

    var parsedAudioFormat = audioFormat switch
    {
        "u8" => AudioFormat.Unsigned8Bit,
        "s16le" => AudioFormat.Signed16BitIntegerLittleEndian,
        "f32le" => AudioFormat.Float32BitLittleEndian,
        _ => throw new NotSupportedException("The audio format is not supported.")
    };
    var parsedWaveType = Enum.Parse<WaveType>(waveType, ignoreCase: true);

    var demoPlayerFactory = new DemoPlayerFactory(parsedAudioFormat, sampleRate, channels, parsedWaveType, volume);
    var demoPlayer = demoPlayerFactory.CreateDemoPlayer(DemoType.DefaultDemo);

    var timer = Stopwatch.StartNew();
    await demoPlayer.PlayAsync();

    Console.WriteLine($"Finished playing audio in {timer.ElapsedMilliseconds}ms.");

}, audioFormatOption, sampleRateOption, channelsOption, waveOption, volumeOption);

await rootCommand.InvokeAsync(args);