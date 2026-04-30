using BlazOrbit.Components;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class ScrollBarGenerator : IAssetGenerator
{
    public string FileName => "_scrollbar.css";
    public string Name => "ScrollBar";

    public Task<string> GetContent()
    {
        // Opt-in scope: the universal `*` selector only applies under [data-bob-scrollbars] or
        // inside .bob-scrollbars. The library never styles consumer scrollbars by default.
        string scopeAttr = $"[{FeatureDefinitions.DataAttributes.Scrollbars}]";
        string scopeClass = $".{FeatureDefinitions.CssClasses.Scrollbars}";

        return Task.FromResult($$"""
/* =========================
   SCROLLBAR (opt-in)
   Activate by adding {{FeatureDefinitions.DataAttributes.Scrollbars}} to <html>
   or the .{{FeatureDefinitions.CssClasses.Scrollbars}} class to any wrapper.
   ========================= */

:root {
    {{FeatureDefinitions.Tokens.Scrollbar.Width}}: {{FeatureDefinitions.Tokens.Scrollbar.WidthValue}};
    {{FeatureDefinitions.Tokens.Scrollbar.ThumbRadius}}: {{FeatureDefinitions.Tokens.Scrollbar.ThumbRadiusValue}};
    {{FeatureDefinitions.Tokens.Scrollbar.ThumbBorderWidth}}: {{FeatureDefinitions.Tokens.Scrollbar.ThumbBorderWidthValue}};
}

/* Firefox */
{{scopeAttr}},
{{scopeAttr}} *,
{{scopeClass}},
{{scopeClass}} * {
    scrollbar-width: thin;
    scrollbar-color: var(--palette-primary) var(--palette-surface);
}

/* WebKit (Chrome, Edge, Safari) */
{{scopeAttr}} ::-webkit-scrollbar,
{{scopeClass}} ::-webkit-scrollbar {
    width: var({{FeatureDefinitions.Tokens.Scrollbar.Width}});
    height: var({{FeatureDefinitions.Tokens.Scrollbar.Width}});
}

{{scopeAttr}} ::-webkit-scrollbar-track,
{{scopeClass}} ::-webkit-scrollbar-track {
    background: var(--palette-surface);
    border-radius: var({{FeatureDefinitions.Tokens.Scrollbar.ThumbRadius}});
}

{{scopeAttr}} ::-webkit-scrollbar-thumb,
{{scopeClass}} ::-webkit-scrollbar-thumb {
    background: var(--palette-primary);
    border-radius: var({{FeatureDefinitions.Tokens.Scrollbar.ThumbRadius}});
    border: var({{FeatureDefinitions.Tokens.Scrollbar.ThumbBorderWidth}}) solid var(--palette-surface);
}

{{scopeAttr}} ::-webkit-scrollbar-thumb:hover,
{{scopeClass}} ::-webkit-scrollbar-thumb:hover {
    background: var(--palette-secondary);
}

{{scopeAttr}} ::-webkit-scrollbar-thumb:active,
{{scopeClass}} ::-webkit-scrollbar-thumb:active {
    background: var(--palette-info);
}

{{scopeAttr}} ::-webkit-scrollbar-corner,
{{scopeClass}} ::-webkit-scrollbar-corner {
    background: var(--palette-surface);
}
""");
    }
}
