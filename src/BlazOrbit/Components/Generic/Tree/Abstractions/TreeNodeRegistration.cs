using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazOrbit.Components;

/// <summary>Registration record produced by a <c>BOBTreeMenuItem</c> when it registers with its tree menu.</summary>
public sealed class TreeMenuNodeRegistration : TreeNodeRegistration
{
    /// <summary>Optional navigation target rendered as an anchor href.</summary>
    public string? Href { get; init; }
    /// <summary>Active-route match strategy applied when <see cref="Href"/> is set.</summary>
    public NavLinkMatch Match { get; init; } = NavLinkMatch.Prefix;
    /// <summary>Callback fired when the menu node is clicked.</summary>
    public EventCallback OnClick { get; init; }
    /// <summary>Optional anchor target attribute.</summary>
    public string? Target { get; init; }
}

/// <summary>Common shape for registrations contributed by tree-node child components.</summary>
public class TreeNodeRegistration
{
    /// <summary>Optional data bag forwarded to the constructed node.</summary>
    public object? Data { get; init; }
    /// <summary>Optional leading icon for the node.</summary>
    public IconKey? Icon { get; init; }
    /// <summary>When <see langword="true"/>, the node is expanded on first render.</summary>
    public bool InitiallyExpanded { get; init; }
    /// <summary>When <see langword="true"/>, the node renders disabled.</summary>
    public bool IsDisabled { get; init; }
    /// <summary>Stable key for the node.</summary>
    public required string Key { get; init; }
    /// <summary>Optional custom content fragment.</summary>
    public RenderFragment? NodeContent { get; init; }
    /// <summary>Key of the parent node, or <see langword="null"/> for roots.</summary>
    public string? ParentKey { get; init; }
    /// <summary>Display text for the node.</summary>
    public string? Text { get; init; }
}

/// <summary>Registration record produced by a <c>BOBTreeSelectorItem</c> when it registers with its selector.</summary>
public sealed class TreeSelectionNodeRegistration : TreeNodeRegistration
{
    /// <summary>When <see langword="true"/>, the node starts selected.</summary>
    public bool IsSelected { get; init; }
}