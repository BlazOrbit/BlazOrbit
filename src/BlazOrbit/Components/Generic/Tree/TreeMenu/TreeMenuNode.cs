using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components;

/// <summary>Concrete tree-node type used by <c>BOBTreeMenu</c>.</summary>
public sealed class TreeMenuNode<TItem> : TreeNodeBase<TItem, TreeMenuNode<TItem>>
{
    /// <summary>Optional navigation metadata — when set, the node renders as a link.</summary>
    public NavigationInfo Navigation { get; init; } = new();
    /// <summary>Callback fired when the node is clicked.</summary>
    public EventCallback OnClick { get; init; }
}