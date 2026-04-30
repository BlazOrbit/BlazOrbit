using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Rendering", "BOBThemeEditor")]
public class BOBThemeEditorRenderingTests
{
    private static Dictionary<string, CssColor> CreatePalette() => new()
    {
        ["Primary"] = new CssColor("#1A73E8"),
        ["PrimaryContrast"] = new CssColor("#FFFFFF"),
        ["Background"] = new CssColor("#121212"),
        ["BackgroundContrast"] = new CssColor("#FFFFFF"),
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_Editor_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert
        cut.Find(".bob-theme-editor").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Category_Sections(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert — at least one category rendered
        cut.FindAll(".bob-theme-editor__category").Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Category_Titles(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert — category title elements rendered
        cut.FindAll(".bob-theme-editor__category-title").Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Color_Input_For_Each_Palette_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert — color inputs present for palette keys that match categories
        cut.FindAll("bob-component[data-bob-component='input-color'], input[type='color']")
            .Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Color_Components(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert — BOBInputColor components rendered for matching palette keys
        cut.FindAll("bob-component[data-bob-component='input-color']")
            .Should().HaveCountGreaterThan(0);
    }
}
