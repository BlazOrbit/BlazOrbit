using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component State", "BOBThemeEditor")]
public class BOBThemeEditorStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_New_Palette_Values_After_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>
            {
                ["Primary"] = new("#111111"),
            }));

        cut.Find("bob-component[data-bob-component='input-color'] input.bob-input__field")
           .GetAttribute("value").Should().Be("#111111");

        // Act — replace palette with a different value
        cut.Render(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>
            {
                ["Primary"] = new("#222222"),
            }));

        // Assert
        cut.Find("bob-component[data-bob-component='input-color'] input.bob-input__field")
           .GetAttribute("value").Should().Be("#222222");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Only_Inputs_For_Palette_Keys_Matching_Categories(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — palette has 2 recognised keys ("Primary", "Error") + one unknown ("Foo")
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>
            {
                ["Primary"] = new("#111111"),
                ["Error"] = new("#ff0000"),
                ["Foo"] = new("#00ff00"),
            }));

        // Assert — editor only renders inputs for keys that appear in one of its categories
        cut.FindAll("bob-component[data-bob-component='input-color']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_All_Category_Sections_Regardless_Of_Palette_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — empty palette
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, []));

        // Assert — category skeleton is always rendered (Surface, Main, Status, Utility)
        cut.FindAll(".bob-theme-editor__category").Should().HaveCount(4);
        cut.FindAll("bob-component[data-bob-component='input-color']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Normalize_Null_Palette_To_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — setter guards null with `value ?? new()`
        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, null!));

        // Assert — no inputs, but skeleton still rendered
        cut.Find(".bob-theme-editor").Should().NotBeNull();
        cut.FindAll("bob-component[data-bob-component='input-color']").Should().BeEmpty();
    }
}
