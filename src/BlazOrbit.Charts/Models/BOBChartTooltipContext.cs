namespace BlazOrbit.Charts.Models;

/// <summary>
/// State passed to a custom <c>TooltipTemplate</c> render fragment. Captures
/// the active series, the active point (X / Y values) and the resolved color.
/// </summary>
/// <typeparam name="TX">Type of the X-axis value.</typeparam>
/// <typeparam name="TY">Type of the Y-axis value.</typeparam>
public sealed class BOBChartTooltipContext<TX, TY>
{
    /// <summary>Label of the series the active point belongs to.</summary>
    public string SeriesLabel { get; init; } = string.Empty;

    /// <summary>The active point's X value.</summary>
    public TX X { get; init; } = default!;

    /// <summary>The active point's Y value.</summary>
    public TY Y { get; init; } = default!;

    /// <summary>Color resolved for the series (palette or explicit override).</summary>
    public string Color { get; init; } = string.Empty;
}
