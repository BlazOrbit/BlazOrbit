using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component Rendering", "BOBThemeSelector")]
public class BOBThemeSelectorRenderingTests
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
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("theme-selector");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>();

        // Assert
        cut.Find("button").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Theme_Icon_When_ShowIcon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx, "light");

        // Arrange & Act
        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>(p => p
            .Add(c => c.ShowIcon, true));

        // Assert
        cut.Find(".bob-theme-switch__icon").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Icon_When_ShowIcon_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>(p => p
            .Add(c => c.ShowIcon, false));

        // Assert
        cut.FindAll(".bob-theme-switch__icon").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("default");
    }
}
