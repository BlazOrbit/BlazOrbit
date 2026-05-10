namespace BlazOrbit.Charts.Models;

/// <summary>
/// Event args fired when the user hovers a slice on a Pie / Donut chart.
/// Distinct from <see cref="BOBChartSliceClickArgs{TY}"/> only by intent —
/// allows callers to drive secondary UI (a side panel, a stat readout) off
/// the same hover that already raises the native <c>&lt;title&gt;</c> tooltip.
/// </summary>
/// <typeparam name="TY">Numeric type of the slice value.</typeparam>
public sealed class BOBChartSliceHoverArgs<TY>
{
    /// <summary>Display label of the hovered slice.</summary>
    public string SliceLabel { get; init; } = string.Empty;

    /// <summary>Raw value of the hovered slice (pre-normalisation).</summary>
    public TY Value { get; init; } = default!;

    /// <summary>Index of the slice in declaration order (0-based).</summary>
    public int SliceIndex { get; init; }

    /// <summary>Percentage the slice represents of the total (0..100).</summary>
    public double Percentage { get; init; }
}
