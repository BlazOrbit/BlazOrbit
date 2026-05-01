namespace BlazOrbit.Components.Layout;

/// <summary>Animation descriptor for toast enter/exit transitions.</summary>
public sealed class ToastAnimation
{
    /// <summary>Default slide-and-fade animation.</summary>
    public static ToastAnimation Default => new();
    /// <summary>Fade-only animation.</summary>
    public static ToastAnimation FadeOnly => new() { Type = ToastAnimationType.Fade };
    /// <summary>No animation.</summary>
    public static ToastAnimation None => new() { Type = ToastAnimationType.None, Duration = TimeSpan.Zero };
    /// <summary>Slide-and-fade animation.</summary>
    public static ToastAnimation SlideAndFade => new() { Type = ToastAnimationType.SlideAndFade };
    /// <summary>Slide-only animation.</summary>
    public static ToastAnimation SlideOnly => new() { Type = ToastAnimationType.Slide };
    /// <summary>Animation duration.</summary>
    public TimeSpan Duration { get; init; } = TimeSpan.FromMilliseconds(300);
    /// <summary>CSS timing function (e.g. <c>ease-out</c>).</summary>
    public string Easing { get; init; } = "ease-out";
    /// <summary>Animation kind.</summary>
    public ToastAnimationType Type { get; init; } = ToastAnimationType.SlideAndFade;

    /// <summary>Returns a copy with <see cref="Duration"/> overridden.</summary>
    public ToastAnimation WithDuration(TimeSpan duration) => new()
    {
        Type = Type,
        Duration = duration,
        Easing = Easing
    };

    /// <summary>Returns a copy with <see cref="Easing"/> overridden.</summary>
    public ToastAnimation WithEasing(string easing) => new()
    {
        Type = Type,
        Duration = Duration,
        Easing = easing
    };
}

/// <summary>Toast animation kinds.</summary>
public enum ToastAnimationType
{
    /// <summary>No animation.</summary>
    None,
    /// <summary>Fade in/out only.</summary>
    Fade,
    /// <summary>Slide in/out only.</summary>
    Slide,
    /// <summary>Combined slide and fade.</summary>
    SlideAndFade
}