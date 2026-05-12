namespace BlazOrbit.Notifications;

/// <summary>
/// Default <see cref="INotificationStore"/> implementation — purely in-memory, scoped per
/// circuit / page. State evaporates on full page reload; consumers wanting persistence
/// supply their own implementation.
/// </summary>
public sealed class InMemoryNotificationStore : INotificationStore
{
    private readonly List<BOBNotification> _entries = [];
    private readonly object _lock = new();

    /// <inheritdoc />
    public IReadOnlyList<BOBNotification> All()
    {
        lock (_lock)
        {
            return _entries
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }
    }

    /// <inheritdoc />
    public void Add(BOBNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        lock (_lock)
        {
            _entries.Add(notification);
        }
    }

    /// <inheritdoc />
    public bool MarkRead(string id)
    {
        lock (_lock)
        {
            BOBNotification? hit = _entries.Find(n => n.Id == id);
            if (hit is null)
            {
                return false;
            }

            hit.IsRead = true;
            return true;
        }
    }

    /// <inheritdoc />
    public void MarkAllRead()
    {
        lock (_lock)
        {
            foreach (BOBNotification n in _entries)
            {
                n.IsRead = true;
            }
        }
    }

    /// <inheritdoc />
    public bool Remove(string id)
    {
        lock (_lock)
        {
            return _entries.RemoveAll(n => n.Id == id) > 0;
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }
}