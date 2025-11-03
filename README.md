# Beep Audio Player

Beep is a simple cross-platform low-level dotnet library for playing PCM audio.

It was inspired by [oto](https://github.com/ebitengine/oto) golang library that I used before. However, it is not a direct port of it.
It has been created to be used by my [ZX Spectrum emulator](https://github.com/oldbit-com/Spectron), hence the name Beep. 
I needed a simple way of playing audio and couldn't find anything that would suit my needs.

I don't plan to add any advanced features to this library. It is quite challenging to implement a good audio library 
that would work on all platforms. Each platform has its own way of handling audio and good examples are hard to find.

I've tested it on MacOS, Windows and Linux. But I can't guarantee that it will work on all systems.

## Features
- written in C# and .NET 8
- no external dependencies other than native OS frameworks
- cross-platform, currently supports macOS, Windows and Linux
- supports 8-bit unsigned, 16-bit signed and 32-bit float PCM data formats
- simple volume control

Internally, it uses 32-bit float PCM audio format (little endian).

## Platforms:
- [MacOS](#MacOS)
- [Windows](#Windows)
- [Linux](#Linux)

### MacOS
Audio playback is implemented using [AudioToolbox.framework](https://developer.apple.com/documentation/audiotoolbox). The framework is available on macOS by default.

### Windows
Audio playback is implemented using [WASAPI](https://docs.microsoft.com/en-us/windows/win32/coreaudio/wasapi). The framework is available on Windows by default.

### Linux
Audio playback is implemented using [ALSA](https://www.alsa-project.org). The library may need to be installed on some Linux distributions.

Additionally, you might need ALSA development library. Installation method may differ depending on the Linux distribution.

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
Please check the [Demo](src/Demo) project for an example how to use `Beep`.

### Code
```csharp
using OldBit.Beep;

using var audioPlayer = new AudioPlayer(AudioFormat.Float32BitLittleEndian, 44100, 2);
audioPlayer.Volume = 50;

audioPlayer.Start();
await audioPlayer.EnqueueAsync(new byte[] { 0.5f, 0.5f, 0.5f, 0.5f });
audioPlayer.Stop();
```

`EnqueueAsync` method takes an array of audio bytes. The format is one of the following:

| Format                     | Description        | Size                    |
|----------------------------|--------------------|-------------------------|
| `Unsigned8BitLittleEndian` | 8-bit unsigned PCM | 1 byte (0..255)         |
| `Signed16BitLittleEndian`  | 16-bit signed PCM  | 2 bytes (-32768..32767) |
| `Float32BitLittleEndian`   | 32-bit float PCM   | 4 bytes (-1.0..1.0)     |
