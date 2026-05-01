using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components;

/// <summary>Base class for concrete tree-node records that carry a payload of <typeparamref name="TItem"/>.</summary>
public abstract class TreeNodeBase<TItem, TSelf> : ITreeNode<TItem>
    where TSelf : TreeNodeBase<TItem, TSelf>
{
    /// <summary>Optional consumer-supplied data bag attached to the node.</summary>
    public object? AdditionalData { get; init; }
    IReadOnlyList<ITreeNode> ITreeNode.Children => ChildrenInternal;
    IReadOnlyList<ITreeNode<TItem>> ITreeNode<TItem>.Children => ChildrenInternal;
    /// <summary>Custom content rendered in place of the default text + icon.</summary>
    public RenderFragment? CustomContent { get; init; }
    /// <summary>Distance from the root (root nodes have depth 0).</summary>
    public int Depth { get; init; }
    /// <summary><see langword="true"/> when the node has children, including not-yet-loaded ones.</summary>
    public bool HasChildren => HasChildrenFlag || ChildrenInternal.Count > 0;
    /// <summary>Optional leading icon for the node.</summary>
    public IconKey? Icon { get; init; }
    /// <summary><see langword="true"/> when the node cannot be selected or expanded.</summary>
    public bool IsDisabled { get; init; }
    /// <summary>Payload attached to the node.</summary>
    public TItem? Item { get; init; }
    /// <summary>Stable identifier within the tree.</summary>
    public required string Key { get; init; }
    ITreeNode? ITreeNode.Parent => ParentNode;
    ITreeNode<TItem>? ITreeNode<TItem>.Parent => ParentNode;
    /// <summary>Strongly-typed parent node, or <see langword="null"/> for roots.</summary>
    public TSelf? ParentNode { get; init; }
    /// <summary>Display text rendered next to the icon.</summary>
    public string? Text { get; init; }
    internal List<TSelf> ChildrenInternal { get; } = [];
    internal bool HasChildrenFlag { get; set; }
}