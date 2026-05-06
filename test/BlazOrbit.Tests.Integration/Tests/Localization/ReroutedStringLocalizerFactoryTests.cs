using BlazOrbit.Localization.Shared;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Trait("Localization", "ReroutedStringLocalizerFactory")]
public class ReroutedStringLocalizerFactoryTests
{
    [Fact]
    public void Constructor_Should_Throw_When_Inner_Is_Null()
    {
        Action act = () => new ReroutedStringLocalizerFactory(
            null!, new Dictionary<string, string>());
        act.Should().Throw<ArgumentNullException>().WithParameterName("inner");
    }

    [Fact]
    public void Constructor_Should_Throw_When_AssemblyMap_Is_Null()
    {
        Action act = () => new ReroutedStringLocalizerFactory(
            Substitute.For<IStringLocalizerFactory>(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("assemblyMap");
    }

    [Fact]
    public void Create_Type_Should_Passthrough_When_Assembly_Not_Mapped()
    {
        IStringLocalizerFactory inner = Substitute.For<IStringLocalizerFactory>();
        IStringLocalizer expected = Substitute.For<IStringLocalizer>();
        inner.Create(typeof(string)).Returns(expected);

        var sut = new ReroutedStringLocalizerFactory(
            inner, new Dictionary<string, string>());

        IStringLocalizer result = sut.Create(typeof(string));

        result.Should().Be(expected);
        inner.Received(1).Create(typeof(string));
    }

    [Fact]
    public void Create_Type_Should_Rewrite_When_Assembly_Is_Mapped()
    {
        IStringLocalizerFactory inner = Substitute.For<IStringLocalizerFactory>();
        IStringLocalizer expected = Substitute.For<IStringLocalizer>();

        // FakeResourceType lives in assembly "BlazOrbit.Tests.Integration"
        // FullName = "BlazOrbit.Tests.Integration.Tests.Localization.ReroutedStringLocalizerFactoryTests+FakeResourceType"
        // Rewritten = "BlazOrbit.Tests.Integration.Translations.Tests.Localization.ReroutedStringLocalizerFactoryTests+FakeResourceType"
        string rewrittenBaseName = "BlazOrbit.Tests.Integration.Translations.Tests.Localization.ReroutedStringLocalizerFactoryTests+FakeResourceType";
        string translationsAsm = "BlazOrbit.Tests.Integration.Translations";
        inner.Create(rewrittenBaseName, translationsAsm).Returns(expected);

        var map = new Dictionary<string, string>
        {
            ["BlazOrbit.Tests.Integration"] = translationsAsm
        };
        var sut = new ReroutedStringLocalizerFactory(inner, map);

        Type resourceType = typeof(FakeResourceType);

        IStringLocalizer result = sut.Create(resourceType);

        result.Should().Be(expected);
        inner.Received(1).Create(rewrittenBaseName, translationsAsm);
    }

    [Fact]
    public void Create_Type_Should_Passthrough_When_Map_Misses()
    {
        IStringLocalizerFactory inner = Substitute.For<IStringLocalizerFactory>();
        IStringLocalizer expected = Substitute.For<IStringLocalizer>();
        inner.Create(typeof(FakeResourceType)).Returns(expected);

        var map = new Dictionary<string, string>
        {
            ["OtherAssembly"] = "OtherAssembly.Translations"
        };
        var sut = new ReroutedStringLocalizerFactory(inner, map);

        IStringLocalizer result = sut.Create(typeof(FakeResourceType));

        result.Should().Be(expected);
        inner.Received(1).Create(typeof(FakeResourceType));
    }

    [Fact]
    public void Create_Type_Should_Passthrough_When_TranslationsAsm_Is_Empty()
    {
        IStringLocalizerFactory inner = Substitute.For<IStringLocalizerFactory>();
        IStringLocalizer expected = Substitute.For<IStringLocalizer>();
        inner.Create(typeof(FakeResourceType)).Returns(expected);

        var map = new Dictionary<string, string>
        {
            ["BlazOrbit.Localization.Shared"] = ""
        };
        var sut = new ReroutedStringLocalizerFactory(inner, map);

        IStringLocalizer result = sut.Create(typeof(FakeResourceType));

        result.Should().Be(expected);
    }

    [Fact]
    public void Create_Type_Should_Throw_When_ResourceSource_Is_Null()
    {
        var sut = new ReroutedStringLocalizerFactory(
            Substitute.For<IStringLocalizerFactory>(),
            new Dictionary<string, string>());

        Action act = () => sut.Create((Type)null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("resourceSource");
    }

    [Fact]
    public void Create_String_Should_Passthrough_To_Inner_Unmodified()
    {
        IStringLocalizerFactory inner = Substitute.For<IStringLocalizerFactory>();
        IStringLocalizer expected = Substitute.For<IStringLocalizer>();
        inner.Create("baseName", "location").Returns(expected);

        var sut = new ReroutedStringLocalizerFactory(
            inner, new Dictionary<string, string>());

        IStringLocalizer result = sut.Create("baseName", "location");

        result.Should().Be(expected);
        inner.Received(1).Create("baseName", "location");
    }

    // Anchor type used only for Assembly / FullName tests above.
    private sealed class FakeResourceType { }
}
