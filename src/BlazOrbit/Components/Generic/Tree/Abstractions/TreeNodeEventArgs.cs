namespace BlazOrbit.Components;

public sealed class TreeNodeEventArgs<TNode> : EventArgs
    where TNode : ITreeNode
{
    public int Depth { get; init; }
    public bool IsExpanded { get; init; }
    public required string Key { get; init; }
    public required TNode Node { get; init; }
}