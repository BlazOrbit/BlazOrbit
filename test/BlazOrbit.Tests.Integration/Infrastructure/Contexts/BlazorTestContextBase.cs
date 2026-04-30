using BlazOrbit.Tests.Integration.Infrastructure.Fakes;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Infrastructure.Contexts;

public abstract class BlazorTestContextBase : BunitContext
{
    protected BlazorTestContextBase()
    {
        ConfigureCommonServices(Services);
        ConfigureScenarioServices(Services);
    }

    public abstract string Scenario { get; }

    protected virtual void ConfigureCommonServices(IServiceCollection services)
    {
        // Register ===
        services.AddBlazOrbit();

        // JSInterop fake (bUnit controla IJSRuntime)
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Navigation manager común
        services.AddSingleton<FakeNavigationManager>();
        services.AddSingleton<NavigationManager>(sp =>
        sp.GetRequiredService<FakeNavigationManager>());
    }

    protected abstract void ConfigureScenarioServices(IServiceCollection services);
}