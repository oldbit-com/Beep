# Beep

Beep is a simple cross platform dotnet library for playing audio using PCM data. By default dotnet does not have a built in 
way to play audio and this library provides a very basic way to play audio.

It was inspired by the [oto](https://github.com/ebitengine/oto) golang library that I used before. However, it is not a direct port of it.

## Features
- no external dependencies other than native OS libraries
- cross platform, currently supports MacOS and Windows
- supports 8-bit unsigned, 16-bit signed and 32-bit float PCM audio

However this is more an example and exercise how to create cross platform libraries in dotnet with native OS dependencies.
Supported platforms:
- [MacOS](#MacOS)
- [Windows](#Windows)
- Linux TBD

### MacOS
Audio playback is implemented using It uses [AudioToolbox.framework](https://developer.apple.com/documentation/audiotoolbox).

### Windows
Audio playback is implemented using [WASAPI](https://docs.microsoft.com/en-us/windows/win32/coreaudio/wasapi).

## Usage

### Demo
Please check [Demo](src/Demo) project for an example how to use the library.

### Code
```csharp
using OldBit.Beep;

using var audioPlayer = new AudioPlayer(AudioFormat.Float32BitLittleEndian, 44100, 2);
audioPlayer.Volume = 50;  // That is the default value, valid values are 1-100

audioPlayer.Start();
await audioPlayer.EnqueueAsync(new byyte[] { 0.5f, 0.5f, 0.5f, 0.5f });
audioPlayer.Stop();
```

