using BlazOrbit.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazOrbit.Components.Layout.Services;

/// <summary>Coordinates toast notifications shown by <c>BOBToastHost</c>.</summary>
public interface IToastService
{
    /// <summary>Raised whenever the active-toasts list changes; awaited sequentially.</summary>
    event Func<Task>? OnChangeAsync;

    /// <summary>Currently active toasts.</summary>
    IReadOnlyList<ToastState> ActiveToasts { get; }

    /// <summary>Closes the toast identified by <paramref name="toastId"/>.</summary>
    Task CloseAsync(Guid toastId);

    /// <summary>Closes every active toast.</summary>
    Task CloseAllAsync();

    /// <summary>Pauses auto-dismiss for the toast identified by <paramref name="toastId"/>.</summary>
    Task PauseAsync(Guid toastId);

    /// <summary>Resumes auto-dismiss for the toast identified by <paramref name="toastId"/>.</summary>
    Task ResumeAsync(Guid toastId);

    /// <summary>Shows a toast whose body is built by the supplied render-tree builder action.</summary>
    Task ShowAsync(Action<RenderTreeBuilder> builder);

    /// <summary>Shows a toast whose body is built by the supplied builder, with explicit options.</summary>
    Task ShowAsync(Action<RenderTreeBuilder> builder, ToastOptions? options);

    /// <summary>Shows a toast that renders <typeparamref name="TComponent"/>.</summary>
    Task ShowAsync<TComponent>() where TComponent : IComponent;

    /// <summary>Shows a toast that renders <typeparamref name="TComponent"/> with explicit options.</summary>
    Task ShowAsync<TComponent>(ToastOptions? options) where TComponent : IComponent;

    /// <summary>Shows a toast that renders <typeparamref name="TComponent"/> with the given parameters.</summary>
    Task ShowAsync<TComponent>(IDictionary<string, object?>? parameters) where TComponent : IComponent;

    /// <summary>Shows a toast that renders <typeparamref name="TComponent"/> with parameters and explicit options.</summary>
    Task ShowAsync<TComponent>(IDictionary<string, object?>? parameters, ToastOptions? options) where TComponent : IComponent;
}

/// <summary>Default <see cref="IToastService"/> implementation registered by <c>AddBlazOrbit()</c>.</summary>
public sealed class ToastService : IToastService, IDisposable
{
    private readonly object _lock = new();
    private readonly List<ToastState> _toasts = [];
    private bool _disposed;

    /// <inheritdoc />
    public event Func<Task>? OnChangeAsync;

    /// <inheritdoc />
    public IReadOnlyList<ToastState> ActiveToasts
    {
        get
        {
            lock (_lock)
            {
                return _toasts.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>Number of currently active toasts.</summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _toasts.Count;
            }
        }
    }

    /// <inheritdoc />
    public async Task CloseAsync(Guid toastId)
    {
        ToastState? toast;
        lock (_lock)
        {
            toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast == null)
            {
                return;
            }

            toast.IsClosing = true;
            toast.DismissTokenSource?.Cancel();
        }

