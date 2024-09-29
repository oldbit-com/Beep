using OldBit.Beep.Readers;

namespace OldBit.Beep;

internal interface IAudioPlayer : IDisposable
{
    Task PlayAsync(PcmDataReader reader, CancellationToken cancellationToken);

    ValueTask EnqueueAsync(PcmDataReader reader, CancellationToken cancellationToken);

    void Start();

    void Stop();

    void Pause();

    void Resume();
}