# Beep

Beep is a simple cross platform low level dotnet library for playing PCM audio data. It is also a good starting point for creating 
more advanced audio libraries. 

It was inspired by [oto](https://github.com/ebitengine/oto) golang library that I used before. However, it is not a direct port of it. It has been written from scratch in C# and .net 8, using async/await pattern.

It is quite hard to find a decent examples of how to integrate native OS audio frameworks, especially that these frameworks are not so easy to use.

## Features
- no external dependencies other than native OS frameworks 
- cross platform, currently supports MacOS and Windows
- supports 8-bit unsigned, 16-bit signed and 32-bit float PCM audio
- simple volume control

### Supported platforms:
- [MacOS](#MacOS)
- [Windows](#Windows)
- Linux TBD

#### MacOS
Audio playback is implemented using [AudioToolbox.framework](https://developer.apple.com/documentation/audiotoolbox).

#### Windows
Audio playback is implemented using [WASAPI](https://docs.microsoft.com/en-us/windows/win32/coreaudio/wasapi).

#### Linux
TBD later, time permitting

## Usage

### Demo app
Please check [Demo](src/Demo) project for an example how to use this library.

### Code
```csharp
using OldBit.Beep;

using var audioPlayer = new AudioPlayer(AudioFormat.Float32BitLittleEndian, 44100, 2);
audioPlayer.Volume = 50;  // That is the default value, valid values are 1-100

await audioPlayer.PlayAsync(new byte[] { 0.5f, 0.5f, 0.5f, 0.5f });
```
`PlayAsync` method takes a byte array of audio data or a Stream of bytes.
The data should be in the format specified in the constructor.

The following data formats are supported:
- `Unsigned8BitLittleEndian` - 8-bit unsigned PCM
- `Signed16BitLittleEndian` - 16-bit signed PCM
- `Float32BitLittleEndian` - 32-bit float PCM
