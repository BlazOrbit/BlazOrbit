namespace BlazOrbit.Charts.Models;

/// <summary>
/// Event args fired when the user clicks a slice on a Pie / Donut chart.
/// </summary>
/// <typeparam name="TY">Numeric type of the slice value.</typeparam>
public sealed class BOBChartSliceClickArgs<TY>
{
    /// <summary>Display label of the clicked slice.</summary>
    public string SliceLabel { get; init; } = string.Empty;

    /// <summary>Raw value of the clicked slice (pre-normalisation).</summary>
    public TY Value { get; init; } = default!;

    /// <summary>Index of the slice in declaration order (0-based).</summary>
    public int SliceIndex { get; init; }

    /// <summary>Percentage the slice represents of the total (0..100).</summary>
    public double Percentage { get; init; }
}