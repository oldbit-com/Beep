# Beep

Beep is a simple cross platform low level dotnet library for playing PCM audio data.

It was inspired by [oto](https://github.com/ebitengine/oto) golang library that I used before. However, it is not a direct port of it.

It has been written from scratch in C# using .NET 8, and it does not depend on any other libraries, but uses native OS audio frameworks.

In general there are two main methods for playing audio:
- `PlayAsync` - allows playing audio data from a byte array or a stream until the end of the data.
  It starts and stops the audio playback automatically.
- `EnqueueAsync` - allows playing audio data chunks, player needs to be started and stopped
   programmatically. It can play small chunks of data.

## Features
- no external dependencies other than native OS frameworks 
- cross platform, currently supports MacOS and Windows
- supports 8-bit unsigned, 16-bit signed and 32-bit float PCM audio
- simple volume control

### Supported platforms:
- [MacOS](#MacOS)
- [Windows](#Windows)
- [Linux](#Linux)

#### MacOS
Audio playback is implemented using [AudioToolbox.framework](https://developer.apple.com/documentation/audiotoolbox).

#### Windows
Audio playback is implemented using [WASAPI](https://docs.microsoft.com/en-us/windows/win32/coreaudio/wasapi).

#### Linux
Audio playback is implemented using [ALSA](https://www.alsa-project.org).

You may need to install ALSA development files. Installation method may differ depending on the Linux distribution.

On Debian based Linux distributions run:
```shell
apt install libasound2-dev
```
On RedHat based Linux distributions run:

```shell
dnf install alsa-lib-devel
```

## Usage

### Demo app
Please check [Demo](src/Demo) project for an example how to use this library.

### Code
```csharp
using OldBit.Beep;

using var audioPlayer = new AudioPlayer(AudioFormat.Float32BitLittleEndian, 44100, 2);
audioPlayer.Volume = 50;  // That is the default value, valid values are 0-100

audioPlayer.Start();
await audioPlayer.EnqueueAsync(new byte[] { 0.5f, 0.5f, 0.5f, 0.5f });
audioPlayer.Stop();
```
`EnqueueAsync` method takes a byte array of audio data or a Stream of bytes.
The data should be in the format specified in the constructor.

The following data formats are supported:
- `Unsigned8BitLittleEndian` - 8-bit unsigned PCM
- `Signed16BitLittleEndian` - 16-bit signed PCM
- `Float32BitLittleEndian` - 32-bit float PCM
