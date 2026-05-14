using System.Globalization;
using BlazOrbit.Charts.Services;
using BlazOrbit.Tests.Integration.Infrastructure.Fakes;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Infrastructure.Contexts;

public abstract class BlazorTestContextBase : BunitContext
{
    protected BlazorTestContextBase()
    {
        // Reset culture per-context so leaks from culture-mutating tests running in
        // parallel on the same thread-pool thread do not bleed into component tests
        // that expect the default English bundle. Tests that exercise culture switching
        // still set + restore around their own try/finally; the reset below covers the
        // gap when xUnit's thread-pool reuses a thread mid-flight.
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        ConfigureCommonServices(Services);
        ConfigureScenarioServices(Services);
    }

    public abstract string Scenario { get; }

    protected virtual void ConfigureCommonServices(IServiceCollection services)
    {
        // Register ===
        services.AddBlazOrbit();
        services.AddBlazOrbitCharts();

        // JSInterop fake (bUnit controla IJSRuntime)
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Navigation manager común
        services.AddSingleton<FakeNavigationManager>();
        services.AddSingleton<NavigationManager>(sp =>
            sp.GetRequiredService<FakeNavigationManager>());
    }

    protected abstract void ConfigureScenarioServices(IServiceCollection services);
}