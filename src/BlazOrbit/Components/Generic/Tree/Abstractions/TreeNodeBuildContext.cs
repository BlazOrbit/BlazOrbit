using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace BlazOrbit.Components;

/// <summary>
/// Context object passed to the node factory while building a tree.
/// Plumbing for <see cref="TreeStructure{TNode, TItem}"/>; not user-facing.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class TreeNodeBuildContext<TItem>
{
    public object? AdditionalData { get; init; }
    public RenderFragment? CustomContent { get; init; }
    public int Depth { get; init; }
    public bool HasChildren { get; init; }
    public IconKey? Icon { get; init; }
    public bool IsDisabled { get; init; }
    public TItem? Item { get; init; }
    public required string Key { get; init; }
    public object? Parent { get; init; }
    public string? Text { get; init; }
}