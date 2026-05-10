namespace BlazOrbit.Notifications;

/// <summary>Default <see cref="INotificationCenter"/> implementation.</summary>
public sealed class NotificationCenter : INotificationCenter
{
    private readonly INotificationStore _store;

    /// <summary>Initializes a new <see cref="NotificationCenter"/>.</summary>
    public NotificationCenter(INotificationStore store)
    {
        _store = store;
    }

    /// <inheritdoc />
    public event Func<Task>? OnChangeAsync;

    /// <inheritdoc />
    public IReadOnlyList<BOBNotification> All => _store.All();

    /// <inheritdoc />
    public int UnreadCount => _store.All().Count(n => !n.IsRead);

    /// <inheritdoc />
    public async Task PushAsync(BOBNotification notification)
    {
        _store.Add(notification);
        await NotifyChangeAsync();
    }

    /// <inheritdoc />
    public async Task MarkReadAsync(string id)
    {
        if (_store.MarkRead(id))
        {
            await NotifyChangeAsync();
        }
    }

    /// <inheritdoc />
    public async Task MarkAllReadAsync()
    {
        _store.MarkAllRead();
        await NotifyChangeAsync();
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string id)
    {
        if (_store.Remove(id))
        {
            await NotifyChangeAsync();
        }
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        _store.Clear();
        await NotifyChangeAsync();
    }

    private async Task NotifyChangeAsync()
    {
        // Sequential await of subscribers — matches IModalService.OnChangeAsync conventions.
        // Subscriber exceptions bubble (caller decides whether to ignore them); the inbox
        // mutation has already been persisted by the time we get here.
        Func<Task>? handler = OnChangeAsync;
        if (handler is null) return;
        foreach (Delegate d in handler.GetInvocationList())
        {
            await ((Func<Task>)d)();
        }
    }
}
