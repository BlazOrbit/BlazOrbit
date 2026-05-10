namespace BlazOrbit.Charts.Models;

/// <summary>
/// Event args fired when the user hovers a data point. Charts emit this
/// alongside (not instead of) their internal tooltip rendering, so callers
/// can drive secondary UI (status bars, linked highlighting) off the same
/// signal.
/// </summary>
/// <typeparam name="TX">Type of the X-axis value of the hovered point.</typeparam>
/// <typeparam name="TY">Type of the Y-axis value of the hovered point.</typeparam>
public sealed class BOBChartHoverArgs<TX, TY>
{
    /// <summary>Label of the series the hovered point belongs to.</summary>
    public string SeriesLabel { get; init; } = string.Empty;

    /// <summary>X value of the hovered point.</summary>
    public TX X { get; init; } = default!;

    /// <summary>Y value of the hovered point.</summary>
    public TY Y { get; init; } = default!;

    /// <summary>Index of the point within its series (0-based).</summary>
    public int PointIndex { get; init; }
}
