using OldBit.Beep.Readers;

namespace OldBit.Beep;

internal interface IAudioPlayer : IDisposable
{
    Task PlayAsync(PcmDataReader reader, CancellationToken cancellationToken);

    void Pause();

    void Resume();
}