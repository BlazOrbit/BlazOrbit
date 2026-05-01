using BlazOrbit.Components;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BlazOrbit.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class BaseComponentGenerator : IAssetGenerator
{
    public string FileName => "_base.css";
    public string Name => "Base Component";

    public Task<string> GetContent()
    {
        StringBuilder sb = new();
        sb.AppendLine(GetBaseStyles());
        sb.AppendLine();
        sb.AppendLine(GetSizeSystem());
        sb.AppendLine();
        sb.AppendLine(GetDensitySystem());
        sb.AppendLine();
        sb.AppendLine(GetStateStyles());
        sb.AppendLine();
        sb.AppendLine(GetFocusSystem());
        sb.AppendLine();
        sb.AppendLine(GetShadowSystem());
        sb.AppendLine();
        sb.AppendLine(GetUtilities());
        sb.AppendLine();
        sb.AppendLine(GetReducedMotion());
        return Task.FromResult(sb.ToString());
    }

    private static string GetBaseStyles() => $$"""
/* ========================================
   Base Component Styles
   Auto-generated - Do not edit manually
   ======================================== */

{{FeatureDefinitions.Tags.Component}} {
    display: inline-flex;
    box-sizing: border-box;
    font-family: inherit;
    font-size: calc(1rem * var(--bob-size-multiplier, 1));
    line-height: inherit;
    gap: calc(0.5rem * var(--bob-density-multiplier, 1));
}
""";

    private static string GetDensitySystem() => $$"""
/* ========================================
   DENSITY SYSTEM
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="compact"] {
    {{FeatureDefinitions.ComponentVariables.Density.Multiplier}}: var(--bob-compact-multiplier, 0.5);
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="standard"] {
    {{FeatureDefinitions.ComponentVariables.Density.Multiplier}}: var(--bob-standard-multiplier, 1);
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="comfortable"] {
    {{FeatureDefinitions.ComponentVariables.Density.Multiplier}}: var(--bob-comfortable-multiplier, 1.5);
}
""";

    private static string GetSizeSystem() => $$"""
/* ========================================
   SIZE SYSTEM
   Uses CSS variable for consistent scaling.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="small"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: var(--bob-small-multiplier);
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="medium"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: var(--bob-medium-multiplier);
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="large"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: var(--bob-large-multiplier);
}
""";

    private static string GetStateStyles() => $$"""
/* ========================================
   STATE STYLES
   Disabled, loading, error, etc.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Disabled}}="true"] {
    opacity: var(--bob-opacity-disabled, 0.5);
    pointer-events: none;
    cursor: not-allowed;
}

/* Loading hosts get a wait cursor and stop accepting pointer events. The
   visual content (spinner / dimming) is still owned by each component's
   BOBLoadingIndicator child render. */
{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Loading}}="true"] {
    cursor: wait;
    pointer-events: none;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.FullWidth}}="true"] {
    width: 100%;
}
""";

    private static string GetFocusSystem() => $$"""
/* ========================================
   FOCUS SYSTEM
   WCAG 2.4.7 Focus Visible.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}:focus-visible,
{{FeatureDefinitions.Tags.Component}} :focus-visible {
    outline: var(--bob-highlight-outline);
    outline-offset: var(--bob-highlight-outline-offset);
}
""";

    private static string GetShadowSystem() => $$"""
/* ========================================
   SHADOW SYSTEM
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Shadow}}="true"] {
    box-shadow: var({{FeatureDefinitions.InlineVariables.Shadow}});
}
""";

    private static string GetUtilities() => $$"""
/* ========================================
   UTILITY CLASSES
   ======================================== */

.bob-field__required {
    color: var(--palette-error, #d32f2f);
    margin-inline-start: 0.25em;
}

.bob-field__validation {
    font-size: 0.75rem;
    color: var(--palette-error, #d32f2f);
}

.bob-field__helper {
    font-size: 0.75rem;
    color: var(--palette-surface-contrast);
    opacity: 0.7;
}

.sr-only {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border: 0;
}
""";

    // WCAG 2.2 AA 2.3.3 Animation from Interactions. The 0.01ms duration
    // (instead of 0) preserves animationend / transitionend callbacks where the
    // library uses them. Functional close timers (BOBToast, BOBDialog, BOBDrawer)
    // are awaiteable Task.Delay calls independent of CSS animation duration, so
    // they keep working unchanged.
    private static string GetReducedMotion() => $$"""
/* ========================================
   REDUCED MOTION
   WCAG 2.3.3 Animation from Interactions.
   ======================================== */

@media (prefers-reduced-motion: reduce) {
    *,
    *::before,
    *::after {
        animation-duration: 0.01ms !important;
        animation-iteration-count: 1 !important;
        transition-duration: 0.01ms !important;
        scroll-behavior: auto !important;
    }
}
""";
}