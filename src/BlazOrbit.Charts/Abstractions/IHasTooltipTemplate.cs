using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Charts.Abstractions;

/// <summary>
/// Implemented by chart types that surface tooltips on hover. Consumers can
/// either provide a format string for the default renderer or supply a fully
/// custom <see cref="RenderFragment{T}"/> with access to the active context.
/// </summary>
/// <typeparam name="TX">Type of the X-axis values for the active hover point.</typeparam>
/// <typeparam name="TY">Type of the Y-axis values for the active hover point.</typeparam>
public interface IHasTooltipTemplate<TX, TY>
{
    /// <summary>
    /// Optional <c>string.Format</c> pattern applied to the default tooltip,
    /// e.g. <c>"{0}: {1:N2} €"</c> where <c>{0}</c> is the label and
    /// <c>{1}</c> the value. Ignored when <see cref="TooltipTemplate"/> is set.
    /// </summary>
    string? TooltipFormat { get; }

    /// <summary>
    /// Custom render fragment that fully replaces the default tooltip layout.
    /// Receives the active <see cref="BOBChartTooltipContext{TX, TY}"/>.
    /// </summary>
    RenderFragment<BOBChartTooltipContext<TX, TY>>? TooltipTemplate { get; }
}
