namespace BlazOrbit.Charts.Models;

/// <summary>
/// A single slice of a Pie / Donut chart. Pie / Donut charts do not have an
/// X axis, so they consume <see cref="BOBChartSlice{TY}"/> instead of
/// <see cref="BOBChartSeries{TX, TY}"/>.
/// </summary>
/// <typeparam name="TY">Numeric type of the slice value.</typeparam>
public sealed class BOBChartSlice<TY>
{
    /// <summary>Display label of the slice (legend, tooltip, ARIA).</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>The slice value. Slices are normalized to 100% at render time.</summary>
    public TY Value { get; init; } = default!;

    /// <summary>
    /// Optional explicit color. When <c>null</c> the color is drawn from the
    /// active palette in declaration order.
    /// </summary>
    public string? Color { get; init; }
}
