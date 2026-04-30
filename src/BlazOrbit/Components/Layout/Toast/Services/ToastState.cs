using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components.Layout.Services;

public sealed class ToastState
{
    public ToastState() => StartedAt = DateTime.UtcNow;

    public required RenderFragment Content { get; init; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public CancellationTokenSource? DismissTokenSource { get; set; }
    public TimeSpan ElapsedBeforePause { get; set; } = TimeSpan.Zero;
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsClosing { get; set; }
    public bool IsPaused { get; set; }
    public required ToastOptions Options { get; init; }

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

    public DateTime StartedAt { get; set; }
}