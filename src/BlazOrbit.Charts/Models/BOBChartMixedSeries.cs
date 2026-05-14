using BlazOrbit.Charts.Enums;

namespace BlazOrbit.Charts.Models;

/// <summary>
/// Single series inside a <see cref="Components.BOBMixedChart{TX}"/>. Carries the
/// visual type (bar / line / area), the data points and an optional flag to bind the
/// series to the chart's secondary Y axis instead of the primary one.
/// </summary>
/// <typeparam name="TX">X-axis domain type.</typeparam>
public sealed class BOBChartMixedSeries<TX>
{
    /// <summary>Display label used by the legend and tooltips.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Visual representation for this series.</summary>
    public BOBChartMixedSeriesType Type { get; init; } = BOBChartMixedSeriesType.Bar;

    /// <summary>Data points in plot order (typically left-to-right).</summary>
    public IEnumerable<BOBChartPoint<TX, double>> Points { get; init; } = [];

    /// <summary>Optional explicit colour (any valid CSS colour).</summary>
    public string? Color { get; init; }

    /// <summary>
    /// When <see langword="true"/>, the series is scaled against the chart's secondary
    /// Y axis (rendered on the right side) — useful for "actual vs target" reports
    /// where one series is a value and the other a ratio in a different range.
    /// </summary>
    public bool UseSecondaryAxis { get; init; }

    /// <summary>Stroke width for line / area series. Ignored by bar series. Default 2.</summary>
    public int BorderWidth { get; init; } = 2;
}
