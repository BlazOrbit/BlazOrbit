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
        BobLocalizationBundleSpec spec = BuildSpec(
            new Dictionary<string, Dictionary<ulong, string>>
            {
                ["en-US"] = new() { [hash] = "Hello" }, ["es-ES"] = new() { [hash] = "Hola" }
            });
        BundleProvider provider = new();

        // Act
        provider.TryGet(spec, hash, new CultureInfo("es-ES"), out string? value).Should().BeTrue();

        // Assert
        value.Should().Be("Hola");
    }

    [Fact]
    public void Should_Walk_Parent_Culture_Chain()
    {
        // Arrange — es-MX should fall back to es (no es-MX entry).
        ulong hash = BobLocalizationHash.Compute("Welcome");
        BobLocalizationBundleSpec spec = BuildSpec(
            new Dictionary<string, Dictionary<ulong, string>> { ["es"] = new() { [hash] = "Bienvenido" } });
        BundleProvider provider = new();

        // Act
        provider.TryGet(spec, hash, new CultureInfo("es-MX"), out string? value).Should().BeTrue();

        // Assert
        value.Should().Be("Bienvenido");
    }

    [Fact]
    public void Should_Terminate_At_Bundle_Default_Culture()
    {
        // Arrange — ko-KR resolves nothing along its chain; bundle default en-US wins.
        ulong hash = BobLocalizationHash.Compute("Goodbye");
        BobLocalizationBundleSpec spec = BuildSpec(
            defaultCulture: "en-US",
            translations: new Dictionary<string, Dictionary<ulong, string>>
            {
                ["en-US"] = new() { [hash] = "Goodbye" }
            });
        BundleProvider provider = new();

        // Act
        provider.TryGet(spec, hash, new CultureInfo("ko-KR"), out string? value).Should().BeTrue();

        // Assert
        value.Should().Be("Goodbye");
    }

    [Fact]
    public void Should_Return_False_When_Hash_Is_Unknown()
    {
        BobLocalizationBundleSpec spec = BuildSpec(new Dictionary<string, Dictionary<ulong, string>>());
        BundleProvider provider = new();

        provider.TryGet(spec, 0xDEADBEEFUL, new CultureInfo("en-US"), out string? value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void Should_Return_False_When_Bundle_Has_No_Translations()
    {
        // Regression: a bundle declared without `.tn` data (Translations == null) must yield
        // a clean miss instead of leaking a translation from another registered bundle that
        // happens to share the same hash.
        ulong hash = BobLocalizationHash.Compute("Hello");
        _snapshot.RegisterFake(BuildSpec(
            new Dictionary<string, Dictionary<ulong, string>>
            {
                ["en-US"] = new() { [hash] = "Hello-from-other-bundle" }
            },
            resourceType: typeof(OtherResources)));

        BobLocalizationBundleSpec emptySpec = BuildSpec(translations: null);
        BundleProvider provider = new();

        provider.TryGet(emptySpec, hash, new CultureInfo("en-US"), out string? value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void Should_Throw_On_Null_Spec()
    {
        BundleProvider provider = new();
        Action act = () => provider.TryGet(null!, 0x1UL, new CultureInfo("en-US"), out _);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Should_Throw_On_Null_Culture()
    {
        BobLocalizationBundleSpec spec = BuildSpec(new Dictionary<string, Dictionary<ulong, string>>());
        BundleProvider provider = new();
        Action act = () => provider.TryGet(spec, 0x1UL, null!, out _);
        act.Should().Throw<ArgumentNullException>();
    }

    private sealed class OtherResources
    {
    }

    private static BobLocalizationBundleSpec BuildSpec(
        Dictionary<string, Dictionary<ulong, string>>? translations = null,
        string defaultCulture = "en-US",
        Type? resourceType = null)
    {
        FrozenDictionary<string, FrozenDictionary<ulong, string>>? frozen = translations is null
            ? null
            : translations.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.ToFrozenDictionary()).ToFrozenDictionary();

        return new BobLocalizationBundleSpec(
            resourceType ?? typeof(TestResources),
            defaultCulture,
            [],
            [],
            frozen,
            null);
    }
}