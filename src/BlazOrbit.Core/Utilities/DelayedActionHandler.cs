namespace BlazOrbit.Abstractions;

internal sealed class DelayedActionHandler : IDisposable
{
    private readonly object _lock = new();
    private readonly TimeProvider _timeProvider;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    public DelayedActionHandler(TimeProvider? timeProvider = null) => _timeProvider = timeProvider ?? TimeProvider.System;

    public void Cancel()
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _disposed = true;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }

    public Task ExecuteWithDelayAsync(Func<Task> action, TimeSpan delay)
        => ExecuteWithDelayAsync(_ => action(), delay);

    public async Task ExecuteWithDelayAsync(Func<CancellationToken, Task> action, TimeSpan delay)
    {
        CancellationToken ct;
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            ct = _cts.Token;
        }

        try
        {
            await Task.Delay(delay, _timeProvider, ct);

            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }
            }

            // Forward the token so long-running actions can propagate cancellation if Dispose/Cancel
            // fires mid-flight. Dispose cancels the CTS so the token observes the request.
            await action(ct);
        }
        catch (TaskCanceledException)
        {
        }
        catch (OperationCanceledException)
        {
        }
    }
}