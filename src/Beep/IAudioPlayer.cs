using OldBit.Beep.Pcm;

namespace OldBit.Beep;

internal interface IAudioPlayer : IDisposable
{
    ValueTask EnqueueAsync(PcmDataReader reader, CancellationToken cancellationToken);

    bool TryEnqueue(PcmDataReader reader);

    void Start();

    void Stop();
}