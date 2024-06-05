using OldBit.Beep.Readers;

namespace OldBit.Beep;

internal interface IAudioPlayer : IDisposable
{
    void Start();

    void Stop();

    Task PlayAsync(PcmDataReader reader, CancellationToken cancellationToken);
}