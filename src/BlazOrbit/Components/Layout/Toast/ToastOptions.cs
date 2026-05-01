namespace BlazOrbit.Components.Layout;

/// <summary>Configuration for a single toast: duration, position, animation, severity.</summary>
public sealed class ToastOptions
{
    /// <summary>Default options (5 s auto-dismiss, top-right, slide-and-fade).</summary>
    public static ToastOptions Default => new();

    /// <summary>10 s auto-dismiss preset.</summary>
    public static ToastOptions Long => new()
    {
        AutoDismiss = true,
        Duration = TimeSpan.FromSeconds(10)
    };

    /// <summary>Sticky preset — never auto-dismisses, always closable.</summary>
    public static ToastOptions Persistent => new()
    {
        AutoDismiss = false,
        Closable = true
    };

    /// <summary>2 s auto-dismiss preset.</summary>
    public static ToastOptions Quick => new()
    {
        AutoDismiss = true,
        Duration = TimeSpan.FromSeconds(2)
    };

    /// <summary>Enter/exit animation descriptor.</summary>
    public ToastAnimation Animation { get; init; } = ToastAnimation.Default;
    /// <summary>When <see langword="true"/>, the toast dismisses automatically after <see cref="Duration"/>.</summary>
    public bool AutoDismiss { get; init; } = true;
    /// <summary>When <see langword="true"/>, a close button is shown.</summary>
    public bool Closable { get; init; } = true;
    /// <summary>Extra CSS class applied to the toast root.</summary>
    public string? CssClass { get; init; }
    /// <summary>Auto-dismiss duration when <see cref="AutoDismiss"/> is enabled.</summary>
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(5);
    /// <summary>Callback fired when the toast body is clicked.</summary>
    public Action? OnClick { get; init; }
    /// <summary>Callback fired after the toast finishes closing.</summary>
    public Action? OnClose { get; init; }
    /// <summary>Anchor corner where the toast appears.</summary>
    public ToastPosition Position { get; init; } = ToastPosition.TopRight;

    /// <summary>
    /// Material Design elevation level (0–24) applied to the toast surface. Sets a derived
    /// shadow and lifts the surface in dark mode via a tint overlay. <see langword="null"/> (default)
    /// keeps the component-default shadow defined in CSS.
    /// </summary>
    public int? Elevation { get; init; }

    /// <summary>
    /// Drives the ARIA live-region semantics emitted by <c>BOBToast</c>.
    /// <see cref="ToastSeverity.Error"/> renders as <c>role="alert" aria-live="assertive"</c>
    /// (interrupts the screen reader); the others render as <c>role="status" aria-live="polite"</c>.
    /// </summary>
    public ToastSeverity Severity { get; init; } = ToastSeverity.Info;
}

/// <summary>Anchor corner for a toast within the host viewport.</summary>
public enum ToastPosition
{
    /// <summary>Top-left.</summary>
    TopLeft,
    /// <summary>Top-center.</summary>
    TopCenter,
    /// <summary>Top-right (default).</summary>
    TopRight,
    /// <summary>Bottom-left.</summary>
    BottomLeft,
    /// <summary>Bottom-center.</summary>
    BottomCenter,
    /// <summary>Bottom-right.</summary>
    BottomRight
}

/// <summary>Toast severity — drives accent color and ARIA live-region role.</summary>
public enum ToastSeverity
{
    /// <summary>Informational toast.</summary>
    Info,
    /// <summary>Success toast.</summary>
    Success,
    /// <summary>Warning toast.</summary>
    Warning,
    /// <summary>Error toast — rendered with <c>role="alert" aria-live="assertive"</c>.</summary>
    Error
}