namespace OldBit.Beeper;

public interface IAudioPlayer : IDisposable
{
    void Start();

    void Stop();

    Task Enqueue(float[] data, CancellationToken cancellationToken = default);

    int BufferSizeInBytes { get; }
}