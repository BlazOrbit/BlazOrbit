namespace BlazOrbit.Charts.Models;

/// <summary>
/// Edge in a <see cref="Components.BOBSankeyChart{TY}"/> dataset. Carries flow from
/// <see cref="Source"/> to <see cref="Target"/> with magnitude <see cref="Value"/>.
/// The chart auto-derives node positions from the link graph — consumers only need to
/// describe the edges.
/// </summary>
/// <typeparam name="TY">Numeric type of the link value.</typeparam>
public sealed record BOBChartSankeyLink<TY>
{
    /// <summary>Label of the source node (left side of the link).</summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>Label of the target node (right side of the link).</summary>
    public string Target { get; init; } = string.Empty;

    /// <summary>Flow magnitude. The link ribbon's thickness scales with this.</summary>
    public TY? Value { get; init; }

    /// <summary>Optional explicit link color (any valid CSS color).</summary>
    public string? Color { get; init; }
}
