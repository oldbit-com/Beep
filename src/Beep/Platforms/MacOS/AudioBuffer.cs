namespace OldBit.Beep.Platforms.MacOS;

internal sealed class AudioBuffer
{
    private static readonly TimeSpan AudioQueueCallbackTimeout = TimeSpan.FromSeconds(15);

    internal readonly IntPtr Pointer;

    internal DateTimeOffset LastAccessTime { get; set; }

    internal bool IsStale => DateTimeOffset.UtcNow - LastAccessTime > AudioQueueCallbackTimeout;

    internal AudioBuffer(IntPtr pointer)
    {
        Pointer = pointer;
        LastAccessTime = DateTimeOffset.UtcNow;
    }
}