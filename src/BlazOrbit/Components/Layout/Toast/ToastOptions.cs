namespace BlazOrbit.Components.Layout;

public sealed class ToastOptions
{
    public static ToastOptions Default => new();

    public static ToastOptions Long => new()
    {
        AutoDismiss = true,
        Duration = TimeSpan.FromSeconds(10)
    };

    public static ToastOptions Persistent => new()
    {
        AutoDismiss = false,
        Closable = true
    };

    public static ToastOptions Quick => new()
    {
        AutoDismiss = true,
        Duration = TimeSpan.FromSeconds(2)
    };

    public ToastAnimation Animation { get; init; } = ToastAnimation.Default;
    public bool AutoDismiss { get; init; } = true;
    public bool Closable { get; init; } = true;
    public string? CssClass { get; init; }
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(5);
    public Action? OnClick { get; init; }
    public Action? OnClose { get; init; }
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

public enum ToastPosition
{
    TopLeft,
    TopCenter,
    TopRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public enum ToastSeverity
{
    Info,
    Success,
    Warning,
    Error
}