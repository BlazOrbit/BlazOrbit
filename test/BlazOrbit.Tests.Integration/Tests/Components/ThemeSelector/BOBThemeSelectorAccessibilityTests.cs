using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component Accessibility", "BOBThemeSelector")]
public class BOBThemeSelectorAccessibilityTests
{
    private static void RegisterFakeTheme(BlazorTestContextBase ctx, string theme = "light")
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>(theme));
        ctx.Services.AddScoped(_ => fake);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Button_Be_Present_And_Enabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>();

        // Assert
        cut.Find("button").HasAttribute("disabled").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Theme_Label_Be_Visible(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx, "light");

        // Arrange & Act
        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>();

        // Assert
        cut.Find(".bob-theme-switch__label").Should().NotBeNull();
    }
}
