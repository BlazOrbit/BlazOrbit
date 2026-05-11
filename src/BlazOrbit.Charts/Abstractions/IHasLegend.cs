using BlazOrbit.Charts.Enums;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Charts.Abstractions;

/// <summary>
/// Implemented by chart types that emit an interactive legend. The legend
/// renders one entry per series (or slice for pie / donut) and supports
/// click-to-toggle visibility.
/// </summary>
public interface IHasLegend
{
    /// <summary>Whether the legend is visible. Defaults to <c>true</c>.</summary>
    bool ShowLegend { get; }

    /// <summary>Anchor of the legend block relative to the plot area.</summary>
    BOBChartLegendPosition LegendPosition { get; }

    /// <summary>
    /// Fired when the user toggles a legend entry. The argument is the
    /// <c>Label</c> of the affected series / slice.
    /// </summary>
    EventCallback<string> OnLegendToggle { get; }
}
