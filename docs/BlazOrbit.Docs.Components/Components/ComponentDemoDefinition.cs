using BlazOrbit.SyntaxHighlight;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Docs.Components;

public sealed class ComponentDemoDefinition
{
    public string? Code { get; init; }
    public string? CodeTitle { get; init; }
    public RenderFragment Demo { get; init; } = default!;

    public SyntaxHighlightLanguage Language { get; init; }
        = SyntaxHighlightLanguage.Razor;
}
