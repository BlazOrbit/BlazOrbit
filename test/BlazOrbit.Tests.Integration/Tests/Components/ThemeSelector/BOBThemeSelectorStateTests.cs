using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component State", "BOBThemeSelector")]
public class BOBThemeSelectorStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Light_Label_When_Theme_Is_Light(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — GetThemeAsync returns "light"
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>("light"));
        ctx.Services.AddScoped(_ => fake);

        // Act
        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>();

        // Assert — after firstRender, label = "Light"
        cut.Find(".bob-theme-switch__label").TextContent.Should().Be("Light");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Dark_Label_When_GetThemeAsync_Returns_Dark(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — GetThemeAsync returns "dark"
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>("dark"));
        ctx.Services.AddScoped(_ => fake);

        // Act
        IRenderedComponent<BOBThemeSelector> cut = ctx.Render<BOBThemeSelector>();

        // Assert
        cut.Find(".bob-theme-switch__label").TextContent.Should().Be("Dark");
    }
}
