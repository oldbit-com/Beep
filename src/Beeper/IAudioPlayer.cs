using OldBit.Beeper.Helpers;
using OldBit.Beeper.IO;

namespace OldBit.Beeper;

internal interface IAudioPlayer : IDisposable
{
    void Start();

    void Stop();

    Task Play(PcmDataReader reader, CancellationToken cancellationToken);
}