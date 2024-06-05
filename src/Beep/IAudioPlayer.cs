using OldBit.Beep.Readers;

namespace OldBit.Beep;

internal interface IAudioPlayer : IDisposable
{
    void Start();

    void Stop();

    Task Play(PcmDataReader reader, CancellationToken cancellationToken);
}