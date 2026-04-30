using BlazOrbit.Components;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.BuildTools.Generators.Families;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class DataCollectionFamilyCssGenerator : IAssetGenerator
{
    public string FileName => "_data-collection-family.css";
    public string Name => "Data Collection Family";

    public Task<string> GetContent()
    {
        string root = FeatureDefinitions.Tags.Component;
        string dc = FeatureDefinitions.DataAttributes.DataCollectionBase;
        string density = FeatureDefinitions.DataAttributes.Density;

        string toolbar = FeatureDefinitions.CssClasses.DataCollection.Toolbar;
        string toolbarSpacer = FeatureDefinitions.CssClasses.DataCollection.ToolbarSpacer;
        string filter = FeatureDefinitions.CssClasses.DataCollection.Filter;
        string selectionInfo = FeatureDefinitions.CssClasses.DataCollection.SelectionInfo;
        string pageSize = FeatureDefinitions.CssClasses.DataCollection.PageSize;
        string pagination = FeatureDefinitions.CssClasses.DataCollection.Pagination;
        string paginationInfo = FeatureDefinitions.CssClasses.DataCollection.PaginationInfo;
        string paginationControls = FeatureDefinitions.CssClasses.DataCollection.PaginationControls;
        string checkbox = FeatureDefinitions.CssClasses.DataCollection.Checkbox;
        string empty = FeatureDefinitions.CssClasses.DataCollection.Empty;
        string emptyIcon = FeatureDefinitions.CssClasses.DataCollection.EmptyIcon;
        string emptyText = FeatureDefinitions.CssClasses.DataCollection.EmptyText;
        string loading = FeatureDefinitions.CssClasses.DataCollection.Loading;

        return Task.FromResult($$"""
/* ========================================
   Data Collection Family Styles
   Auto-generated - Do not edit manually
   ======================================== */

{{root}}[{{dc}}] {
    --_dc-padding-x: 1rem;
    --_dc-padding-y: 0.75rem;
    --_dc-header-bg: color-mix(in oklab, var(--palette-surface) 95%, var(--palette-primary));
    --_dc-hover-bg: color-mix(in oklab, var(--_dc-background) 90%, var(--palette-hover-tint) 10%);
    --_dc-selected-bg: color-mix(in oklab, var(--_dc-background) 80%, var(--palette-primary) 20%);
    --_dc-background: var(--bob-inline-background, var(--palette-surface));
    --_dc-border-radius: var(--bob-inline-border-radius, var(--bob-border-radius));
    --_dc-border: var(--bob-inline-border, var(--bob-border-width) var(--bob-border-style) var(--palette-border));
    --_dc-border-top: var(--bob-inline-border-top, var(--bob-inline-border, var(--bob-border-width) var(--bob-border-style) var(--palette-border)));
    --_dc-border-right: var(--bob-inline-border-right, var(--bob-inline-border, var(--bob-border-width) var(--bob-border-style) var(--palette-border)));
    --_dc-border-bottom: var(--bob-inline-border-bottom, var(--bob-inline-border, var(--bob-border-width) var(--bob-border-style) var(--palette-border)));
    --_dc-border-left: var(--bob-inline-border-left, var(--bob-inline-border, var(--bob-border-width) var(--bob-border-style) var(--palette-border)));

    display: block;
    width: 100%;
    background-color: var(--_dc-background);
    border: var(--_dc-border);
    border-top: var(--_dc-border-top);
    border-right: var(--_dc-border-right);
    border-bottom: var(--_dc-border-bottom);
    border-left: var(--_dc-border-left);
    border-radius: var(--_dc-border-radius);
}

/* === TOOLBAR === */
{{root}}[{{dc}}] .{{toolbar}} {
    display: flex;
    align-items: center;
    gap: calc(1rem * var(--bob-density-multiplier, 1));
    padding: var(--_dc-padding-y) var(--_dc-padding-x);
    background: var(--_dc-header-bg);
    border-bottom: 1px solid var(--palette-border);
}

{{root}}[{{dc}}] .{{toolbarSpacer}} {
    flex: 1;
}

/* === FILTER === */
{{root}}[{{dc}}] .{{filter}} {
    position: relative;
    display: flex;
    align-items: center;
    gap: calc(0.5rem * var(--bob-density-multiplier, 1));
    flex: 0 1 300px;
}

/* === SELECTION INFO === */
{{root}}[{{dc}}] .{{selectionInfo}} {
    display: flex;
    align-items: center;
    gap: calc(0.5rem * var(--bob-density-multiplier, 1));
    font-size: 0.875rem;
    color: var(--palette-primary);
}

/* === PAGE SIZE SELECTOR === */
{{root}}[{{dc}}] .{{pageSize}} {
    display: flex;
    align-items: center;
    gap: calc(0.5rem * var(--bob-density-multiplier, 1));
    font-size: 0.875rem;
}

/* === PAGINATION === */
{{root}}[{{dc}}] .{{pagination}} {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: var(--_dc-padding-y) var(--_dc-padding-x);
    border-top: 1px solid var(--palette-border);
    background: var(--_dc-header-bg);
}

{{root}}[{{dc}}] .{{paginationInfo}} {
    font-size: 0.875rem;
    color: var(--palette-surface-contrast);
    opacity: 0.8;
}

{{root}}[{{dc}}] .{{paginationControls}} {
    display: flex;
    align-items: center;
    gap: calc(0.25rem * var(--bob-density-multiplier, 1));
}

/* === CHECKBOX (shared) === */
{{root}}[{{dc}}] .{{checkbox}} {
    cursor: pointer;
    width: 1.125rem;
    height: 1.125rem;
    accent-color: var(--palette-primary);
}

/* === EMPTY STATE === */
{{root}}[{{dc}}] .{{empty}} {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: calc(0.5rem * var(--bob-density-multiplier, 1));
    padding: calc(var(--_dc-padding-y) * 4) var(--_dc-padding-x);
    color: var(--palette-surface-contrast);
    opacity: 0.6;
    text-align: center;
}

{{root}}[{{dc}}] .{{emptyIcon}} {
    font-size: 3rem;
    opacity: 0.5;
}

{{root}}[{{dc}}] .{{emptyText}} {
    margin: 0;
    font-size: 1rem;
}

/* === LOADING STATE === */
{{root}}[{{dc}}] .{{loading}} {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: calc(var(--_dc-padding-y) * 4) var(--_dc-padding-x);
}

/* === DENSITY: Padding overrides === */
{{root}}[{{dc}}][{{density}}="compact"] {
    --_dc-padding-x: 0.5rem;
    --_dc-padding-y: 0.375rem;
}

{{root}}[{{dc}}][{{density}}="comfortable"] {
    --_dc-padding-x: 1.5rem;
    --_dc-padding-y: 1rem;
}
""");
    }
}
