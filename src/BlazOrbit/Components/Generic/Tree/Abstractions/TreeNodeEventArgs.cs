namespace BlazOrbit.Components;

/// <summary>Event payload raised by tree components for node-level interactions.</summary>
public sealed class TreeNodeEventArgs<TNode> : EventArgs
    where TNode : ITreeNode
{
    /// <summary>Depth of the affected node.</summary>
    public int Depth { get; init; }
    /// <summary><see langword="true"/> when the node is currently expanded.</summary>
    public bool IsExpanded { get; init; }
    /// <summary>Stable key of the affected node.</summary>
    public required string Key { get; init; }
    /// <summary>Affected node instance.</summary>
    public required TNode Node { get; init; }
}