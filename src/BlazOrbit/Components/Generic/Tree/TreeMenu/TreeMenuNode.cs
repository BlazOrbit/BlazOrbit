using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components;

public sealed class TreeMenuNode<TItem> : TreeNodeBase<TItem, TreeMenuNode<TItem>>
{
    public NavigationInfo Navigation { get; init; } = new();
    public EventCallback OnClick { get; init; }
}