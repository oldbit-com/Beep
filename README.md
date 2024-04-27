# Beeper

Beeper is a simple cross platform dotnet library for playing audio.

By default dotnet does not have a built in way to play audio, so this library provides a very simple way to play PCM audio in dotnet projects.

It was inspired by the [oto](https://github.com/ebitengine/oto) golang library I used before.

The library does not have any dependencies, it uses native OS libraries to play audio.

However this is more an example and exercise how to create cross platform libraries in dotnet with native OS dependencies.


Supported platforms:
- MacOS
- Windows

### MacOS
It uses AudioToolbox.framework to play audio.

### Windows

## Usage

```csharp
using OldBit.Beeper;

using var audioPlayer = new AudioPlayer(AudioFormat.Float32BitLittleEndian, 44100, 2);
audioPlayer.Start();
await audioPlayer.EnqueueAsync(new byyte[] { 0.5f, 0.5f, 0.5f, 0.5f });
audioPlayer.Stop();
```

