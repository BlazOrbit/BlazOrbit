using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component State", "BOBThemeGenerator")]
public class BOBThemeGeneratorStateTests
{
    private static void OpenImportDialog(IRenderedComponent<BOBThemeGenerator> cut)
        => cut.Find(".bob-theme-generator__actions-group button").Click();

    private static void ClickImportButton(IRenderedComponent<BOBThemeGenerator> cut)
        => cut.FindAll("[role='dialog'] button")
           .First(b => b.TextContent.Trim() == "Import")
           .Click();

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Palette_Vars_In_Preview_Style(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();

        // Assert — preview container carries palette CSS vars from active (dark) theme
        string style = cut.Find(".bob-theme-generator__preview-container").GetAttribute("style") ?? "";
        style.Should().Contain("--palette-primary:");
        style.Should().Contain("--palette-background:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Roundtrip_Import_Then_Export_To_Same_Color(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();
        OpenImportDialog(cut);
        cut.Find("textarea").Change("{\"dark\":{\"primary\":\"#aabbcc\"}}");
        ClickImportButton(cut);

        // Act — open Export JSON (second action group → first button)
        IReadOnlyList<IElement> actionGroups = cut.FindAll(".bob-theme-generator__actions-group");
        actionGroups[1].QuerySelector("button")!.Click();

        // Assert — imported value appears in exported JSON
        string exported = cut.Find("[role='dialog'] .bob-code-block__content").TextContent;
        exported.ToLowerInvariant().Should().Contain("#aabbcc");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Report_Partial_Parse_Failures_Inline(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();
        OpenImportDialog(cut);
        cut.Find("textarea").Change("{\"dark\":{\"primary\":\"not-a-color\"}}");

        // Act
        ClickImportButton(cut);

        // Assert — dialog stays open, error container surfaces failure per key
        string error = cut.Find(".bob-theme-generator__import-error").TextContent;
        error.Should().Contain("dark.primary");
        error.Should().Contain("not-a-color");
        cut.FindAll("[role='dialog']").Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Restore_Default_Palette_On_Reset(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — mutate via import
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();
        string beforeStyle = cut.Find(".bob-theme-generator__preview-container").GetAttribute("style") ?? "";

        OpenImportDialog(cut);
        cut.Find("textarea").Change("{\"dark\":{\"primary\":\"#aabbcc\"}}");
        ClickImportButton(cut);

        string mutatedStyle = cut.Find(".bob-theme-generator__preview-container").GetAttribute("style") ?? "";
        mutatedStyle.Should().Contain("#aabbcc");

        // Act — click Reset (last button in actions row)
        cut.FindAll(".bob-theme-generator__actions > bob-component[data-bob-component='button'] button")
           .Last()
           .Click();

        // Assert — preview style returns to initial defaults (no longer contains mutated color)
        string afterStyle = cut.Find(".bob-theme-generator__preview-container").GetAttribute("style") ?? "";
        afterStyle.Should().NotContain("#aabbcc");
        afterStyle.Should().Be(beforeStyle);
    }
}
