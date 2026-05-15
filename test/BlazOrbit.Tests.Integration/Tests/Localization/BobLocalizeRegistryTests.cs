using System.Collections.Frozen;
using System.Globalization;
using BlazOrbit.Localization;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

/// <summary>
/// The <see cref="BobLocalize"/> static registry is process-wide, so each test clears it
/// in setup. xUnit instantiates a fresh class per test method, so the constructor runs once
/// per test - perfect for the cleanup pattern.
/// </summary>
[Collection("BobLocalize-StaticState")]
[Trait("Component Integration", "BobLocalize")]
public class BobLocalizeRegistryTests : IDisposable
{
    private readonly BobLocalizeStateSnapshot _snapshot = new();
    public void Dispose() => _snapshot.Dispose();

    private sealed class FakeResources_A
    {
    }

    private sealed class FakeResources_B
    {
    }

    [Fact]
    public void Should_Register_And_Retrieve_Bundle()
    {
        BobLocalizationBundleSpec spec = NewSpec(typeof(FakeResources_A));
        _snapshot.RegisterFake(spec);

        BobLocalize.TryGetBundle(typeof(FakeResources_A), out BobLocalizationBundleSpec? retrieved).Should().BeTrue();
        retrieved.Should().BeSameAs(spec);
    }

    [Fact]
    public void Should_Return_False_For_Unregistered_Type()
    {
        BobLocalize.TryGetBundle(typeof(FakeResources_A), out BobLocalizationBundleSpec? retrieved).Should().BeFalse();
        retrieved.Should().BeNull();
    }

    [Fact]
    public void Should_Replace_Bundle_On_Re_Registration()
    {
        BobLocalizationBundleSpec first = NewSpec(typeof(FakeResources_A), "en-US");
        BobLocalizationBundleSpec second = NewSpec(typeof(FakeResources_A), "es-ES");

        _snapshot.RegisterFake(first);
        _snapshot.RegisterFake(second);

        BobLocalize.TryGetBundle(typeof(FakeResources_A), out BobLocalizationBundleSpec? current).Should().BeTrue();
        current!.DefaultCulture.Should().Be("es-ES");
    }

    [Fact]
    public void Should_List_All_Registered_Bundles()
    {
        // The registry is process-global and the [ModuleInitializer]s register the
        // production bundles before any test runs. We assert containment of our fakes
        // rather than a strict count so this test composes with the global state.
        _snapshot.RegisterFake(NewSpec(typeof(FakeResources_A)));
        _snapshot.RegisterFake(NewSpec(typeof(FakeResources_B)));

        BobLocalize.AllBundles.Select(b => b.ResourceType)
            .Should().Contain([typeof(FakeResources_A), typeof(FakeResources_B)]);
    }

    [Fact]
    public void Should_Unregister_Bundle()
    {
        _snapshot.RegisterFake(NewSpec(typeof(FakeResources_A)));

        BobLocalize.UnregisterBundle(typeof(FakeResources_A)).Should().BeTrue();
        BobLocalize.TryGetBundle(typeof(FakeResources_A), out _).Should().BeFalse();
    }

    [Fact]
    public void Should_Fire_CultureChanged_Event()
    {
        CultureInfo? captured = null;
        Action<CultureInfo> handler = c => captured = c;
        BobLocalize.CultureChanged += handler;
        try
        {
            BobLocalize.NotifyCultureChanged(new CultureInfo("ja-JP"));
            captured.Should().NotBeNull();
            captured!.Name.Should().Be("ja-JP");
        }
        finally
        {
            BobLocalize.CultureChanged -= handler;
        }
    }

    private static BobLocalizationBundleSpec NewSpec(Type resourceType, string defaultCulture = "en-US")
        => new(
            resourceType,
            defaultCulture,
            [],
            [],
            null,
            null);
}
