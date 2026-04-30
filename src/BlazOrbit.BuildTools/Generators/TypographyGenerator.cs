using BlazOrbit.Components;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class TypographyGenerator : IAssetGenerator
{
    public string FileName => "_typography.css";
    public string Name => "Typography";

    public Task<string> GetContent() => Task.FromResult($$"""
/* ========================================
   Typography System
   Auto-generated - Do not edit manually
   ======================================== */

:root {
    {{FeatureDefinitions.Typography.FontFamily}}: {{FeatureDefinitions.Typography.FontFamilyValue}};
    {{FeatureDefinitions.Typography.FontFamilyHeading}}: {{FeatureDefinitions.Typography.FontFamilyHeadingValue}};
    {{FeatureDefinitions.Typography.FontMono}}: {{FeatureDefinitions.Typography.FontMonoValue}};

    /* Fluid typography: scales from 0.875rem (640px) to 1.125rem (1536px) */
    {{FeatureDefinitions.Typography.FontSizeBase}}: {{FeatureDefinitions.Typography.FontSizeBaseValue}};

    {{FeatureDefinitions.Typography.LineHeight}}: {{FeatureDefinitions.Typography.LineHeightValue}};
    {{FeatureDefinitions.Typography.LineHeightHeading}}: {{FeatureDefinitions.Typography.LineHeightHeadingValue}};
}

/* ========================================
   Headings - Scale: 1.25 ratio (Major Third)
   ======================================== */

h1, h2, h3, h4, h5, h6 {
    font-family: var({{FeatureDefinitions.Typography.FontFamilyHeading}});
    font-weight: {{FeatureDefinitions.Typography.HeadingFontWeight}};
    line-height: var({{FeatureDefinitions.Typography.LineHeightHeading}});
}

h1 { font-size: {{FeatureDefinitions.Typography.H1FontSize}}; }
h2 { font-size: {{FeatureDefinitions.Typography.H2FontSize}}; }
h3 { font-size: {{FeatureDefinitions.Typography.H3FontSize}}; }
h4 { font-size: {{FeatureDefinitions.Typography.H4FontSize}}; }
h5 { font-size: {{FeatureDefinitions.Typography.H5FontSize}}; }
h6 { font-size: {{FeatureDefinitions.Typography.H6FontSize}}; }

/* ========================================
   Paragraph & Inline Text
   ======================================== */

p { line-height: var({{FeatureDefinitions.Typography.LineHeight}}); }
small { font-size: {{FeatureDefinitions.Typography.SmallFontSize}}; }
strong, b { font-weight: {{FeatureDefinitions.Typography.BoldFontWeight}}; }
em, i { font-style: italic; }

mark {
    background-color: var(--palette-highlight);
    padding-inline: 0.25em;
    padding-block: 0.125em;
}

/* ========================================
   Links
   ======================================== */

a {
    color: var(--palette-primary);
    transition: {{FeatureDefinitions.Typography.LinkTransitionValue}};
}

a:hover { text-decoration: underline; }

a:focus-visible {
    outline: var({{FeatureDefinitions.Tokens.Highlight.Outline}});
    outline-offset: var({{FeatureDefinitions.Tokens.Highlight.OutlineOffset}});
}

/* ========================================
   Code & Preformatted
   ======================================== */

code, kbd, samp {
    font-family: var({{FeatureDefinitions.Typography.FontMono}});
    font-size: calc({{FeatureDefinitions.Typography.SmallFontSize}} * var({{FeatureDefinitions.ComponentVariables.Size.Multiplier}}, 1));
    padding-inline: 0.375em;
    padding-block: 0.125em;
    background-color: var(--palette-surface);
    font-weight: {{FeatureDefinitions.Typography.CodeFontWeight}};
}

pre {
    font-family: var({{FeatureDefinitions.Typography.FontMono}});
    font-size: calc({{FeatureDefinitions.Typography.SmallFontSize}} * var({{FeatureDefinitions.ComponentVariables.Size.Multiplier}}, 1));
    line-height: {{FeatureDefinitions.Typography.PreLineHeight}};
    padding: 1em;
    overflow-x: auto;
    white-space: pre;
}

pre code {
    background-color: transparent;
    padding: 0;
}

/* ========================================
   Blockquote & HR
   ======================================== */

blockquote {
    border-inline-start: 4px solid var(--palette-primary);
    padding-inline-start: 1em;
    font-style: italic;
    color: var(--palette-surface-contrast);
}

hr {
    border: none;
    border-block-start: var({{FeatureDefinitions.Tokens.Border.Width}}) var({{FeatureDefinitions.Tokens.Border.Style}}) var(--palette-border, currentColor);
    opacity: {{FeatureDefinitions.Typography.HrOpacity}};
}

/* ========================================
   Selection
   ======================================== */

::selection {
    background-color: var(--palette-primary);
    color: var(--palette-primary-contrast);
}
""");
}
