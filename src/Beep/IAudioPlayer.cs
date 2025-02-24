using OldBit.Beep.Pcm;

namespace OldBit.Beep;

/// <summary>
/// Initializes a new instance of the <see cref="PlayerErrorEventArgs"/> class with the specified error.
/// </summary>
/// <param name="error">The error that occurred.</param>
public class PlayerErrorEventArgs(Exception error) : EventArgs
{
    /// <summary>
    /// The error that occurred.
    /// </summary>
    public Exception Error { get; internal set; } = error;
}

internal interface IAudioPlayer : IDisposable
{
    ValueTask EnqueueAsync(PcmDataReader reader, CancellationToken cancellationToken);

    bool TryEnqueue(PcmDataReader reader);

    void Start();

    void Stop();

    event EventHandler<PlayerErrorEventArgs> OnError;
}