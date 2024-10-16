namespace OldBit.Beep;

/// <summary>
/// Exception thrown by the audio player.
/// </summary>
public class AudioPlayerException : Exception
{
    public int StatusCode { get; }

    public AudioPlayerException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public AudioPlayerException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}