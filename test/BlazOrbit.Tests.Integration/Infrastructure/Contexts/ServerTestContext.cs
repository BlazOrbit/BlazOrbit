using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Infrastructure.Contexts;

public sealed class ServerTestContext : BlazorTestContextBase
{
    public override string Scenario => "Server";

    protected override void ConfigureScenarioServices(IServiceCollection services) => services.AddBlazOrbitLocalizationServer();
}