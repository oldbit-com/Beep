namespace OldBit.Beep;

/// <summary>
/// Exception thrown by the audio player.
/// </summary>
public class AudioPlayerException : Exception
{
    /// <summary>
    /// The status code of the exception. This is usually a platform-specific error code.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioPlayerException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public AudioPlayerException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioPlayerException"/> class with a specified error message and status code.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="statusCode">The status code of the exception. This is usually a platform-specific error code.</param>
    public AudioPlayerException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}