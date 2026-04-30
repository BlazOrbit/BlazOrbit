namespace BlazOrbit.Components.Layout;

public sealed class ToastAnimation
{
    public static ToastAnimation Default => new();
    public static ToastAnimation FadeOnly => new() { Type = ToastAnimationType.Fade };
    public static ToastAnimation None => new() { Type = ToastAnimationType.None, Duration = TimeSpan.Zero };
    public static ToastAnimation SlideAndFade => new() { Type = ToastAnimationType.SlideAndFade };
    public static ToastAnimation SlideOnly => new() { Type = ToastAnimationType.Slide };
    public TimeSpan Duration { get; init; } = TimeSpan.FromMilliseconds(300);
    public string Easing { get; init; } = "ease-out";
    public ToastAnimationType Type { get; init; } = ToastAnimationType.SlideAndFade;

    public ToastAnimation WithDuration(TimeSpan duration) => new()
    {
        Type = Type,
        Duration = duration,
        Easing = Easing
    };

    public ToastAnimation WithEasing(string easing) => new()
    {
        Type = Type,
        Duration = Duration,
        Easing = easing
    };
}

public enum ToastAnimationType
{
    None,
    Fade,
    Slide,
    SlideAndFade
}