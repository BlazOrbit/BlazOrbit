namespace BlazOrbit.Charts.Models;

/// <summary>
/// Event payload raised when the user clicks a treemap / sunburst node. Carries the
/// hierarchical path (root-to-node labels) so consumers can drill into the underlying
/// dataset without having to walk the tree themselves.
/// </summary>
/// <typeparam name="TY">Numeric type of the node value.</typeparam>
/// <param name="Node">The clicked node (leaf or branch).</param>
/// <param name="Path">Ordered labels from the root down to the clicked node, inclusive.</param>
/// <param name="Depth">0-based depth - root nodes at 0, their children at 1, etc.</param>
public sealed record BOBChartTreemapClickArgs<TY>(
    BOBChartTreemapNode<TY> Node,
    IReadOnlyList<string> Path,
    int Depth);
