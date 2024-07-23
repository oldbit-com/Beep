namespace OldBit.Beep.Helpers;

internal class AsyncPauseResume
{
    private bool _isPaused;
    private TaskCompletionSource<bool> _pauseCompletionSource = null!;

    internal async Task WaitIfPausedAsync(CancellationToken cancellationToken)
    {
        if (!_isPaused)
        {
            return;
        }

        await using (cancellationToken.Register(() => _pauseCompletionSource.TrySetResult(true)))
        {
            await _pauseCompletionSource.Task;
        }
    }

    internal void Pause()
    {
        if (_isPaused)
        {
            return;
        }

        _isPaused = true;
        _pauseCompletionSource = new TaskCompletionSource<bool>();
    }

    internal void Resume()
    {
        if (!_isPaused)
        {
            return;
        }

        _isPaused = false;
        _pauseCompletionSource.TrySetResult(true);
    }
}