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
    /// <summary>Consumer-supplied data bag forwarded to the node.</summary>
    public object? AdditionalData { get; init; }
    /// <summary>Optional custom render fragment.</summary>
    public RenderFragment? CustomContent { get; init; }
    /// <summary>Depth of the node being built.</summary>
    public int Depth { get; init; }
    /// <summary><see langword="true"/> when children exist (eager or lazy).</summary>
    public bool HasChildren { get; init; }
    /// <summary>Optional leading icon.</summary>
    public IconKey? Icon { get; init; }
    /// <summary><see langword="true"/> when the node should render disabled.</summary>
    public bool IsDisabled { get; init; }
    /// <summary>Payload attached to the node.</summary>
    public TItem? Item { get; init; }
    /// <summary>Stable key for the node.</summary>
    public required string Key { get; init; }
    /// <summary>Parent node reference, untyped (boxed).</summary>
    public object? Parent { get; init; }
    /// <summary>Display text for the node.</summary>
    public string? Text { get; init; }
}