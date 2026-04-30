using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component State", "BOBThemePreview")]
public class BOBThemePreviewStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dialog_When_Show_Dialog_Button_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBThemePreview> cut = ctx.Render<BOBThemePreview>();
        cut.FindAll("[role='dialog']").Should().BeEmpty();

        // Act
        cut.FindAll("button")
           .First(b => b.TextContent.Trim() == "Show Dialog")
           .Click();

        // Assert
        cut.FindAll("[role='dialog']").Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Drawer_When_Show_Drawer_Button_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBThemePreview> cut = ctx.Render<BOBThemePreview>();
        cut.FindAll("""[data-bob-component="drawer"], .bob-drawer-overlay, .bob-drawer""").Should().BeEmpty();

        // Act
        cut.FindAll("button")
           .First(b => b.TextContent.Trim() == "Show Drawer")
           .Click();

        // Assert — drawer host materialized
        cut.FindAll("""[data-bob-component="drawer"], .bob-drawer-overlay, .bob-drawer""").Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Text_Value_Across_Inputs(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — the two "Text Input" + "With Helper" fields share `_textValue`
        IRenderedComponent<BOBThemePreview> cut = ctx.Render<BOBThemePreview>();
        IReadOnlyList<IElement> textInputs =
            cut.FindAll("bob-component[data-bob-component='input-text'] input.bob-input__field");
        textInputs.Should().HaveCountGreaterThan(1);

        // Act
        textInputs[0].Change("shared");

        // Assert — second input (bound to the same backing field) now reflects the same value
        IReadOnlyList<IElement> after =
            cut.FindAll("bob-component[data-bob-component='input-text'] input.bob-input__field");
        after[1].GetAttribute("value").Should().Be("shared");
    }
}
