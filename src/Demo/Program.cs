using System.CommandLine;
using Demo;
using OldBit.Beeper.Windows;


var xxx = new CoreAudioPlayer();
xxx.Start();

var formatOption = new Option<string>("--format", () => "f32le", "The format of the output file")
    .FromAmong("f32le", "s16le", "u8");
formatOption.AddAlias("-f");

var sampleRateOption = new Option<int>("--rate", () => 44100, "The sample rate");
sampleRateOption.AddAlias("-r");

var channelsOption = new Option<int>("--channels", () => 2, "The number of channels");
channelsOption.AddAlias("-c");

var rootCommand = new RootCommand();
rootCommand.AddOption(formatOption);
rootCommand.AddOption(sampleRateOption);
rootCommand.AddOption(channelsOption);

rootCommand.SetHandler(async (format, sampleRate, channels) =>
{
    Console.WriteLine("Playing demo audio...");
    Console.WriteLine($"Format: {format}  Sample rate: {sampleRate}  Channels: {channels}");

    var demoPlayer = new DemoPlayer(format, sampleRate, channels);
    await demoPlayer.Play();

}, formatOption, sampleRateOption, channelsOption);

await rootCommand.InvokeAsync(args);