using System.Collections.Frozen;
using System.Globalization;
using BlazOrbit.Localization;
using BlazOrbit.Localization.Providers;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Collection("BobLocalize-StaticState")]
[Trait("Component Integration", "LiteralProvider")]
public class LiteralProviderTests : IDisposable
{
    private readonly BobLocalizeStateSnapshot _snapshot = new();
    public void Dispose() => _snapshot.Dispose();

    private sealed class TestResources
    {
    }

    [Fact]
    public void Should_Always_Return_False_As_Marker_Provider()
    {
        // LiteralProvider is a chain-position sentinel — the actual literal carry-through is
        // handled by BobLocalizer / the generator-emitted accessor. This guarantees the
        // `ResourceNotFound = true` diagnostic when no real provider answered.
        ulong hash = BobLocalizationHash.Compute("Hello, world!");
        _snapshot.RegisterFake(new BobLocalizationBundleSpec(
            typeof(TestResources),
            "en-US",
            [],
            [],
            null,
            new Dictionary<ulong, string> { [hash] = "Hello, world!" }.ToFrozenDictionary()));

        LiteralProvider provider = new();

        provider.TryGet(hash, new CultureInfo("ja-JP"), out string? value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void Should_Return_False_For_Unknown_Hash()
    {
        _snapshot.RegisterFake(new BobLocalizationBundleSpec(
            typeof(TestResources),
            "en-US",
            [],
            [],
            null,
            new Dictionary<ulong, string>().ToFrozenDictionary()));

        LiteralProvider provider = new();

        provider.TryGet(0xDEADBEEFUL, new CultureInfo("en-US"), out string? value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void Should_Return_False_When_No_Bundles_Have_Source_Literals()
    {
        _snapshot.RegisterFake(new BobLocalizationBundleSpec(
            typeof(TestResources),
            "en-US",
            [],
            [],
            null,
            null));

        LiteralProvider provider = new();

        provider.TryGet(0x1UL, new CultureInfo("en-US"), out string? value).Should().BeFalse();
        value.Should().BeNull();
    }
}