using BlazOrbit.Components;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.BuildTools.Generators.Families;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class InputFamilyGenerator : IAssetGenerator
{
    public string FileName => "_input-family.css";
    public string Name => "Input Family";

    private static string V(string variable) => $"var({variable})";
    private static string V(string variable, string fallback) => $"var({variable}, {fallback})";

    public Task<string> GetContent()
    {
        string inputBase = FeatureDefinitions.DataAttributes.InputBase;
        string variant = FeatureDefinitions.DataAttributes.Variant;
        string floated = FeatureDefinitions.DataAttributes.Floated;
        string error = FeatureDefinitions.DataAttributes.Error;
        string requiredAttr = FeatureDefinitions.DataAttributes.Required;
        string sizeMult = FeatureDefinitions.ComponentVariables.Size.Multiplier;

        string inlineBg = FeatureDefinitions.InlineVariables.BackgroundColor;
        string inlineColor = FeatureDefinitions.InlineVariables.Color;
        string inlinePrefixColor = FeatureDefinitions.InlineVariables.PrefixColor;
        string inlinePrefixBg = FeatureDefinitions.InlineVariables.PrefixBackgroundColor;
        string inlineSuffixColor = FeatureDefinitions.InlineVariables.SuffixColor;
        string inlineSuffixBg = FeatureDefinitions.InlineVariables.SuffixBackgroundColor;

        string wrapper = FeatureDefinitions.CssClasses.Input.Wrapper;
        string field = FeatureDefinitions.CssClasses.Input.Field;
        string label = FeatureDefinitions.CssClasses.Input.Label;
        // The required asterisk lives on the label::after pseudo-element gated by
        // [data-bob-required="true"] (see requiredAttr below) — no .bob-input__required class.
        string helper = FeatureDefinitions.CssClasses.Input.HelperText;
        string validation = FeatureDefinitions.CssClasses.Input.Validation;
        string opacityPlaceholder = FeatureDefinitions.Tokens.Opacity.Placeholder;

        string addonPrefix = FeatureDefinitions.CssClasses.Input.AddonPrefix;
        string addonSuffix = FeatureDefinitions.CssClasses.Input.AddonSuffix;

        string outline = FeatureDefinitions.CssClasses.Input.Outline;
        string outlineLeading = FeatureDefinitions.CssClasses.Input.OutlineLeading;
        string outlineNotch = FeatureDefinitions.CssClasses.Input.OutlineNotch;
        string outlineTrailing = FeatureDefinitions.CssClasses.Input.OutlineTrailing;

        return Task.FromResult($$"""
/* ========================================
   Input Family Styles
   Auto-generated - Do not edit manually
   ======================================== */

/* === BASE === */

bob-component[{{inputBase}}] {
    position: relative;
    display: flex;
    flex-direction: column;
    width: 100%;
    min-width: var(--_input-min-width);
    gap: 0.25rem;

    --_input-min-width: 200px;
    --_input-h: calc(3.5rem * {{V(sizeMult, "1")}});
    --_input-px: 1rem;
    --_input-py: 0.75rem;
    --_input-radius: {{V(FeatureDefinitions.Tokens.Input.Radius)}};
    --_input-transition: {{V(FeatureDefinitions.Tokens.Input.TransitionDuration)}} {{V(FeatureDefinitions.Tokens.Input.TransitionEasing)}};
    --_input-scale: {{V(FeatureDefinitions.Tokens.Input.FloatedScale)}};
    --_input-floated-size: calc(1rem * var(--_input-scale) * {{V(sizeMult, "1")}});

    --_input-label-color: var(--palette-surface-contrast);
    --_input-focus-color: var(--palette-highlight);
    --_input-error-color: var(--palette-error);

    --_input-border-color: var(--palette-border);
    --_input-border-width: 1px;

    --_wrapper-bg: {{V(inlineBg, "transparent")}};
    --_wrapper-radius: var(--_input-radius);
    --_wrapper-min-h: var(--_input-h);
    --_wrapper-pt: 0px;

    --_field-px: var(--_input-px);

    --_addon-offset: 0rem;
    --_outline-leading-width: calc(var(--_input-px) - 4px + var(--_addon-offset));

    border-radius: var(--_input-radius);
}

/* === WRAPPER === */

bob-component[{{inputBase}}] .{{wrapper}} {
    position: relative;
    display: flex;
    align-items: center;
    min-height: var(--_wrapper-min-h);
    background: var(--_wrapper-bg);
    border-radius: var(--_wrapper-radius);
}

/* Local stacking inside the input wrapper — children sit above the
   absolutely-positioned outline pseudo-element within this stacking
   context. Not part of the global z-index scale. */
bob-component[{{inputBase}}] .{{wrapper}} > *:not(.{{outline}}) {
    z-index: 1;
}

/* === FIELD === */

bob-component[{{inputBase}}] .{{field}} {
    flex: 1;
    width: 100%;
    min-width: 0;
    height: 100%;
    padding-block: var(--_input-py);
    padding-inline: var(--_field-px);
    padding-block-start: calc(var(--_input-py) + var(--_wrapper-pt));
    border: none;
    background: transparent;
    font: inherit;
    color: {{V(inlineColor, "inherit")}};
    outline: none;
}

bob-component[{{inputBase}}] .{{field}}::placeholder {
    color: var(--_input-label-color);
    opacity: 0;
    transition: opacity var(--_input-transition);
}

bob-component[{{inputBase}}][{{floated}}="true"] .{{field}}::placeholder {
    opacity: {{V(opacityPlaceholder)}};
}

/* === OUTLINE SYSTEM === */

bob-component[{{inputBase}}] .{{outline}} {
    position: absolute;
    inset: 0;
    display: flex;
    pointer-events: none;
}

bob-component[{{inputBase}}] .{{outlineLeading}} {
    width: var(--_outline-leading-width);
    border: var(--_input-border-width) solid var(--_input-border-color);
    border-inline-end: none;
    border-radius: var(--_input-radius) 0 0 var(--_input-radius);
    transition: border-color var(--_input-transition), border-width var(--_input-transition);
}

bob-component[{{inputBase}}] .{{outlineNotch}} {
    position: relative;
    display: flex;
    flex-direction: column;
    border-block-start: var(--_input-border-width) solid var(--_input-border-color);
    border-block-end: var(--_input-border-width) solid var(--_input-border-color);
    transition: border-color var(--_input-transition), border-width var(--_input-transition);
}

bob-component[{{inputBase}}] .{{outlineTrailing}} {
    flex: 1;
    border: var(--_input-border-width) solid var(--_input-border-color);
    border-inline-start: none;
    border-radius: 0 var(--_input-radius) var(--_input-radius) 0;
    transition: border-color var(--_input-transition), border-width var(--_input-transition);
}

/* === LABEL === */

bob-component[{{inputBase}}] .{{label}} {
    display: inline-flex;
    align-items: center;
    padding-inline: 4px;
    font-size: 1rem;
    line-height: 1;
    color: var(--_input-label-color);
    white-space: nowrap;
    pointer-events: none;
    transition: font-size var(--_input-transition), transform var(--_input-transition), color var(--_input-transition), padding var(--_input-transition);
}

/* Required asterisk is rendered by the CSS pseudo-element gated on
   [data-bob-required="true"] instead of a Razor-emitted <span>. The underlying
   class name is kept in FeatureDefinitions for back-compat with any consumer
   override that targets it directly. */
bob-component[{{inputBase}}][{{requiredAttr}}="true"] .{{label}}::after {
    content: " *";
    color: var(--_input-error-color);
    margin-inline-start: 0.25em;
}

bob-component[{{inputBase}}]:focus-within .{{label}} {
    color: var(--_input-focus-color);
}

bob-component[{{inputBase}}][{{error}}="true"] .{{label}} {
    color: var(--_input-error-color);
}

/* === HELPER & VALIDATION === */

bob-component[{{inputBase}}] .{{helper}} {
    font-size: 0.75rem;
    color: var(--_input-label-color);
    opacity: 0.7;
    padding-inline-start: var(--_input-px);
}

bob-component[{{inputBase}}] .{{validation}} {
    font-size: 0.75rem;
    color: var(--_input-error-color);
    padding-inline-start: var(--_input-px);
}

/* === ADDON PREFIX ===
   Prefix color/background overrides flow through the IHasPrefix interface; the
   inline custom-properties below mirror the FeatureDefinitions.InlineVariables.Prefix*
   constants. Consumers (or per-component razor.css) can also override the
   private vars directly. */

bob-component[{{inputBase}}] .{{addonPrefix}} {
    --_input-prefix-color: {{V(inlinePrefixColor, "currentColor")}};
    --_input-prefix-bg: {{V(inlinePrefixBg, "transparent")}};
    order: -1;
    color: var(--_input-prefix-color);
    background-color: var(--_input-prefix-bg);
    border-inline-start: none;
    border-inline-end: 1px solid var(--_input-border-color);
}

bob-component[{{inputBase}}]:has(.{{addonPrefix}}) {
    --_addon-offset: calc(2.5rem * {{V(sizeMult, "1")}});
}

/* === ADDON SUFFIX ===
   Same fallback chain as ADDON PREFIX, but for suffix. */

bob-component[{{inputBase}}] .{{addonSuffix}} {
    --_input-suffix-color: {{V(inlineSuffixColor, "currentColor")}};
    --_input-suffix-bg: {{V(inlineSuffixBg, "transparent")}};
    order: 1;
    color: var(--_input-suffix-color);
    background-color: var(--_input-suffix-bg);
    border-inline-start: 1px solid var(--_input-border-color);
    border-inline-end: none;
}

bob-component[{{inputBase}}]:has(.{{addonSuffix}}) {
    --_addon-offset-end: calc(2.5rem * {{V(sizeMult, "1")}});
}

/* ========================================
   VARIANT: OUTLINED
   ======================================== */

/* Label: resting state */
bob-component[{{inputBase}}][{{variant}}="outlined"] .{{label}} {
    transform: translateY(calc((var(--_input-h) / 2) - 0.5em));
}

/* Label: floated state */
bob-component[{{inputBase}}][{{variant}}="outlined"][{{floated}}="true"] .{{label}} {
    font-size: var(--_input-floated-size);
    transform: translateY(-50%);
    padding-inline: 4px;
}

/* Notch: open when floated */
bob-component[{{inputBase}}][{{variant}}="outlined"][{{floated}}="true"] .{{outlineNotch}} {
    border-block-start-color: transparent;
}

/* Focus state */
bob-component[{{inputBase}}][{{variant}}="outlined"]:focus-within .{{outlineLeading}},
bob-component[{{inputBase}}][{{variant}}="outlined"]:focus-within .{{outlineNotch}},
bob-component[{{inputBase}}][{{variant}}="outlined"]:focus-within .{{outlineTrailing}} {
    border-color: var(--_input-focus-color);
    border-width: 2px;
}

bob-component[{{inputBase}}][{{variant}}="outlined"]:focus-within[{{floated}}="true"] .{{outlineNotch}} {
    border-block-start-color: transparent;
}

/* Error state */
bob-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"] .{{outlineLeading}},
bob-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"] .{{outlineNotch}},
bob-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"] .{{outlineTrailing}} {
    border-color: var(--_input-error-color);
}

bob-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"][{{floated}}="true"] .{{outlineNotch}} {
    border-block-start-color: transparent;
}

bob-component[{{inputBase}}][{{variant}}="outlined"] .{{addonPrefix}} {
    border-radius: 0;
    border-start-start-radius: var(--_input-radius);
    border-end-start-radius: var(--_input-radius);
}

bob-component[{{inputBase}}][{{variant}}="outlined"] .{{addonSuffix}} {
    border-radius: 0;
    border-start-end-radius: var(--_input-radius);
    border-end-end-radius: var(--_input-radius);
}

/* ========================================
   VARIANT: FILLED
   ======================================== */

bob-component[{{inputBase}}][{{variant}}="filled"] {
    --_wrapper-bg: {{V(inlineBg, "color-mix(in srgb, var(--palette-surface-contrast) 6%, transparent)")}};
    --_wrapper-radius: var(--_input-radius) var(--_input-radius) 0 0;
    --_wrapper-pt: 0px;
    --_outline-leading-width: calc(var(--_input-px) + var(--_addon-offset));
}

bob-component[{{inputBase}}][{{variant}}="filled"]:has(.{{label}}) {
    --_wrapper-pt: calc(0.75rem * {{V(sizeMult, "1")}});
}

/* Outline: only bottom border */
bob-component[{{inputBase}}][{{variant}}="filled"] .{{outlineLeading}},
bob-component[{{inputBase}}][{{variant}}="filled"] .{{outlineTrailing}} {
    border: none;
    border-block-end: var(--_input-border-width) solid var(--_input-border-color);
    border-radius: 0;
}

bob-component[{{inputBase}}][{{variant}}="filled"] .{{outlineNotch}} {
    border-block-start: none;
    align-items: flex-start;
}

/* Label: resting state */
bob-component[{{inputBase}}][{{variant}}="filled"] .{{label}} {
    transform: translateY(calc((var(--_input-h) + var(--_wrapper-pt)) / 2 - 0.5em));
}

/* Label: floated state */
bob-component[{{inputBase}}][{{variant}}="filled"][{{floated}}="true"] .{{label}} {
    font-size: var(--_input-floated-size);
    transform: translateY(calc(var(--_wrapper-pt) * 0.5));
}

/* Focus state */
bob-component[{{inputBase}}][{{variant}}="filled"]:focus-within .{{outlineLeading}},
bob-component[{{inputBase}}][{{variant}}="filled"]:focus-within .{{outlineNotch}},
bob-component[{{inputBase}}][{{variant}}="filled"]:focus-within .{{outlineTrailing}} {
    border-block-end-color: var(--_input-focus-color);
    border-block-end-width: 2px;
}

/* Error state */
bob-component[{{inputBase}}][{{variant}}="filled"][{{error}}="true"] .{{outlineLeading}},
bob-component[{{inputBase}}][{{variant}}="filled"][{{error}}="true"] .{{outlineNotch}},
bob-component[{{inputBase}}][{{variant}}="filled"][{{error}}="true"] .{{outlineTrailing}} {
    border-block-end-color: var(--_input-error-color);
}

bob-component[{{inputBase}}][{{variant}}="filled"] .{{addonPrefix}} {
    border-radius: 0;
    border-start-start-radius: var(--_input-radius);
}

bob-component[{{inputBase}}][{{variant}}="filled"] .{{addonSuffix}} {
    border-radius: 0;
    border-start-end-radius: var(--_input-radius);
}

/* ========================================
   VARIANT: STANDARD
   ======================================== */

bob-component[{{inputBase}}][{{variant}}="standard"] {
    --_wrapper-pt: 0px;
    --_wrapper-radius: 0;
    --_outline-leading-width: var(--_addon-offset);
}

bob-component[{{inputBase}}][{{variant}}="standard"]:has(.{{label}}) {
    --_wrapper-pt: calc(1rem * {{V(sizeMult, "1")}});
}

/* Outline: only bottom border */
bob-component[{{inputBase}}][{{variant}}="standard"] .{{outlineLeading}} {
    border: none;
    border-block-end: var(--_input-border-width) solid var(--_input-border-color);
    border-radius: 0;
}

bob-component[{{inputBase}}][{{variant}}="standard"] .{{outlineNotch}} {
    border-block-start: none;
    align-items: flex-start;
}

bob-component[{{inputBase}}][{{variant}}="standard"] .{{outlineTrailing}} {
    border: none;
    border-block-end: var(--_input-border-width) solid var(--_input-border-color);
    border-radius: 0;
}

bob-component[{{inputBase}}][{{variant}}="standard"] .{{label}} {
    padding-inline-start: 0;
}

/* Label: resting state */
bob-component[{{inputBase}}][{{variant}}="standard"] .{{label}} {
    transform: translateY(calc((var(--_input-h) + var(--_wrapper-pt)) / 2 - 0.5em));
}

/* Label: floated state */
bob-component[{{inputBase}}][{{variant}}="standard"][{{floated}}="true"] .{{label}} {
    font-size: var(--_input-floated-size);
    transform: translateY(calc(var(--_wrapper-pt) * 0.25));
}

/* Focus state */
bob-component[{{inputBase}}][{{variant}}="standard"]:focus-within .{{outlineLeading}},
bob-component[{{inputBase}}][{{variant}}="standard"]:focus-within .{{outlineNotch}},
bob-component[{{inputBase}}][{{variant}}="standard"]:focus-within .{{outlineTrailing}} {
    border-block-end-color: var(--_input-focus-color);
    border-block-end-width: 2px;
}

/* Error state */
bob-component[{{inputBase}}][{{variant}}="standard"][{{error}}="true"] .{{outlineLeading}},
bob-component[{{inputBase}}][{{variant}}="standard"][{{error}}="true"] .{{outlineNotch}},
bob-component[{{inputBase}}][{{variant}}="standard"][{{error}}="true"] .{{outlineTrailing}} {
    border-block-end-color: var(--_input-error-color);
}

/* Helper/validation no padding */
bob-component[{{inputBase}}][{{variant}}="standard"] .{{helper}},
bob-component[{{inputBase}}][{{variant}}="standard"] .{{validation}} {
    padding-inline-start: 0;
}

/* ========================================
   VARIANT: FLAT
   ======================================== */

bob-component[{{inputBase}}][{{variant}}="flat"] {
    --_wrapper-pt: 0px;
    --_wrapper-radius: 0;
    --_wrapper-bg: {{V(inlineBg, "transparent")}};
    --_outline-leading-width: var(--_addon-offset);
}

bob-component[{{inputBase}}][{{variant}}="flat"]:has(.{{label}}) {
    --_wrapper-pt: calc(1rem * {{V(sizeMult, "1")}});
}

/* Outline: sin ningún borde */
bob-component[{{inputBase}}][{{variant}}="flat"] .{{outlineLeading}},
bob-component[{{inputBase}}][{{variant}}="flat"] .{{outlineNotch}},
bob-component[{{inputBase}}][{{variant}}="flat"] .{{outlineTrailing}} {
    border: none;
}

/* Label: resting state */
bob-component[{{inputBase}}][{{variant}}="flat"] .{{label}} {
    transform: translateY(calc((var(--_input-h) + var(--_wrapper-pt)) / 2 - 0.5em));
}

/* Label: floated state */
bob-component[{{inputBase}}][{{variant}}="flat"][{{floated}}="true"] .{{label}} {
    font-size: var(--_input-floated-size);
    transform: translateY(calc(var(--_wrapper-pt) * 0.25));
}

/* Focus state: sin borde, solo cambio de color en label (ya cubierto por :focus-within .label) */

/* Error state */
bob-component[{{inputBase}}][{{variant}}="flat"][{{error}}="true"] .{{label}} {
    color: var(--_input-error-color);
}

/* Helper/validation sin padding lateral */
bob-component[{{inputBase}}][{{variant}}="flat"] .{{helper}},
bob-component[{{inputBase}}][{{variant}}="flat"] .{{validation}} {
    padding-inline-start: 0;
}

""");
    }
}