        await NotifyChangeAsync();
    }

    /// <inheritdoc />
    public async Task CloseAllAsync()
    {
        lock (_lock)
        {
            foreach (ToastState toast in _toasts)
            {
                toast.IsClosing = true;
                toast.DismissTokenSource?.Cancel();
            }
        }

        await NotifyChangeAsync();
    }

    /// <inheritdoc />
    public async Task PauseAsync(Guid toastId)
    {
        lock (_lock)
        {
            ToastState? toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast == null || toast.IsPaused || !toast.Options.AutoDismiss)
            {
                return;
            }

            toast.DismissTokenSource?.Cancel();
            toast.ElapsedBeforePause += DateTime.UtcNow - toast.StartedAt;
            toast.IsPaused = true;
        }

        await NotifyChangeAsync();
    }

    /// <inheritdoc />
    public async Task ResumeAsync(Guid toastId)
    {
        lock (_lock)
        {
            ToastState? toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast == null || !toast.IsPaused || !toast.Options.AutoDismiss)
            {
                return;
            }

            toast.IsPaused = false;
            toast.StartedAt = DateTime.UtcNow;

            ScheduleDismiss(toast);
        }

        await NotifyChangeAsync();
    }

    /// <summary>Shows a toast whose body is the supplied <see cref="RenderFragment"/>.</summary>
    public Task ShowFragmentAsync(RenderFragment content, ToastOptions? options = null)
    {
        ToastState state = new()
        {
            Content = content,
            Options = options ?? ToastOptions.Default
        };

        return AddToastAsync(state);
    }

    /// <inheritdoc />
    public Task ShowAsync(Action<RenderTreeBuilder> builder) => ShowAsync(builder, null);

    /// <inheritdoc />
    public Task ShowAsync(Action<RenderTreeBuilder> builder, ToastOptions? options)
    {
        RenderFragment fragment = new(builder);
        return ShowFragmentAsync(fragment, options);
    }

    /// <inheritdoc />
    public Task ShowAsync<TComponent>() where TComponent : IComponent => ShowAsync<TComponent>(null, null);

    /// <inheritdoc />
    public Task ShowAsync<TComponent>(ToastOptions? options) where TComponent : IComponent => ShowAsync<TComponent>(null, options);

    /// <inheritdoc />
    public Task ShowAsync<TComponent>(IDictionary<string, object?>? parameters) where TComponent : IComponent => ShowAsync<TComponent>(parameters, null);

    /// <inheritdoc />
    public Task ShowAsync<TComponent>(IDictionary<string, object?>? parameters, ToastOptions? options) where TComponent : IComponent
    {
        void fragment(RenderTreeBuilder builder)
        {
            int seq = 0;
            builder.OpenComponent<TComponent>(seq++);

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object?> param in parameters)
                {
                    builder.AddAttribute(seq++, param.Key, param.Value);
                }
            }

            builder.CloseComponent();
        }

        return ShowFragmentAsync(fragment, options);
    }

    internal async Task RemoveAsync(Guid toastId)
    {
        ToastState? toast;
        lock (_lock)
        {
            toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast == null)
            {
                return;
            }

            toast.DismissTokenSource?.Dispose();
            _toasts.Remove(toast);
        }

        toast.Options.OnClose?.Invoke();
        await NotifyChangeAsync();
    }

    private async Task AddToastAsync(ToastState toast)
    {
        lock (_lock)
        {
            _toasts.Add(toast);

            if (toast.Options.AutoDismiss)
            {
                ScheduleDismiss(toast);
            }
        }

        await NotifyChangeAsync();
    }

    private async Task DismissAfterDelayAsync(Guid toastId, TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
            await CloseAsync(toastId);
        }
        catch (TaskCanceledException)
        {
        }
    }

    // Awaits each subscriber sequentially. Multiple subscribers run in registration
    // order, and an exception from one stops propagation — by design: a misbehaving
    // host should surface, not get swallowed.
    private async Task NotifyChangeAsync()
    {
        Func<Task>? handler = OnChangeAsync;
        if (handler is null)
        {
            return;
        }

        foreach (Func<Task> subscriber in handler.GetInvocationList().Cast<Func<Task>>())
        {
            await subscriber();
        }
    }

    // Intrinsic timer pattern: fires CloseAsync after a delay independently of the
    // calling Show*/ResumeAsync flow. Awaiting it would block Show() for the toast's
    // entire visible duration, which defeats the purpose. Same shape as
    // BOBTooltip.StartAutoClose.
    private void ScheduleDismiss(ToastState toast)
    {
        toast.DismissTokenSource?.Cancel();
        toast.DismissTokenSource = new CancellationTokenSource();

        TimeSpan delay = toast.RemainingTime > TimeSpan.Zero
            ? toast.RemainingTime
            : toast.Options.Duration;

        BOBAsyncHelper.SafeFireAndForget(() => DismissAfterDelayAsync(toast.Id, delay, toast.DismissTokenSource.Token));
    }

    /// <summary>Cancels every pending dismiss timer and releases bookkeeping.</summary>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (ToastState toast in _toasts)
            {
                try
                {
                    toast.DismissTokenSource?.Cancel();
                    toast.DismissTokenSource?.Dispose();
                }
                catch (ObjectDisposedException) { }
            }

            _toasts.Clear();
        }

        OnChangeAsync = null;
    }
}
