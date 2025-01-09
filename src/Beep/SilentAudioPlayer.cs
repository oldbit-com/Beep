using OldBit.Beep.Pcm;

namespace OldBit.Beep;

/// <summary>
/// Represents an audio player that does not play any audio.
/// </summary>
internal class SilentAudioPlayer : IAudioPlayer
{
    public ValueTask EnqueueAsync(PcmDataReader reader, CancellationToken cancellationToken) => ValueTask.CompletedTask;

    public bool TryEnqueue(PcmDataReader reader) => true;

    public void Start() { }

    public void Stop() { }

    public void Dispose() { }
}