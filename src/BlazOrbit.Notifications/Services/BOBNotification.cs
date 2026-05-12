namespace BlazOrbit.Notifications;

/// <summary>
/// Single inbox entry consumed by <see cref="INotificationCenter"/>. Persistent (unlike
/// <c>BOBToast</c>) — meant to live until the user marks it read or clears it.
/// </summary>
public sealed class BOBNotification
{
    /// <summary>Stable identifier. Generated automatically when omitted.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("n");

    /// <summary>Headline rendered in the inbox.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Optional body line below the title.</summary>
    public string? Body { get; init; }

    /// <summary>Severity tier — drives icon / accent colour.</summary>
    public NotificationSeverity Severity { get; init; } = NotificationSeverity.Info;

    /// <summary>Optional category tag used for filtering.</summary>
    public string? Category { get; init; }

    /// <summary>Optional href the inbox row links to.</summary>
    public string? ActionUrl { get; init; }

    /// <summary>Created-at timestamp. Defaults to <see cref="DateTimeOffset.UtcNow"/>.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Read flag. Mutable so <see cref="INotificationCenter.MarkReadAsync(string)"/> can flip it without rebuilding the record.</summary>
    public bool IsRead { get; set; }
}