using BlazOrbit.Localization;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

[Trait("Component Integration", "BobLocalizationHash")]
public class BobLocalizationHashTests
{
    [Fact]
    public void Should_Be_Deterministic_For_Same_Input()
    {
        ulong a = BobLocalizationHash.Compute("Hello, world!");
        ulong b = BobLocalizationHash.Compute("Hello, world!");
        a.Should().Be(b);
    }

    [Fact]
    public void Should_Differ_For_Different_Inputs()
    {
        ulong a = BobLocalizationHash.Compute("Hello");
        ulong b = BobLocalizationHash.Compute("World");
        a.Should().NotBe(b);
    }

    [Fact]
    public void Should_Differ_For_Case_Variations()
    {
        ulong a = BobLocalizationHash.Compute("hello");
        ulong b = BobLocalizationHash.Compute("Hello");
        a.Should().NotBe(b);
    }

    [Fact]
    public void Should_Handle_Empty_String()
    {
        // Empty input collapses to the FNV offset basis - verifies the algorithm boundary.
        ulong h = BobLocalizationHash.Compute(string.Empty);
        h.Should().Be(0xCBF29CE484222325UL);
    }

    [Fact]
    public void Should_Handle_Unicode_Characters()
    {
        ulong a = BobLocalizationHash.Compute("Hola, ¿qué tal?");
        ulong b = BobLocalizationHash.Compute("Hola, ¿qué tal?");
        a.Should().Be(b);

        ulong c = BobLocalizationHash.Compute("こんにちは");
        c.Should().NotBe(a);
    }

    [Fact]
    public void Should_Throw_On_Null_Input()
    {
        Action act = () => BobLocalizationHash.Compute(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
