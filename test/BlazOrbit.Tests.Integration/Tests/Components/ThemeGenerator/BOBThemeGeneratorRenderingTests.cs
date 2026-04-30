using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Rendering", "BOBThemeGenerator")]
public class BOBThemeGeneratorRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();

        // Assert
        cut.Find(".bob-theme-generator").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Actions_Section(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();

        // Assert
        cut.Find(".bob-theme-generator__actions").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Preview_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();

        // Assert
        cut.Find(".bob-theme-generator__preview-container").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Editor_Section(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();

        // Assert
        cut.Find(".bob-theme-generator__editor").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Export_Action_Group(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();

        // Assert
        cut.FindAll(".bob-theme-generator__actions-group").Should().HaveCount(2);
    }
}
