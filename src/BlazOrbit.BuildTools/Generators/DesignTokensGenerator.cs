using BlazOrbit.Components;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.BuildTools.Generators;

/// <summary>
/// Generates core design tokens: spacing, z-index, opacity. Typography is handled by
/// TypographyGenerator. Border radius, transitions, and shadows are handled by component systems.
/// </summary>
[ExcludeFromCodeCoverage]
[AssetGenerator]
public class DesignTokensGenerator : IAssetGenerator
{
    public string FileName => "_tokens.css";
    public string Name => "Design Tokens";

    public Task<string> GetContent() => Task.FromResult($$"""
:root {
    /* ========================================
       Z-INDEX SCALE
       ======================================== */
    {{FeatureDefinitions.Tokens.ZIndex.Dropdown}}: {{FeatureDefinitions.Tokens.ZIndex.DropdownValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Sticky}}: {{FeatureDefinitions.Tokens.ZIndex.StickyValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Modal}}: {{FeatureDefinitions.Tokens.ZIndex.ModalValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Tooltip}}: {{FeatureDefinitions.Tokens.ZIndex.TooltipValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Toast}}: {{FeatureDefinitions.Tokens.ZIndex.ToastValue}};

    /* ========================================
       OPACITY STATES
       ======================================== */
    {{FeatureDefinitions.Tokens.Opacity.Disabled}}: {{FeatureDefinitions.Tokens.Opacity.DisabledValue}};
    {{FeatureDefinitions.Tokens.Opacity.Placeholder}}: {{FeatureDefinitions.Tokens.Opacity.PlaceholderValue}};

    /* ========================================
       OUTLINE HIGHLIGHT
       ======================================== */
    {{FeatureDefinitions.Tokens.Highlight.Outline}}: {{FeatureDefinitions.Tokens.Highlight.OutlineValue}};
    {{FeatureDefinitions.Tokens.Highlight.OutlineOffset}}: {{FeatureDefinitions.Tokens.Highlight.OutlineOffsetValue}};

    /* ========================================
       SIZE MULTIPLIERS
       ======================================== */
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: {{FeatureDefinitions.Tokens.Size.DefaultMultiplierValue}};
    {{FeatureDefinitions.Tokens.Size.SmallMultiplier}}: {{FeatureDefinitions.Tokens.Size.SmallMultiplierValue}};
    {{FeatureDefinitions.Tokens.Size.MediumMultiplier}}: {{FeatureDefinitions.Tokens.Size.MediumMultiplierValue}};
    {{FeatureDefinitions.Tokens.Size.LargeMultiplier}}: {{FeatureDefinitions.Tokens.Size.LargeMultiplierValue}};

    /* ========================================
       DENSITY MULTIPLIERS
       ======================================== */
    {{FeatureDefinitions.ComponentVariables.Density.Multiplier}}: {{FeatureDefinitions.Tokens.Density.DefaultMultiplierValue}};
    {{FeatureDefinitions.Tokens.Density.CompactMultiplier}}: {{FeatureDefinitions.Tokens.Density.CompactMultiplierValue}};
    {{FeatureDefinitions.Tokens.Density.StandardMultiplier}}: {{FeatureDefinitions.Tokens.Density.StandardMultiplierValue}};
    {{FeatureDefinitions.Tokens.Density.ComfortableMultiplier}}: {{FeatureDefinitions.Tokens.Density.ComfortableMultiplierValue}};

    /* ========================================
       BORDER DEFAULT
       ======================================== */
    {{FeatureDefinitions.Tokens.Border.Width}}: {{FeatureDefinitions.Tokens.Border.WidthValue}};
    {{FeatureDefinitions.Tokens.Border.Style}}: {{FeatureDefinitions.Tokens.Border.StyleValue}};
    {{FeatureDefinitions.Tokens.Border.Radius}}: {{FeatureDefinitions.Tokens.Border.RadiusValue}};

    /* ========================================
       INPUT FAMILY
       ======================================== */
    {{FeatureDefinitions.Tokens.Input.Radius}}: {{FeatureDefinitions.Tokens.Input.RadiusValue}};
    {{FeatureDefinitions.Tokens.Input.TransitionDuration}}: {{FeatureDefinitions.Tokens.Input.TransitionDurationValue}};
    {{FeatureDefinitions.Tokens.Input.TransitionEasing}}: {{FeatureDefinitions.Tokens.Input.TransitionEasingValue}};
    {{FeatureDefinitions.Tokens.Input.FloatedScale}}: {{FeatureDefinitions.Tokens.Input.FloatedScaleValue}};

    /* ========================================
       PICKER FAMILY
       ======================================== */
    {{FeatureDefinitions.Tokens.Picker.Radius}}: {{FeatureDefinitions.Tokens.Picker.RadiusValue}};
    {{FeatureDefinitions.Tokens.Picker.CellSize}}: {{FeatureDefinitions.Tokens.Picker.CellSizeValue}};
    {{FeatureDefinitions.Tokens.Picker.Padding}}: {{FeatureDefinitions.Tokens.Picker.PaddingValue}};
}

{{GetRippleStyles()}}
""");

    private static string GetRippleStyles() => $$"""
/* ========================================
   RIPPLE EFFECT
   CSS-OPT-02 block A.3: per-instance overrides flow through IHasRipple via
   the FeatureDefinitions.InlineVariables.Ripple* constants. The fallback
   chain is inline override > global token > rgba/ms literal.
   ======================================== */

.{{FeatureDefinitions.CssClasses.Ripple}} {
    position: absolute;
    border-radius: 50%;
    background-color: var({{FeatureDefinitions.InlineVariables.RippleColor}}, var({{FeatureDefinitions.Tokens.Ripple.Color}}, {{FeatureDefinitions.Tokens.Ripple.ColorFallbackValue}}));
    transform: scale(0);
    animation: {{FeatureDefinitions.Tokens.Ripple.Animation}} var({{FeatureDefinitions.InlineVariables.RippleDuration}}, var({{FeatureDefinitions.Tokens.Ripple.Duration}}, {{FeatureDefinitions.Tokens.Ripple.DurationFallbackValue}})) linear;
    pointer-events: none;
}

@keyframes {{FeatureDefinitions.Tokens.Ripple.Animation}} {
    to {
        transform: scale(4);
        opacity: 0;
    }
}
""";
}
