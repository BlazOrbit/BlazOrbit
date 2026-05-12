using System.Collections.Frozen;
using System.Globalization;
using BlazOrbit.Localization;
using BlazOrbit.Localization.Providers;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Collection("BobLocalize-StaticState")]
[Trait("Component Integration", "BundleProvider")]
public class BundleProviderTests : IDisposable
{
    private readonly BobLocalizeStateSnapshot _snapshot = new();
    public void Dispose() => _snapshot.Dispose();

    private sealed class TestResources
    {
    }

    [Fact]
    public void Should_Return_Translation_For_Current_Culture()
    {
        // Arrange
        ulong hash = BobLocalizationHash.Compute("Hello");
        _snapshot.RegisterFake(BuildSpec(
            new Dictionary<string, Dictionary<ulong, string>>
            {
                ["en-US"] = new() { [hash] = "Hello" }, ["es-ES"] = new() { [hash] = "Hola" }
            }));
        BundleProvider provider = new();

        // Act
        provider.TryGet(hash, new CultureInfo("es-ES"), out string? value).Should().BeTrue();

        // Assert
        value.Should().Be("Hola");
    }

    [Fact]
    public void Should_Walk_Parent_Culture_Chain()
    {
        // Arrange — es-MX should fall back to es (no es-MX entry).
        ulong hash = BobLocalizationHash.Compute("Welcome");
        _snapshot.RegisterFake(BuildSpec(
            new Dictionary<string, Dictionary<ulong, string>> { ["es"] = new() { [hash] = "Bienvenido" } }));
        BundleProvider provider = new();

        // Act
        provider.TryGet(hash, new CultureInfo("es-MX"), out string? value).Should().BeTrue();

        // Assert
        value.Should().Be("Bienvenido");
    }

    [Fact]
    public void Should_Terminate_At_Bundle_Default_Culture()
    {
        // Arrange — ko-KR resolves nothing along its chain; bundle default en-US wins.
        ulong hash = BobLocalizationHash.Compute("Goodbye");
        _snapshot.RegisterFake(BuildSpec(
            defaultCulture: "en-US",
            translations: new Dictionary<string, Dictionary<ulong, string>>
            {
                ["en-US"] = new() { [hash] = "Goodbye" }
            }));
        BundleProvider provider = new();

        // Act
        provider.TryGet(hash, new CultureInfo("ko-KR"), out string? value).Should().BeTrue();

        // Assert
        value.Should().Be("Goodbye");
    }

    [Fact]
    public void Should_Return_False_When_Hash_Is_Unknown()
    {
        _snapshot.RegisterFake(BuildSpec(new Dictionary<string, Dictionary<ulong, string>>()));
        BundleProvider provider = new();

        provider.TryGet(0xDEADBEEFUL, new CultureInfo("en-US"), out string? value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void Should_Return_False_When_No_Bundles_Registered()
    {
        BundleProvider provider = new();
        provider.TryGet(0x123UL, new CultureInfo("en-US"), out string? value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void Should_Throw_On_Null_Culture()
    {
        BundleProvider provider = new();
        Action act = () => provider.TryGet(0x1UL, null!, out _);
        act.Should().Throw<ArgumentNullException>();
    }

    private static BobLocalizationBundleSpec BuildSpec(
        Dictionary<string, Dictionary<ulong, string>>? translations = null,
        string defaultCulture = "en-US")
    {
        FrozenDictionary<string, FrozenDictionary<ulong, string>>? frozen = translations is null
            ? null
            : translations.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.ToFrozenDictionary()).ToFrozenDictionary();

        return new BobLocalizationBundleSpec(
            typeof(TestResources),
            defaultCulture,
            [],
            [],
            frozen,
            null);
    }
}