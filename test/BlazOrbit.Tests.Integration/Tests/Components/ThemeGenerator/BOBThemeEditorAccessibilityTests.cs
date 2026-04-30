using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Accessibility", "BOBThemeEditor")]
public class BOBThemeEditorAccessibilityTests
{
    private static Dictionary<string, CssColor> CreatePalette() => new()
    {
        ["Primary"] = new CssColor("#1A73E8"),
        ["PrimaryContrast"] = new CssColor("#FFFFFF"),
        ["Background"] = new CssColor("#121212"),
        ["BackgroundContrast"] = new CssColor("#FFFFFF"),
        ["Error"] = new CssColor("#D32F2F"),
        ["ErrorContrast"] = new CssColor("#FFFFFF"),
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Category_Titles_As_Headings(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert — categories use h4 headings
        IReadOnlyList<IElement> headings = cut.FindAll("h4.bob-theme-editor__category-title");
        headings.Should().NotBeEmpty();
        foreach (IElement h in headings)
        {
            h.TextContent.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Color_Inputs_With_Labels(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert — every color input renders a non-empty label so the input is identifiable
        IReadOnlyList<IElement> labels = cut.FindAll(".bob-input__label");
        labels.Should().NotBeEmpty();
        foreach (IElement label in labels)
        {
            label.TextContent.Should().NotBeNullOrWhiteSpace();
        }
    }
}
