using BlazOrbit.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Components;

public sealed class TreeSelectionNode<TItem> : TreeNodeBase<TItem, TreeSelectionNode<TItem>>, IHierarchicalSelectionOption
{
    IReadOnlyList<IHierarchicalSelectionOption> IHierarchicalSelectionOption.Children
        => ChildrenInternal.Cast<IHierarchicalSelectionOption>().ToList();

    RenderFragment? ISelectionOption.Content => CustomContent;
    int IHierarchicalSelectionOption.Depth => Depth;
    string ISelectionOption.DisplayText => Text ?? Item?.ToString() ?? Key;
    bool ISelectionOption.IsDisabled => IsDisabled;
    string IHierarchicalSelectionOption.Key => Key;
    IHierarchicalSelectionOption? IHierarchicalSelectionOption.Parent => ParentNode;
    object? ISelectionOption.Value => Item;
}