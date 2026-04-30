using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Rendering", "BOBThemePreview")]
public class BOBThemePreviewRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_Preview_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemePreview> cut = ctx.Render<BOBThemePreview>();

        // Assert
        cut.Find(".bob-theme-preview").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Preview_Sections(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemePreview> cut = ctx.Render<BOBThemePreview>();

        // Assert — multiple sections rendered (Buttons, Inputs, Selection, Card, etc.)
        cut.FindAll(".bob-theme-preview__section").Should().HaveCountGreaterThan(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Buttons_Section(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemePreview> cut = ctx.Render<BOBThemePreview>();

        // Assert — buttons exist in preview
        cut.FindAll("bob-component[data-bob-component='button']").Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Card_Section(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemePreview> cut = ctx.Render<BOBThemePreview>();

        // Assert
        cut.FindAll("bob-component[data-bob-component='card']").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Preview_Rows(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemePreview> cut = ctx.Render<BOBThemePreview>();

        // Assert
        cut.FindAll(".bob-theme-preview__row").Should().HaveCountGreaterThan(0);
    }
}
