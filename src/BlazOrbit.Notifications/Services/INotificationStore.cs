namespace BlazOrbit.Notifications;

/// <summary>
/// Storage strategy for <see cref="INotificationCenter"/>. Default implementation is
/// in-memory - consumers wanting localStorage / IndexedDB / server-side persistence
/// register their own under <c>AddBlazOrbitNotifications()</c>.
/// </summary>
public interface INotificationStore
{
    /// <summary>Snapshot of every stored notification, newest first.</summary>
    IReadOnlyList<BOBNotification> All();

    /// <summary>Persists a new entry.</summary>
    void Add(BOBNotification notification);

    /// <summary>Marks an entry as read by id; returns <see langword="true"/> when found.</summary>
    bool MarkRead(string id);

    /// <summary>Marks every entry as read.</summary>
    void MarkAllRead();

    /// <summary>Removes a single entry; returns <see langword="true"/> when found.</summary>
    bool Remove(string id);

    /// <summary>Removes every entry.</summary>
    void Clear();
}