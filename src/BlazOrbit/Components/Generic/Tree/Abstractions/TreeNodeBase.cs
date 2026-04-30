using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components;

public abstract class TreeNodeBase<TItem, TSelf> : ITreeNode<TItem>
    where TSelf : TreeNodeBase<TItem, TSelf>
{
    public object? AdditionalData { get; init; }
    IReadOnlyList<ITreeNode> ITreeNode.Children => ChildrenInternal;
    IReadOnlyList<ITreeNode<TItem>> ITreeNode<TItem>.Children => ChildrenInternal;
    public RenderFragment? CustomContent { get; init; }
    public int Depth { get; init; }
    public bool HasChildren => HasChildrenFlag || ChildrenInternal.Count > 0;
    public IconKey? Icon { get; init; }
    public bool IsDisabled { get; init; }
    public TItem? Item { get; init; }
    public required string Key { get; init; }
    ITreeNode? ITreeNode.Parent => ParentNode;
    ITreeNode<TItem>? ITreeNode<TItem>.Parent => ParentNode;
    public TSelf? ParentNode { get; init; }
    public string? Text { get; init; }
    internal List<TSelf> ChildrenInternal { get; } = [];
    internal bool HasChildrenFlag { get; set; }
}