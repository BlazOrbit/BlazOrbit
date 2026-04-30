using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Validation", "BOBThemeGenerator")]
public class BOBThemeGeneratorValidationTests
{
    private static void OpenImportDialog(IRenderedComponent<BOBThemeGenerator> cut) => cut.Find(".bob-theme-generator__actions-group button").Click();

    private static void ClickImportButton(IRenderedComponent<BOBThemeGenerator> cut)
    {
        IElement? btn = cut.FindAll("[role='dialog'] button")
            .FirstOrDefault(b => b.TextContent.Trim() == "Import");
        btn?.Click();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_For_Empty_Import(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();
        OpenImportDialog(cut);

        // Act — click Import without pasting anything
        ClickImportButton(cut);

        // Assert
        cut.Find(".bob-theme-generator__import-error").TextContent
            .Should().Contain("Please paste JSON content");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_For_Invalid_JSON(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();
        OpenImportDialog(cut);

        // Act — change textarea value (onchange event triggers @bind-Value update) and click Import
        cut.Find("textarea").Change("not valid json {{");
        ClickImportButton(cut);

        // Assert
        cut.Find(".bob-theme-generator__import-error").TextContent
            .Should().Contain("Invalid JSON");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Import_Dialog_On_Valid_JSON(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBThemeGenerator> cut = ctx.Render<BOBThemeGenerator>();
        OpenImportDialog(cut);

        // Act
        string validJson = "{\"dark\": {\"primary\": \"#ff0000\"}, \"light\": {\"primary\": \"#0000ff\"}}";
        cut.Find("textarea").Change(validJson);
        ClickImportButton(cut);

        // Assert — no import error shown after successful import
        cut.FindAll(".bob-theme-generator__import-error").Should().BeEmpty();
    }
}
