namespace BlazOrbit.Charts.Models;

/// <summary>
/// Event args fired when the user finishes a brush drag on the chart's plot
/// area. <see cref="MinX"/> / <see cref="MaxX"/> bracket the selected range
/// in the original X-axis domain (a <see cref="System.DateTime"/> when
/// <typeparamref name="TX"/> is temporal, an <see cref="int"/> when numeric,
/// etc.).
/// </summary>
/// <typeparam name="TX">X-axis domain type the chart was bound to.</typeparam>
public sealed class BOBChartBrushArgs<TX>
{
    /// <summary>Lower bound of the selected range, inclusive.</summary>
    public TX MinX { get; init; } = default!;

    /// <summary>Upper bound of the selected range, inclusive.</summary>
    public TX MaxX { get; init; } = default!;
}
