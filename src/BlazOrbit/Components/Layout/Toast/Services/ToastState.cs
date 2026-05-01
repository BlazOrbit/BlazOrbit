using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components.Layout.Services;

/// <summary>Runtime state for a single toast tracked by <see cref="IToastService"/>.</summary>
public sealed class ToastState
{
    /// <summary>Initializes a new toast state and stamps <see cref="StartedAt"/> with the current UTC time.</summary>
    public ToastState() => StartedAt = DateTime.UtcNow;

    /// <summary>Render fragment displayed inside the toast body.</summary>
    public required RenderFragment Content { get; init; }
    /// <summary>Timestamp captured at construction.</summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    /// <summary>Cancellation source backing the auto-dismiss timer.</summary>
    public CancellationTokenSource? DismissTokenSource { get; set; }
    /// <summary>Total elapsed time accumulated across pause/resume cycles.</summary>
    public TimeSpan ElapsedBeforePause { get; set; } = TimeSpan.Zero;
    /// <summary>Stable identifier for the toast.</summary>
    public Guid Id { get; } = Guid.NewGuid();
    /// <summary><see langword="true"/> while the closing animation is running.</summary>
    public bool IsClosing { get; set; }
    /// <summary><see langword="true"/> when the auto-dismiss timer is paused.</summary>
    public bool IsPaused { get; set; }
    /// <summary>Options describing duration, position, and animation.</summary>
    public required ToastOptions Options { get; init; }

    /// <summary>Auto-dismiss progress in percent (0–100); 0 when auto-dismiss is disabled.</summary>
    public double ProgressPercentage
    {
        get
        {
            if (!Options.AutoDismiss || Options.Duration == TimeSpan.Zero)
            {
                return 0;
            }

            TimeSpan elapsed = IsPaused
                ? ElapsedBeforePause
                : ElapsedBeforePause + (DateTime.UtcNow - StartedAt);

            double percentage = elapsed.TotalMilliseconds / Options.Duration.TotalMilliseconds * 100;
            return Math.Clamp(percentage, 0, 100);
        }
    }

    /// <summary>Time remaining until the toast auto-dismisses; <see cref="TimeSpan.Zero"/> when auto-dismiss is off.</summary>
    public TimeSpan RemainingTime
    {
        get
        {
            if (!Options.AutoDismiss)
            {
                return TimeSpan.Zero;
            }

            TimeSpan elapsed = IsPaused
                ? ElapsedBeforePause
                : ElapsedBeforePause + (DateTime.UtcNow - StartedAt);

            TimeSpan remaining = Options.Duration - elapsed;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    /// <summary>Timestamp at which the current visible interval started; reset on resume.</summary>
    public DateTime StartedAt { get; set; }
}