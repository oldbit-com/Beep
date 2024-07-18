using System.CommandLine;
using Demo;
using Demo.Generator;

var formatOption = new Option<string>("--format", () => "f32le", "The format of the output file")
    .FromAmong("f32le", "s16le", "u8");
formatOption.AddAlias("-f");

var sampleRateOption = new Option<int>("--rate", () => 44100, "The sample rate");
sampleRateOption.AddAlias("-r");

var channelsOption = new Option<int>("--channels", () => 2, "The number of channels");
channelsOption.AddAlias("-c");

var waveOption = new Option<string>("--wave", () => "sin", "The type of waive to generate")
    .FromAmong("sine", "square");
waveOption.AddAlias("-w");

var volumeOption = new Option<int>("--vol", () => 50, "The volume level");
volumeOption.AddValidator(result =>
{
    if (result.GetValueForOption(volumeOption) < 1 || result.GetValueForOption(volumeOption) > 100)
    {
        result.ErrorMessage = "Volume must be between 1 and 1000";
    }
});
volumeOption.AddAlias("-v");

var rootCommand = new RootCommand();
rootCommand.AddOption(formatOption);
rootCommand.AddOption(sampleRateOption);
rootCommand.AddOption(channelsOption);
rootCommand.AddOption(waveOption);
rootCommand.AddOption(volumeOption);

rootCommand.SetHandler(async (format, sampleRate, channels, waveType, volume) =>
{
    Console.WriteLine("Playing demo audio...");
    Console.WriteLine($"Format: {format}  Sample rate: {sampleRate}  Channels: {channels}  WaveType: {waveType}  Volume: {volume}");

    var parsedWaveType = Enum.Parse<WaveType>(waveType, ignoreCase: true);
    var demoPlayer = new DemoPlayer(format, sampleRate, channels, parsedWaveType, volume);
    await demoPlayer.PlayAsync();

}, formatOption, sampleRateOption, channelsOption, waveOption, volumeOption);

await rootCommand.InvokeAsync(args);