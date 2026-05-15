namespace BlazOrbit.Charts.Models;

/// <summary>
/// Hierarchical leaf or branch in a <see cref="Components.BOBTreemapChart{TY}"/> /
/// <see cref="Components.BOBSunburstChart{TY}"/> dataset. Branch nodes (those with a
/// non-empty <see cref="Children"/>) take their magnitude from the descendant sum;
/// leaves (Children null/empty) use <see cref="Value"/> directly.
/// </summary>
/// <typeparam name="TY">Numeric type of the node value.</typeparam>
public sealed record BOBChartTreemapNode<TY>
{
    /// <summary>Display label shown at the node's centroid when the rect is large enough.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// Leaf magnitude. Required for leaves (nodes with null/empty <see cref="Children"/>).
    /// Ignored on branch nodes - the renderer aggregates descendants instead, so set
    /// <see cref="Children"/> to drive a branch and leave <see cref="Value"/> at the
    /// default.
    /// </summary>
    public TY? Value { get; init; }

    /// <summary>
    /// Optional explicit fill colour (any valid CSS color). When <see langword="null"/>
    /// the renderer cycles through the chart theme palette.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Optional child nodes. A non-null, non-empty list marks this node as a branch and
    /// triggers recursive subdivision of the node's rect.
    /// </summary>
    public IReadOnlyList<BOBChartTreemapNode<TY>>? Children { get; init; }
}
