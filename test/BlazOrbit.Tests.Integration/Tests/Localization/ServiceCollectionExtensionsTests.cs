using BlazOrbit.Localization;
using BlazOrbit.Localization.Providers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Collection("BobLocalize-StaticState")]
[Trait("Component Integration", "BobLocalize.ServiceCollection")]
public class ServiceCollectionExtensionsTests
{
    private sealed class TestResources
    {
    }

    [Fact]
    public void AddBlazOrbitLocalization_Should_Register_Built_In_Providers()
    {
        IServiceProvider sp = new ServiceCollection()
            .AddBlazOrbitLocalization()
            .Services
            .BuildServiceProvider();

        sp.GetService<LiteralProvider>().Should().NotBeNull();
        sp.GetService<BundleProvider>().Should().NotBeNull();
    }

    [Fact]
    public void AddBlazOrbitLocalization_Should_Register_Open_Generic_IStringLocalizer()
    {
        IServiceProvider sp = new ServiceCollection()
            .AddBlazOrbitLocalization()
            .Services
            .BuildServiceProvider();

        IStringLocalizer<TestResources> loc = sp.GetRequiredService<IStringLocalizer<TestResources>>();
        loc.Should().BeOfType<BobLocalizer<TestResources>>();
    }

    [Fact]
    public void AddBlazOrbitLocalization_Should_Register_Options()
    {
        IServiceProvider sp = new ServiceCollection()
            .AddBlazOrbitLocalization(opts => opts.DefaultCulture = "es-ES")
            .Services
            .BuildServiceProvider();

        BobLocalizationOptions opts = sp.GetRequiredService<BobLocalizationOptions>();
        opts.DefaultCulture.Should().Be("es-ES");
    }

    [Fact]
    public void AddProvider_Should_Register_Custom_Provider()
    {
        IServiceProvider sp = new ServiceCollection()
            .AddBlazOrbitLocalization()
            .AddProvider<DummyProvider>()
            .Services
            .BuildServiceProvider();

        sp.GetService<DummyProvider>().Should().NotBeNull();
    }

    [Fact]
    public void AddBlazOrbitLocalization_Should_Be_Idempotent()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddBlazOrbitLocalization();
        services.AddBlazOrbitLocalization(); // second call must not duplicate

        IServiceProvider sp = services.BuildServiceProvider();
        sp.GetServices<LiteralProvider>().Should().HaveCount(1);
    }

    private sealed class DummyProvider : IBobLocalizationProvider
    {
        public bool TryGet(BobLocalizationBundleSpec spec, ulong hash, System.Globalization.CultureInfo culture, out string? value)
        {
            value = null;
            return false;
        }
    }
}