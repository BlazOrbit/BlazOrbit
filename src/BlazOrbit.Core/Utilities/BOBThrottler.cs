using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Utilities;

/// <summary>
/// Async-aware leading-edge throttler. The first <see cref="InvokeAsync"/>
/// call within an idle window runs immediately; subsequent calls inside
/// the configured interval are coalesced — only the latest argument is
/// kept and executed once the interval elapses.
/// </summary>
/// <remarks>
/// Use when the consumer wants the first event to feel instant but caps the
/// invocation rate (resize handlers, scroll listeners, drag events). For
/// trailing-edge behaviour (run only after activity stops) use
/// <see cref="BOBDebouncer{T}"/> instead.
/// </remarks>
/// <typeparam name="T">Argument type forwarded to the throttled action.</typeparam>
public sealed class BOBThrottler<T> : IDisposable
{
    private readonly TimeSpan _interval;
    private readonly TimeProvider _timeProvider;
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;
    private bool _coolingDown;
    private bool _hasPending;
    private T? _pendingArg;
    private Func<T, Task>? _pendingAction;
    private bool _disposed;

    /// <summary>Initializes a new throttler with the given minimum interval between invocations.</summary>
    /// <param name="interval">Minimum spacing between two consecutive action invocations.</param>
    /// <param name="timeProvider">Optional clock; defaults to <see cref="TimeProvider.System"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="interval"/> is negative.</exception>
    public BOBThrottler(TimeSpan interval, TimeProvider? timeProvider = null)
    {
        if (interval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be non-negative.");
        }

        _interval = interval;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Invokes <paramref name="action"/> with <paramref name="arg"/>. If the
    /// throttler is idle, the action runs immediately; otherwise the argument
    /// is buffered and a single trailing invocation fires when the cooldown
    /// expires (using the most recent buffered argument).
    /// </summary>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CancellationTokenSource ownership transfers to the awaiter; disposed in Dispose() / on coalesce reset.")]
    public async Task InvokeAsync(T arg, Func<T, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        bool runNow;
        CancellationToken token = default;

        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            if (_coolingDown)
            {
                _hasPending = true;
                _pendingArg = arg;
                _pendingAction = action;
                return;
            }

            _coolingDown = true;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            token = _cts.Token;
            runNow = true;
        }

        if (runNow)
        {
            await action(arg);
            await ScheduleCooldownAsync(token);
        }
    }

    private async Task ScheduleCooldownAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(_interval, _timeProvider, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }
        catch (OperationCanceledException)
        {
            return;
        }

        T? trailingArg;
        Func<T, Task>? trailingAction;
        bool hasTrailing;

        lock (_lock)
        {
            if (_disposed)
            {
                _coolingDown = false;
                _hasPending = false;
                _pendingAction = null;
                return;
            }

            hasTrailing = _hasPending;
            trailingArg = _pendingArg;
            trailingAction = _pendingAction;
            _hasPending = false;
            _pendingArg = default;
            _pendingAction = null;
            _coolingDown = false;
        }

        if (hasTrailing && trailingAction is not null)
        {
            // Re-enter through InvokeAsync so the trailing call honours the
            // standard rate limit instead of firing back-to-back.
            await InvokeAsync(trailingArg!, trailingAction);
        }
    }

    /// <summary>Cancels any pending trailing invocation without disposing the throttler.</summary>
    public void Cancel()
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _coolingDown = false;
            _hasPending = false;
            _pendingAction = null;
            _pendingArg = default;
        }
    }

    /// <summary>Cancels any pending invocation and prevents future ones. Idempotent.</summary>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _hasPending = false;
            _pendingAction = null;
            _pendingArg = default;
        }
    }
}
