using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component Variants", "BOBThemeSelector")]
public class BOBThemeSelectorVariantTests
{
    private static void RegisterFakeTheme(BlazorTestContextBase ctx, string currentTheme = "light")
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>(currentTheme));
        fake.ToggleThemeAsync(Arg.Any<string[]>()).Returns(new ValueTask<string>("dark"));
        ctx.Services.AddScoped(_ => fake);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>(p => p
            .Add(c => c.Variant, BOBThemeSelectorVariant.Default));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("default");
        cut.Find("button").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_SunMoon_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>(p => p
            .Add(c => c.Variant, BOBThemeSelectorVariant.SunMoon));

        // Assert — SunMoon renders BOBSwitch (which has a bob-component)
        cut.Find("bob-component").Should().NotBeNull();
    }
}
