using BlazOrbit;
using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Localization.Wasm;
using BlazOrbit.Tests.Integration.Templates.Components;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Tests.Integration.Tests.Extensions;

public class FakeJsRuntime : IJSRuntime
{
    ValueTask<TValue> IJSRuntime.InvokeAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                    DynamicallyAccessedMemberTypes.PublicFields |
                                    DynamicallyAccessedMemberTypes.PublicProperties)]
        TValue>(string identifier, object?[]? args) => throw new NotImplementedException();

    ValueTask<TValue> IJSRuntime.InvokeAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                    DynamicallyAccessedMemberTypes.PublicFields |
                                    DynamicallyAccessedMemberTypes.PublicProperties)]
        TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
        throw new NotImplementedException();
}

[Trait("Library", "Service Registration")]
public class ServiceRegistrationTests
{
    private static readonly Type[] ExpectedServiceTypes =
    [
        typeof(IVariantRegistry), typeof(IThemeJsInterop), typeof(IBehaviorJsInterop), typeof(IMemoryCache)
    ];

    [Fact(DisplayName = "AddBlazOrbit_RegistersAndResolvesAllServices")]
    public async Task AddBlazOrbit_RegistersAndResolvesAllServices()
    {
        ServiceCollection services = [];
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
        ServiceCollection services = [];
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

    /// <summary>
    /// Regression: many built-in BlazOrbit components (BOBChip, BOBBanner, BOBInputDateTime,
    /// BOBInputNumber, ~35 in total) inject <c>IStringLocalizer&lt;TMarker&gt;</c> directly. Before
    /// this guard, a consumer who opted into BlazOrbit *without* localization (the
    /// `IncludeLocalization=false` template path) hit a DI resolution exception on the first
    /// render — the home page rendered blank with `Cannot resolve service for type
    /// 'IStringLocalizer`1[...]'` in the browser console.
    ///
    /// <para>
    /// Fix: <c>AddBlazOrbit()</c> registers the BOBLocalize runtime (which replaces the open
    /// generic <c>IStringLocalizer&lt;&gt;</c> with <see cref="BlazOrbit.Localization.BobLocalizer{T}"/>).
    /// That adapter falls back to a literal LocalizedString when no <c>[BobLocalizationBundle]</c>
    /// is registered for the marker, so consumers who skip localization still get working components.
    /// </para>
    /// </summary>
    [Fact(DisplayName = "AddBlazOrbit_RegistersIStringLocalizer_SoComponentsCanInjectWithoutOptIn")]
    public void AddBlazOrbit_RegistersIStringLocalizer_SoComponentsCanInjectWithoutOptIn()
    {
        // Arrange — only AddBlazOrbit(), no AddBlazOrbitLocalizationServer/Wasm.
        ServiceCollection services = [];
        services.AddScoped<IJSRuntime, FakeJsRuntime>();
        services.AddBlazOrbit();
        using ServiceProvider provider = services.BuildServiceProvider();

        // Act — resolve the same way BOBInputNumber.razor does at line 29.
        IStringLocalizer<BOBFormsResources>? localizer =
            provider.GetService<IStringLocalizer<BOBFormsResources>>();

        // Assert
        localizer.Should().NotBeNull(
            "AddBlazOrbit() must register IStringLocalizer<T> so components inject cleanly " +
            "even when the consumer hasn't called AddBlazOrbitLocalizationServer/Wasm.");
    }

    // LIB-02: Localization registration tests

    [Fact(DisplayName = "AddBlazOrbitLocalizationServer_RegistersLocalizationSettings")]
    public void AddBlazOrbitLocalizationServer_RegistersLocalizationSettings()
    {
        // Arrange
        ServiceCollection services = [];
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
        ServiceCollection services = [];
        services.AddSingleton<IJSRuntime, FakeJsRuntime>();

        // Act
        services.AddBlazOrbitLocalizationWasm(opts => opts.DefaultCulture = "de-DE");
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        WasmLocalizationSettings? settings =
            provider.GetService<WasmLocalizationSettings>();
        settings.Should().NotBeNull();
        settings!.DefaultCulture.Should().Be("de-DE");
    }

    [Fact(DisplayName = "AddBlazOrbitLocalizationWasm_RegistersILocalizationPersistence")]
    public void AddBlazOrbitLocalizationWasm_RegistersILocalizationPersistence()
    {
        // Arrange
        ServiceCollection services = [];
        services.AddSingleton<IJSRuntime, FakeJsRuntime>();

        // Act
        services.AddBlazOrbitLocalizationWasm();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        ILocalizationPersistence? persistence = provider.GetService<ILocalizationPersistence>();
        persistence.Should().NotBeNull();
    }
}