using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Utilities;

/// <summary>
/// Async-aware trailing-edge debouncer. Each call to
/// <see cref="InvokeAsync"/> resets the timer; the supplied action only runs
/// when the specified interval elapses without further invocations.
/// </summary>
/// <remarks>
/// Designed for Blazor input scenarios — search-as-you-type, server-side
/// validation, expensive callbacks per keystroke. Pairs with
/// <see cref="Microsoft.AspNetCore.Components.EventCallback{T}"/>: pass the
/// callback's <c>InvokeAsync</c> as the action.
/// <para>
/// The debouncer is <see cref="IDisposable"/>; consume from a component's
/// <c>DisposeAsync</c>/<c>Dispose</c> path so a pending invocation does not
/// fire after the component is unmounted.
/// </para>
/// </remarks>
/// <typeparam name="T">Argument type forwarded to the debounced action.</typeparam>
public sealed class BOBDebouncer<T> : IDisposable
{
    private readonly TimeSpan _interval;
    private readonly TimeProvider _timeProvider;
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;
    private bool _disposed;

    /// <summary>Initializes a new debouncer with the given trailing-edge delay.</summary>
    /// <param name="interval">Delay between the last <see cref="InvokeAsync"/> call and action execution.</param>
    /// <param name="timeProvider">Optional clock; defaults to <see cref="TimeProvider.System"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="interval"/> is negative.</exception>
    public BOBDebouncer(TimeSpan interval, TimeProvider? timeProvider = null)
    {
        if (interval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be non-negative.");
        }

        _interval = interval;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Schedules <paramref name="action"/> with <paramref name="arg"/>. If invoked
    /// again before the configured interval elapses, the previous schedule is
    /// cancelled and the timer restarts with the latest argument.
    /// </summary>
    /// <returns>
    /// A task that completes either when the action runs or when a subsequent
    /// invocation cancels this one. Catches <see cref="TaskCanceledException"/>
    /// and <see cref="OperationCanceledException"/> internally — the returned
    /// task always completes successfully.
    /// </returns>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CancellationTokenSource ownership transfers to the awaiter; disposed in Dispose() or on next InvokeAsync.")]
    public async Task InvokeAsync(T arg, Func<T, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        CancellationToken token;
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            token = _cts.Token;
        }

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

        lock (_lock)
        {
            if (_disposed || token.IsCancellationRequested)
            {
                return;
            }
        }

        await action(arg);
    }

    /// <summary>Cancels any pending invocation without disposing the debouncer.</summary>
    public void Cancel()
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// Cancels any pending invocation and prevents future ones. Idempotent.
    /// </summary>
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
        }
    }
}
