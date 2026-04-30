using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazOrbit.Components;

public sealed class TreeMenuNodeRegistration : TreeNodeRegistration
{
    public string? Href { get; init; }
    public NavLinkMatch Match { get; init; } = NavLinkMatch.Prefix;
    public EventCallback OnClick { get; init; }
    public string? Target { get; init; }
}

public class TreeNodeRegistration
{
    public object? Data { get; init; }
    public IconKey? Icon { get; init; }
    public bool InitiallyExpanded { get; init; }
    public bool IsDisabled { get; init; }
    public required string Key { get; init; }
    public RenderFragment? NodeContent { get; init; }
    public string? ParentKey { get; init; }
    public string? Text { get; init; }
}

public sealed class TreeSelectionNodeRegistration : TreeNodeRegistration
{
    public bool IsSelected { get; init; }
}