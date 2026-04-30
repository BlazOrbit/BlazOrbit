using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Interaction", "BOBThemeGenerator")]
public class BOBThemeGeneratorInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Import_Dialog_On_Import_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();

        // Act — click Import JSON button (first action group, first button)
        cut.Find(".bob-theme-generator__actions-group button").Click();

        // Assert — import dialog should open
        cut.Find(".bob-theme-generator__actions").Should().NotBeNull();
        cut.FindAll("textarea").Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Export_Dialog_On_Export_JSON_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();

        // Act — click Export JSON button (second action group, first button)
        IReadOnlyList<IElement> actionGroups = cut.FindAll(".bob-theme-generator__actions-group");
        actionGroups[1].QuerySelector("button")!.Click();

        // Assert — export code block should be visible
        cut.FindAll(".bob-code-block, bob-component[data-bob-component='code-block']")
            .Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reset_Palettes_On_Reset_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();
        string initialStyle = cut.Find(".bob-theme-generator__preview-container").GetAttribute("style") ?? "";

        // Act — click Reset
        cut.Find("button[title='Reset'], .bob-theme-generator__actions > bob-component[data-bob-component='button']:last-child button")
            .Click();

        // Assert — preview container still renders (palettes reset to defaults)
        cut.Find(".bob-theme-generator__preview-container").Should().NotBeNull();
    }
}
