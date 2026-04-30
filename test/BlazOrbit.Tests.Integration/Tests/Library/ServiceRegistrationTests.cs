using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Localization.Wasm;
using BlazOrbit.Tests.Integration.Templates.Components;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Tests.Integration.Tests.Extensions;

public class FakeJsRuntime : IJSRuntime
{
    ValueTask<TValue> IJSRuntime.InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, object?[]? args) => throw new NotImplementedException();

    ValueTask<TValue> IJSRuntime.InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => throw new NotImplementedException();
}

[Trait("Library", "Service Registration")]
public class ServiceRegistrationTests
{
    private static readonly Type[] ExpectedServiceTypes =
    {
        typeof(IVariantRegistry),
        typeof(IThemeJsInterop),
        typeof(IBehaviorJsInterop),
        typeof(IMemoryCache),
    };

    [Fact(DisplayName = "AddBlazOrbit_RegistersAndResolvesAllServices")]
    public async Task AddBlazOrbit_RegistersAndResolvesAllServices()
    {
        ServiceCollection services = new();
        services.AddScoped<IJSRuntime, FakeJsRuntime>();

        services.AddBlazOrbit();
        await using ServiceProvider provider = services.BuildServiceProvider();

        AssertServicesAreRegistered(services);
        AssertServicesCanBeResolved(provider);
    }

    [Fact(DisplayName = "AddBlazOrbitVariants_RegistersCustomVariants")]
    public void ServiceCollectionExtensions_AddBlazOrbitVariants_RegistersCustomVariants()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddBlazOrbit();
        TestVariant customVariant = TestVariant.Custom("Test");
        bool templateCalled = false;

        // Act
        services.AddBlazOrbitVariants(builder => builder.ForComponent<TestVariantComponent>()
                .AddVariant(customVariant, _ =>
                {
                    templateCalled = true;
                    return __builder => { };
                }));

        ServiceProvider provider = services.BuildServiceProvider();
        IVariantRegistry registry = provider.GetRequiredService<IVariantRegistry>();
        RenderFragment? template = registry.GetTemplate(typeof(TestVariantComponent), customVariant, null!);
        template?.Invoke(null!);

        // Assert
        templateCalled.Should().BeTrue();
    }

    private static void AssertServicesAreRegistered(
            IServiceCollection services)
    {
        foreach (Type serviceType in ExpectedServiceTypes)
        {
            services.Should().Contain(d => d.ServiceType == serviceType,
                $"Service {serviceType.Name} should be registered");
        }
    }

    private static void AssertServicesCanBeResolved(
    IServiceProvider provider)
    {
        foreach (Type serviceType in ExpectedServiceTypes)
        {
            provider.GetService(serviceType)
                .Should().NotBeNull($"{serviceType.Name} should be resolvable");
        }
    }

    // LIB-02: Localization registration tests

    [Fact(DisplayName = "AddBlazOrbitLocalizationServer_RegistersLocalizationSettings")]
    public void AddBlazOrbitLocalizationServer_RegistersLocalizationSettings()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<IJSRuntime, FakeJsRuntime>();

        // Act
        services.AddBlazOrbitLocalizationServer(opts =>
        {
            opts.DefaultCulture = "es-ES";
            opts.CultureCookieName = ".Test.Culture";
        });
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        BlazOrbit.Localization.Server.ServerLocalizationSettings? settings =
            provider.GetService<BlazOrbit.Localization.Server.ServerLocalizationSettings>();
        settings.Should().NotBeNull();
        settings!.DefaultCulture.Should().Be("es-ES");
        settings.CultureCookieName.Should().Be(".Test.Culture");
    }

    [Fact(DisplayName = "AddBlazOrbitLocalizationWasm_RegistersLocalizationSettings")]
    public void AddBlazOrbitLocalizationWasm_RegistersLocalizationSettings()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<IJSRuntime, FakeJsRuntime>();

        // Act
        services.AddBlazOrbitLocalizationWasm(opts => opts.DefaultCulture = "de-DE");
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        BlazOrbit.Localization.Wasm.WasmLocalizationSettings? settings =
            provider.GetService<BlazOrbit.Localization.Wasm.WasmLocalizationSettings>();
        settings.Should().NotBeNull();
        settings!.DefaultCulture.Should().Be("de-DE");
    }

    [Fact(DisplayName = "AddBlazOrbitLocalizationWasm_RegistersILocalizationPersistence")]
    public void AddBlazOrbitLocalizationWasm_RegistersILocalizationPersistence()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<IJSRuntime, FakeJsRuntime>();

        // Act
        services.AddBlazOrbitLocalizationWasm();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        ILocalizationPersistence? persistence = provider.GetService<ILocalizationPersistence>();
        persistence.Should().NotBeNull();
    }
}
