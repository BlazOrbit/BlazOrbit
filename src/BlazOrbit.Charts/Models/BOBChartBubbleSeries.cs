namespace BlazOrbit.Charts.Models;

/// <summary>
/// A named, typed sequence of <see cref="BOBChartBubblePoint{TX, TY}"/> data
/// for <see cref="Components.BOBScatterChart{TX, TY}"/> in bubble mode.
/// Pass via the chart's <c>BubbleSeries</c> parameter; the chart auto-scales
/// the per-point <see cref="BOBChartBubblePoint{TX, TY}.Size"/> across the
/// series into pixel radii bounded by <c>BubbleMinRadius</c> /
/// <c>BubbleMaxRadius</c>.
/// </summary>
/// <typeparam name="TX">Type of the X-axis values.</typeparam>
/// <typeparam name="TY">Type of the Y-axis values.</typeparam>
public sealed class BOBChartBubbleSeries<TX, TY>
{
    /// <summary>Display name (legend, tooltip, ARIA).</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>The bubble points, in plot order.</summary>
    public IEnumerable<BOBChartBubblePoint<TX, TY>> Points { get; init; }
        = Array.Empty<BOBChartBubblePoint<TX, TY>>();

    /// <summary>
    /// Optional explicit color for the series. When <c>null</c> the color is
    /// drawn from the active palette in declaration order.
    /// </summary>
    public string? Color { get; init; }
}
