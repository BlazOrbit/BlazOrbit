namespace BlazOrbit.Charts.Models;

/// <summary>
/// Event args fired when the user clicks a data point, bar, slice or other
/// interactive primitive on a chart.
/// </summary>
/// <typeparam name="TX">Type of the X-axis value of the clicked point.</typeparam>
/// <typeparam name="TY">Type of the Y-axis value of the clicked point.</typeparam>
public sealed class BOBChartClickArgs<TX, TY>
{
    /// <summary>Label of the series the clicked point belongs to.</summary>
    public string SeriesLabel { get; init; } = string.Empty;

    /// <summary>X value of the clicked point.</summary>
    public TX X { get; init; } = default!;

    /// <summary>Y value of the clicked point.</summary>
    public TY Y { get; init; } = default!;

    /// <summary>Index of the point within its series (0-based).</summary>
    public int PointIndex { get; init; }
}
