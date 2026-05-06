using BlazOrbit.Components;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.BuildTools.Generators.Families;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class PickerFamilyCssGenerator : IAssetGenerator
{
    public string FileName => "_picker-family.css";
    public string Name => "Picker Family";

    private static string V(string variable) => $"var({variable})";
    private static string V(string variable, string fallback) => $"var({variable}, {fallback})";

    public Task<string> GetContent()
    {
        string picker = FeatureDefinitions.DataAttributes.PickerBase;
        string sizeMult = FeatureDefinitions.ComponentVariables.Size.Multiplier;
        string row = FeatureDefinitions.CssClasses.Picker.Row;
        string title = FeatureDefinitions.CssClasses.Picker.Title;
        string grid = FeatureDefinitions.CssClasses.Picker.Grid;
        string cell = FeatureDefinitions.CssClasses.Picker.Cell;
        string cellSelected = FeatureDefinitions.CssClasses.Picker.CellSelected;
        string input = FeatureDefinitions.CssClasses.Picker.Input;
        string separator = FeatureDefinitions.CssClasses.Picker.Separator;
        string slider = FeatureDefinitions.CssClasses.Picker.Slider;
        string preview = FeatureDefinitions.CssClasses.Picker.Preview;

        return Task.FromResult($$"""
/* ========================================
   Picker Family Styles
   Auto-generated - Do not edit manually
   ======================================== */

bob-component[{{picker}}] {
    display: flex;
    flex-direction: column;
    gap: calc(0.5rem * var(--bob-density-multiplier));
    padding: calc({{V(FeatureDefinitions.Tokens.Picker.Padding)}} * {{V(sizeMult, "1")}});
    background: var(--palette-surface);
    border: 1px solid var(--palette-border);
    border-radius: {{V(FeatureDefinitions.Tokens.Picker.Radius)}};
    user-select: none;
    --_cell: calc({{V(FeatureDefinitions.Tokens.Picker.CellSize)}} * {{V(sizeMult, "1")}});
    --_btn: calc(32px * {{V(sizeMult, "1")}});
}

/* Row */
bob-component[{{picker}}] .{{row}} {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: calc(0.5rem * var(--bob-density-multiplier));
}

/* Title */
bob-component[{{picker}}] .{{title}} {
    flex: 1;
    font-weight: 600;
    text-align: center;
}

/* Grid */
bob-component[{{picker}}] .{{grid}} {
    display: grid;
    grid-template-columns: repeat(7, var(--_cell));
    gap: calc(0.5rem * var(--bob-density-multiplier));
}

/* Cell */
bob-component[{{picker}}] .{{cell}} {
    display: flex;
    align-items: center;
    justify-content: center;
    width: var(--_cell);
    height: var(--_cell);
    border: none;
    border-radius: 50%;
    background: transparent;
    color: inherit;
    font: inherit;
    cursor: pointer;
    transition: background-color 150ms ease;
}

bob-component[{{picker}}] .{{cell}}:hover:not(.{{cellSelected}}) {
    background: color-mix(in srgb, var(--palette-surface-contrast) 8%, transparent);
}

bob-component[{{picker}}] .{{cellSelected}} {
    background: var(--palette-primary);
    color: var(--palette-primary-contrast);
}

bob-component[{{picker}}] .{{cell}}[data-bob-muted="true"] {
    opacity: 0.3;
}

/* Input */
bob-component[{{picker}}] .{{input}} {
    width: calc(3rem * {{V(sizeMult, "1")}});
    height: var(--_btn);
    padding: 0.25rem;
    border: 1px solid var(--palette-border);
    border-radius: 6px;
    background: transparent;
    color: inherit;
    font: inherit;
    font-weight: 600;
    font-variant-numeric: tabular-nums;
    text-align: center;
}

bob-component[{{picker}}] .{{input}}:focus {
    outline: none;
    border-color: var(--palette-primary);
}

/* Separator */
bob-component[{{picker}}] .{{separator}} {
    font-size: 1.25em;
    font-weight: 600;
    opacity: 0.5;
}

/* Slider */
bob-component[{{picker}}] .{{slider}} {
    position: relative;
    height: calc(14px * {{V(sizeMult, "1")}});
    border-radius: 7px;
    overflow: hidden;
}

bob-component[{{picker}}] .{{slider}} input[type="range"] {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    margin: 0;
    opacity: 0;
    cursor: pointer;
}

bob-component[{{picker}}] .{{slider}}::after {
    content: '';
    position: absolute;
    top: 50%;
    left: var(--value, 0%);
    width: 6px;
    height: calc(18px * {{V(sizeMult, "1")}});
    background: #fff;
    border: 1px solid var(--palette-border);
    border-radius: 3px;
    transform: translate(-50%, -50%);
    pointer-events: none;
    box-shadow: 0 1px 3px rgba(0,0,0,0.2);
}

/* Preview */
bob-component[{{picker}}] .{{preview}} {
    height: calc(28px * {{V(sizeMult, "1")}});
    border: 1px solid var(--palette-border);
    border-radius: 6px;
    background-image: 
        linear-gradient(45deg, var(--palette-border) 25%, transparent 25%),
        linear-gradient(-45deg, var(--palette-border) 25%, transparent 25%),
        linear-gradient(45deg, transparent 75%, var(--palette-border) 75%),
        linear-gradient(-45deg, transparent 75%, var(--palette-border) 75%);
    background-size: 8px 8px;
    background-position: 0 0, 0 4px, 4px -4px, -4px 0;
    background-color: var(--palette-background);
    overflow: hidden;
}

bob-component[{{picker}}] .{{preview}} > div {
    width: 100%;
    height: 100%;
}

/* ========================================
   KEYBOARD FOCUS INDICATORS
   ======================================== */

bob-component[{{picker}}] .{{cell}}:focus-visible {
    outline: var(--bob-highlight-outline);
    outline-offset: var(--bob-highlight-outline-offset);
}

bob-component[{{picker}}] .{{input}}:focus-visible {
    outline: var(--bob-highlight-outline);
    outline-offset: var(--bob-highlight-outline-offset);
}

/* Slider focus - box-shadow instead of outline due to overflow:hidden */
bob-component[{{picker}}] .{{slider}}:focus-within {
    box-shadow: 0 0 0 2px var(--palette-surface), 0 0 0 4px var(--palette-highlight);
}
""");
    }
}