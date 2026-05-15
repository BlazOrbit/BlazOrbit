using System.Collections.Frozen;
using System.Globalization;
using BlazOrbit.Localization;
using BlazOrbit.Localization.Providers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Collection("BobLocalize-StaticState")]
[Trait("Component Integration", "BobLocalizer")]
public class BobLocalizerTests : IDisposable
{
    private readonly BobLocalizeStateSnapshot _snapshot = new();
    public void Dispose() => _snapshot.Dispose();

    private sealed class TestResources
    {
    }

    private sealed class OtherResources
    {
    }

    private void Register(BobLocalizationBundleSpec spec) => _snapshot.RegisterFake(spec);

    [Fact]
    public void Should_Return_Translation_When_Bundle_Has_It()
    {
        // Arrange
        ulong hash = BobLocalizationHash.Compute("Hello");
        RegisterBundle(new Dictionary<string, Dictionary<ulong, string>> { ["es-ES"] = new() { [hash] = "Hola" } });

        CultureInfo prev = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
        try
        {
            IServiceProvider sp = BuildServices();
            BobLocalizer<TestResources> loc = new(sp);

            // Act
            LocalizedString result = loc["Hello"];

            // Assert
            result.Value.Should().Be("Hola");
            result.ResourceNotFound.Should().BeFalse();
        }
        finally
        {
            CultureInfo.CurrentUICulture = prev;
        }
    }

    [Fact]
    public void Should_Fall_Back_To_Literal_When_No_Translation()
    {
        // Arrange - bundle exists but the requested culture has no entry.
        RegisterBundle(new Dictionary<string, Dictionary<ulong, string>> { ["es-ES"] = new() });

        CultureInfo prev = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");
        try
        {
            IServiceProvider sp = BuildServices();
            BobLocalizer<TestResources> loc = new(sp);

            // Act
            LocalizedString result = loc["Hello"];

            // Assert
            result.Value.Should().Be("Hello");
            result.ResourceNotFound.Should().BeTrue();
        }
        finally
        {
            CultureInfo.CurrentUICulture = prev;
        }
    }

    [Fact]
    public void Should_Apply_String_Format_With_Arguments()
    {
        // Arrange
        ulong hash = BobLocalizationHash.Compute("{0} files in {1}");
        RegisterBundle(new Dictionary<string, Dictionary<ulong, string>>
        {
            ["en-US"] = new() { [hash] = "{0} archivos en {1}" }
        });

        CultureInfo prev = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        try
        {
            IServiceProvider sp = BuildServices();
            BobLocalizer<TestResources> loc = new(sp);

            // Act
            LocalizedString result = loc["{0} files in {1}", 5, "docs"];

            // Assert
            result.Value.Should().Be("5 archivos en docs");
            result.ResourceNotFound.Should().BeFalse();
        }
        finally
        {
            CultureInfo.CurrentUICulture = prev;
        }
    }

    [Fact]
    public void Should_Format_Literal_When_No_Translation_With_Args()
    {
        // Arrange - bundle registered but missing the hash.
        RegisterBundle(new Dictionary<string, Dictionary<ulong, string>> { ["en-US"] = new() });

        CultureInfo prev = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        try
        {
            IServiceProvider sp = BuildServices();
            BobLocalizer<TestResources> loc = new(sp);

            // Act
            LocalizedString result = loc["{0} pending", 3];

            // Assert
            result.Value.Should().Be("3 pending");
            result.ResourceNotFound.Should().BeTrue();
        }
        finally
        {
            CultureInfo.CurrentUICulture = prev;
        }
    }

    [Fact]
    public void Should_Use_Custom_Provider_From_Chain()
    {
        // Arrange - a fake provider that always wins.
        ulong hash = BobLocalizationHash.Compute("From custom");
        _snapshot.RegisterFake(new BobLocalizationBundleSpec(
            typeof(TestResources),
            "en-US",
            [typeof(FixedProvider), typeof(LiteralProvider)],
            [],
            null,
            null));

        IServiceProvider sp = new ServiceCollection()
            .AddSingleton(new FixedProvider(hash, "INJECTED"))
            .AddSingleton<LiteralProvider>()
            .BuildServiceProvider();

        BobLocalizer<TestResources> loc = new(sp);

        // Act
        LocalizedString result = loc["From custom"];

        // Assert
        result.Value.Should().Be("INJECTED");
        result.ResourceNotFound.Should().BeFalse();
    }

    [Fact]
    public void Should_Route_Prefix_Keys_To_Routed_Provider_Before_Chain()
    {
        // Arrange
        ulong cmsHash = BobLocalizationHash.Compute("cms:welcome");
        _snapshot.RegisterFake(new BobLocalizationBundleSpec(
            typeof(TestResources),
            "en-US",
            [typeof(BundleProvider), typeof(LiteralProvider)],
            [new BobLocalizationKeyRoute("cms:", typeof(FixedProvider))],
            null,
            null));

        IServiceProvider sp = new ServiceCollection()
            .AddSingleton(new FixedProvider(cmsHash, "FROM_CMS"))
            .AddSingleton<BundleProvider>()
            .AddSingleton<LiteralProvider>()
            .BuildServiceProvider();

        BobLocalizer<TestResources> loc = new(sp);

        // Act
        LocalizedString result = loc["cms:welcome"];

        // Assert
        result.Value.Should().Be("FROM_CMS");
    }

