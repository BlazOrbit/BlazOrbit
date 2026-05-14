namespace BlazOrbit.Notifications;

/// <summary>
/// Persistent inbox-style notification service. Pairs with <c>BOBNotificationBell</c> for
/// the unread badge and any consumer-supplied UI (drawer, panel, popover) for the list.
/// </summary>
/// <remarks>
/// Storage is delegated to <see cref="INotificationStore"/>. The default registration uses
/// <see cref="InMemoryNotificationStore"/>; pass a custom implementation to
/// <c>AddBlazOrbitNotifications</c> to persist across reloads.
/// </remarks>
public interface INotificationCenter
{
    /// <summary>Raised whenever the inbox content changes (push / mark-read / remove / clear).</summary>
    event Func<Task>? OnChangeAsync;

    /// <summary>Snapshot of the inbox, newest first.</summary>
    IReadOnlyList<BOBNotification> All { get; }

    /// <summary>Count of entries with <see cref="BOBNotification.IsRead"/> = <see langword="false"/>.</summary>
    int UnreadCount { get; }

    /// <summary>Pushes a new entry into the inbox and notifies subscribers.</summary>
    Task PushAsync(BOBNotification notification);

    /// <summary>Marks a single entry as read.</summary>
    Task MarkReadAsync(string id);

    /// <summary>Marks every entry as read.</summary>
    Task MarkAllReadAsync();

    /// <summary>Removes a single entry from the inbox.</summary>
    Task RemoveAsync(string id);

    /// <summary>Empties the inbox.</summary>
    Task ClearAsync();
}