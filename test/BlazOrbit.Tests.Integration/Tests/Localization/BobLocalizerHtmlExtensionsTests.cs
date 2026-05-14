using System.Collections.Frozen;
using BlazOrbit.Localization;
using BlazOrbit.Localization.Providers;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Collection("BobLocalize-StaticState")]
[Trait("Component Integration", "BobLocalizerHtmlExtensions")]
public class BobLocalizerHtmlExtensionsTests : IDisposable
{
    private readonly BobLocalizeStateSnapshot _snapshot = new();
    public void Dispose() => _snapshot.Dispose();

    private sealed class TestResources
    {
    }

    [Fact]
    public void Html_Should_Return_Template_Verbatim_When_No_Args()
    {
        Register("FormsIntro", "Build <strong>typed</strong> forms.");

        BobLocalizer<TestResources> loc = new(BuildServices());

        MarkupString result = loc.Html("FormsIntro");

        result.Value.Should().Be("Build <strong>typed</strong> forms.");
    }

    [Fact]
    public void Html_Should_HtmlEncode_Format_Arguments()
    {
        // Template is trusted (ships in .tn), but the {0} value can be hostile —
        // the encoder must neutralise it before substitution.
        Register("WelcomeUser", "Hello, <strong>{0}</strong>!");

        BobLocalizer<TestResources> loc = new(BuildServices());

        MarkupString result = loc.Html("WelcomeUser", "<script>alert(1)</script>");

        result.Value.Should().Be("Hello, <strong>&lt;script&gt;alert(1)&lt;/script&gt;</strong>!");
    }

    [Fact]
    public void Html_Should_Handle_Null_Arg_As_Empty_String()
    {
        Register("WelcomeUser", "Hello, <strong>{0}</strong>!");

        BobLocalizer<TestResources> loc = new(BuildServices());

        MarkupString result = loc.Html("WelcomeUser", [null]);

        result.Value.Should().Be("Hello, <strong></strong>!");
    }

    [Fact]
    public void Html_Should_Fallback_To_Literal_Key_When_Translation_Missing()
    {
        // No bundle registered for TestResources — BobLocalizer returns the key itself.
        BobLocalizer<TestResources> loc = new(BuildServices());

        MarkupString result = loc.Html("UnknownKey");

        result.Value.Should().Be("UnknownKey");
    }

    [Fact]
    public void Html_Should_Throw_On_Null_Localizer()
    {
        IStringLocalizer<TestResources>? loc = null;
        Action act = () => loc!.Html("k");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Html_Should_Throw_On_Null_Key()
    {
        BobLocalizer<TestResources> loc = new(BuildServices());
        Action act = () => loc.Html(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    private void Register(string key, string value)
    {
        ulong hash = BobLocalizationHash.Compute(key);
        FrozenDictionary<string, FrozenDictionary<ulong, string>> translations =
            new Dictionary<string, FrozenDictionary<ulong, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["en-US"] = new Dictionary<ulong, string> { [hash] = value }.ToFrozenDictionary()
            }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        _snapshot.RegisterFake(new BobLocalizationBundleSpec(
            typeof(TestResources),
            "en-US",
            [typeof(BundleProvider), typeof(LiteralProvider)],
            [],
            translations,
            null));
    }

    private static IServiceProvider BuildServices() =>
        new ServiceCollection()
            .AddSingleton<BundleProvider>()
            .AddSingleton<LiteralProvider>()
            .BuildServiceProvider();
}