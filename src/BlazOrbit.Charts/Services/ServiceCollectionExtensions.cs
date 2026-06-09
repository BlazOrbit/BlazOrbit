using BlazOrbit.Charts.Services.JsInterop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlazOrbit.Charts.Services;

/// <summary>
/// DI helpers for the BlazOrbit.Charts package. Hosts must call
/// <see cref="AddBlazOrbitCharts(IServiceCollection)"/> alongside the main
/// <c>AddBlazOrbit()</c> registration to wire up the JS interop required by
/// chart components.
/// <para>
/// <b>Stylesheet</b>: the chart-family CSS bundle
/// (<c>_content/BlazOrbit.Charts/css/blazorbit-charts.css</c>) is auto-
/// injected into <c>&lt;head&gt;</c> by the JS module on first import - no
/// manual <c>&lt;link&gt;</c> in <c>index.html</c> / <c>App.razor</c> is
/// required. Add one explicitly only if you need precise ordering against
/// theme overrides; the auto-inject detects an existing link and skips.
/// </para>
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="IChartJsInterop"/> implementation used by
    /// every BOBCharts component. Idempotent - repeated calls add nothing.
    /// </summary>
    /// <param name="services">DI container to mutate.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddBlazOrbitCharts(this IServiceCollection services)
    {
        services.TryAddScoped<IChartJsInterop, ChartJsInterop>();
        return services;
    }
}