    [Fact]
    public void Should_Use_Chain_When_Key_Does_Not_Match_Route()
    {
        // Arrange
        _snapshot.RegisterFake(new BobLocalizationBundleSpec(
            typeof(TestResources),
            "en-US",
            [typeof(LiteralProvider)],
            [new BobLocalizationKeyRoute("cms:", typeof(FixedProvider))],
            null,
            null));

        IServiceProvider sp = new ServiceCollection()
            .AddSingleton(new FixedProvider(0xDEADBEEFUL, "FROM_CMS"))
            .AddSingleton<LiteralProvider>()
            .BuildServiceProvider();

        BobLocalizer<TestResources> loc = new(sp);

        // Act - key does NOT start with "cms:"
        LocalizedString result = loc["other:key"];

        // Assert - falls through to the chain, no provider has it, literal fallback wins.
        result.Value.Should().Be("other:key");
        result.ResourceNotFound.Should().BeTrue();
    }

    [Fact]
    public void Should_Not_Leak_Translation_Across_Bundles_With_Same_Hash()
    {
        // Two bundles share a source literal - same hash, divergent translations.
        // Each BobLocalizer<T> must resolve only against T's own bundle. Regression
        // for non-deterministic cross-bundle leakage produced by iterating the global
        // bundle registry inside BundleProvider.
        ulong hash = BobLocalizationHash.Compute("Basic usage");
        const string culture = "es-ES";

        _snapshot.RegisterFake(new BobLocalizationBundleSpec(
            typeof(TestResources),
            "en-US",
            [typeof(BundleProvider), typeof(LiteralProvider)],
            [],
            new Dictionary<string, FrozenDictionary<ulong, string>>
            {
                [culture] = new Dictionary<ulong, string> { [hash] = "Uso básico" }.ToFrozenDictionary()
            }.ToFrozenDictionary(),
            null));

        _snapshot.RegisterFake(new BobLocalizationBundleSpec(
            typeof(OtherResources),
            "en-US",
            [typeof(BundleProvider), typeof(LiteralProvider)],
            [],
            new Dictionary<string, FrozenDictionary<ulong, string>>
            {
                [culture] = new Dictionary<ulong, string> { [hash] = "Uso DIFERENTE" }.ToFrozenDictionary()
            }.ToFrozenDictionary(),
            null));

        CultureInfo prev = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(culture);
        try
        {
            IServiceProvider sp = BuildServices();
            BobLocalizer<TestResources> locA = new(sp);
            BobLocalizer<OtherResources> locB = new(sp);

            locA["Basic usage"].Value.Should().Be("Uso básico");
            locB["Basic usage"].Value.Should().Be("Uso DIFERENTE");
        }
        finally
        {
            CultureInfo.CurrentUICulture = prev;
        }
    }

    [Fact]
    public void Should_Return_Literal_When_Bundle_Not_Registered()
    {
        // Arrange - no bundle registered for TestResources.
        IServiceProvider sp = BuildServices();
        BobLocalizer<TestResources> loc = new(sp);

        // Act
        LocalizedString result = loc["Unknown bundle"];

        // Assert
        result.Value.Should().Be("Unknown bundle");
        result.ResourceNotFound.Should().BeTrue();
    }

    [Fact]
    public void GetAllStrings_Should_Enumerate_Source_Literals()
    {
        // Arrange
        ulong h1 = BobLocalizationHash.Compute("Hello");
        ulong h2 = BobLocalizationHash.Compute("Welcome");

        _snapshot.RegisterFake(new BobLocalizationBundleSpec(
            typeof(TestResources),
            "en-US",
            [typeof(BundleProvider), typeof(LiteralProvider)],
            [],
            new Dictionary<string, FrozenDictionary<ulong, string>>
            {
                ["es-ES"] = new Dictionary<ulong, string> { [h1] = "Hola" }.ToFrozenDictionary()
            }.ToFrozenDictionary(),
            new Dictionary<ulong, string> { [h1] = "Hello", [h2] = "Welcome" }.ToFrozenDictionary()));

        IServiceProvider sp = BuildServices();
        BobLocalizer<TestResources> loc = new(sp);

        CultureInfo prev = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
        try
        {
            // Act
            LocalizedString[] all = loc.GetAllStrings(true).ToArray();

            // Assert - Hello translates to Hola, Welcome falls back to literal.
            all.Should().HaveCount(2);
            all.Single(s => s.Name == "Hello").Value.Should().Be("Hola");
            LocalizedString welcome = all.Single(s => s.Name == "Welcome");
            welcome.Value.Should().Be("Welcome");
            welcome.ResourceNotFound.Should().BeTrue();
        }
        finally
        {
            CultureInfo.CurrentUICulture = prev;
        }
    }

    private void RegisterBundle(Dictionary<string, Dictionary<ulong, string>> translations)
    {
        FrozenDictionary<string, FrozenDictionary<ulong, string>> frozen = translations.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.ToFrozenDictionary()).ToFrozenDictionary();

        _snapshot.RegisterFake(new BobLocalizationBundleSpec(
            typeof(TestResources),
            "en-US",
            [typeof(BundleProvider), typeof(LiteralProvider)],
            [],
            frozen,
            null));
    }

    private static IServiceProvider BuildServices() =>
        new ServiceCollection()
            .AddSingleton<BundleProvider>()
            .AddSingleton<LiteralProvider>()
            .BuildServiceProvider();

    /// <summary>Test provider that returns a single fixed translation for one hash.</summary>
    private sealed class FixedProvider : IBobLocalizationProvider
    {
        private readonly ulong _hash;
        private readonly string _value;

        public FixedProvider(ulong hash, string value)
        {
            _hash = hash;
            _value = value;
        }

        public bool TryGet(BobLocalizationBundleSpec spec, ulong hash, CultureInfo culture, out string? value)
        {
            if (hash == _hash)
            {
                value = _value;
                return true;
            }

            value = null;
            return false;
        }
    }
}
