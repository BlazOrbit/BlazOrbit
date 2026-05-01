namespace BlazOrbit.Components;

/// <summary>Non-generic projection of a tree node — used by infrastructure that walks heterogeneous trees.</summary>
public interface ITreeNode
{
    /// <summary>Direct child nodes.</summary>
    IReadOnlyList<ITreeNode> Children { get; }
    /// <summary>Distance from the root (root nodes have depth 0).</summary>
    int Depth { get; }
    /// <summary><see langword="true"/> when the node has any children — eagerly loaded or lazy.</summary>
    bool HasChildren { get; }
    /// <summary>Optional leading icon for the node.</summary>
    IconKey? Icon { get; }
    /// <summary><see langword="true"/> when the node cannot be selected or expanded.</summary>
    bool IsDisabled { get; }
    /// <summary>Stable identifier within the tree.</summary>
    string Key { get; }
    /// <summary>Parent node, or <see langword="null"/> for root nodes.</summary>
    ITreeNode? Parent { get; }
    /// <summary>Display text rendered next to the icon.</summary>
    string? Text { get; }
}

/// <summary>Strongly-typed tree node carrying a per-node item payload.</summary>
public interface ITreeNode<TItem> : ITreeNode
{
    /// <summary>Direct child nodes, typed with the same payload.</summary>
    new IReadOnlyList<ITreeNode<TItem>> Children { get; }
    /// <summary>Payload attached to the node (e.g. the source domain object).</summary>
    TItem? Item { get; }
    /// <summary>Parent node, or <see langword="null"/> for root nodes.</summary>
    new ITreeNode<TItem>? Parent { get; }
}