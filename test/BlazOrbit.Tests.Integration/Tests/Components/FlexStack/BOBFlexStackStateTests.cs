using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.FlexStack;

[Trait("Component State", "BOBFlexStack")]
public class BOBFlexStackStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Direction_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Direction, FlexStackDirection.Column));

        cut.Find("bob-component").GetAttribute("data-dir").Should().Be("column");

        // Act
        cut.Render(p => p.Add(c => c.Direction, FlexStackDirection.RowReverse));

        // Assert
        cut.Find("bob-component").GetAttribute("data-dir").Should().Be("row-reverse");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Remove_Direction_DataAttribute_When_Reset_To_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Direction, FlexStackDirection.Column));

        cut.Find("bob-component").HasAttribute("data-dir").Should().BeTrue();

        // Act
        cut.Render(p => p.Add(c => c.Direction, FlexStackDirection.Row));

        // Assert
        cut.Find("bob-component").HasAttribute("data-dir").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Gap_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Gap, "1rem"));

        cut.Find("bob-component").GetAttribute("style").Should().Contain("--gap: 1rem");

        // Act
        cut.Render(p => p.Add(c => c.Gap, "2rem"));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--gap: 2rem");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_UserAttributes_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .AddUnmatched("data-testid", "my-flex")
            .Add(c => c.Gap, "1rem"));

        // Act
        cut.Render(p => p
            .AddUnmatched("data-testid", "my-flex")
            .Add(c => c.Gap, "2rem"));

        // Assert
        cut.Find("bob-component").GetAttribute("data-testid").Should().Be("my-flex");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_FullWidth_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.FullWidth, false));

        cut.Find("bob-component").GetAttribute("data-bob-fullwidth").Should().BeNull();

        // Act
        cut.Render(p => p.Add(c => c.FullWidth, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-fullwidth").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Color_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string initialColor = "rgba(255,0,0,1)";
        string updatedColor = "rgba(0,255,0,1)";

        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Color, initialColor));

        cut.Find("bob-component").GetAttribute("style").Should().Contain($"--bob-inline-color: {initialColor}");

        // Act
        cut.Render(p => p.Add(c => c.Color, updatedColor));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain($"--bob-inline-color: {updatedColor}");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Shadow_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Shadow, ShadowStyle.Create(2, 4)));

        cut.Find("bob-component").GetAttribute("data-bob-shadow").Should().Be("true");

        // Act
        cut.Render(p => p.Add(c => c.Shadow, ShadowStyle.Create(4, 8)));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-shadow").Should().Be("true");
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-shadow:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Border_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Border, BorderStyle.Create().All("1px", BorderStyleType.Solid, "red")));

        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-border");

        // Act
        cut.Render(p => p.Add(c => c.Border, BorderStyle.Create().All("2px", BorderStyleType.Dashed, "blue")));

        // Assert
        string style = cut.Find("bob-component").GetAttribute("style")!;
        style.Should().Contain("--bob-inline-border");
        style.Should().Contain("2px dashed blue");
    }
}
