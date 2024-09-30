using OldBit.Beep.Readers;

namespace OldBit.Beep;

internal interface IAudioPlayer : IDisposable
{
    ValueTask EnqueueAsync(PcmDataReader reader, CancellationToken cancellationToken);

    void Start();

    void Stop();
}