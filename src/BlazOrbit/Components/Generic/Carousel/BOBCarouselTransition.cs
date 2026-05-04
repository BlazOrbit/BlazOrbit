namespace BlazOrbit.Components;

/// <summary>
/// Visual transition applied when the active slide of a <see cref="BOBCarousel"/> changes.
/// </summary>
public enum BOBCarouselTransition
{
    /// <summary>
    /// Slides translate horizontally. The active slide takes the full viewport; previous and
    /// next slides shift in from the side. Default.
    /// </summary>
    Slide = 0,

    /// <summary>
    /// Slides cross-fade in place. All slides occupy the same position; only opacity changes.
    /// </summary>
    Fade = 1,

    /// <summary>
    /// No animation. Slides snap instantly to the active position.
    /// </summary>
    None = 2,

    /// <summary>
    /// 3D coverflow effect. The active slide stays in front; neighbouring slides recede with a
    /// perspective rotation, fading and scaling down based on distance from the active index.
    /// </summary>
    Coverflow = 3,

    /// <summary>
    /// 3D orbit. The active slide stays in front; the rest fan out behind it on a horizontal
    /// circle, always facing the camera. Side and back slides shrink and fade with depth.
    /// </summary>
    Wheel = 4,

    /// <summary>
    /// Continuous 3D perspective carousel. Several slides are visible at once, spread across the
    /// viewport with a fixed inward tilt. Side slides keep their size and stay visible, giving
    /// the impression of a constant 3D scene rather than a deep orbit.
    /// </summary>
    Wheel3D = 5,
}
