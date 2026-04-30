using BlazOrbit.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazOrbit.Components.Layout.Services;

public interface IToastService
{
    event Func<Task>? OnChangeAsync;

    IReadOnlyList<ToastState> ActiveToasts { get; }

    Task CloseAsync(Guid toastId);

    Task CloseAllAsync();

    Task PauseAsync(Guid toastId);

    Task ResumeAsync(Guid toastId);

    Task ShowAsync(Action<RenderTreeBuilder> builder);

    Task ShowAsync(Action<RenderTreeBuilder> builder, ToastOptions? options);

    Task ShowAsync<TComponent>() where TComponent : IComponent;

    Task ShowAsync<TComponent>(ToastOptions? options) where TComponent : IComponent;

    Task ShowAsync<TComponent>(IDictionary<string, object?>? parameters) where TComponent : IComponent;

    Task ShowAsync<TComponent>(IDictionary<string, object?>? parameters, ToastOptions? options) where TComponent : IComponent;
}

public sealed class ToastService : IToastService, IDisposable
{
    private readonly object _lock = new();
    private readonly List<ToastState> _toasts = [];
    private bool _disposed;

    public event Func<Task>? OnChangeAsync;

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

    public Task ShowFragmentAsync(RenderFragment content, ToastOptions? options = null)
    {
        ToastState state = new()
        {
            Content = content,
            Options = options ?? ToastOptions.Default
        };

        return AddToastAsync(state);
    }

    public Task ShowAsync(Action<RenderTreeBuilder> builder) => ShowAsync(builder, null);

    public Task ShowAsync(Action<RenderTreeBuilder> builder, ToastOptions? options)
    {
        RenderFragment fragment = new(builder);
        return ShowFragmentAsync(fragment, options);
    }

    public Task ShowAsync<TComponent>() where TComponent : IComponent => ShowAsync<TComponent>(null, null);

    public Task ShowAsync<TComponent>(ToastOptions? options) where TComponent : IComponent => ShowAsync<TComponent>(null, options);

    public Task ShowAsync<TComponent>(IDictionary<string, object?>? parameters) where TComponent : IComponent => ShowAsync<TComponent>(parameters, null);

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
