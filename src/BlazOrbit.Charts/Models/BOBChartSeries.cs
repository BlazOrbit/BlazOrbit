namespace BlazOrbit.Charts.Models;

/// <summary>
/// A named, typed sequence of data points rendered as a single visual series
/// (a bar group, a line, an area). Multiple series share the same axes and
/// legend.
/// </summary>
/// <typeparam name="TX">Type of the X-axis values.</typeparam>
/// <typeparam name="TY">Type of the Y-axis values.</typeparam>
public sealed class BOBChartSeries<TX, TY>
{
    /// <summary>
    /// Display name of the series. Used by the legend, tooltip and ARIA
    /// labels. Must be unique within a chart.
    /// </summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>The data points, in plot order.</summary>
    public IEnumerable<BOBChartPoint<TX, TY>> Points { get; init; }
        = Array.Empty<BOBChartPoint<TX, TY>>();

    /// <summary>
    /// Optional explicit color for the series. When <c>null</c> the color is
    /// drawn from the active palette in declaration order.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Whether the series renders filled (area chart) or stroked-only
    /// (line chart). Charts that ignore fills (bar) silently disregard.
    /// </summary>
    public bool Fill { get; init; }

    /// <summary>Stroke width for line / area charts, in pixels.</summary>
    public int BorderWidth { get; init; } = 2;
}
