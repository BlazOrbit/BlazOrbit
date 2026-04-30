using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.FlexStack;

[Trait("Component Rendering", "BOBFlexStack")]
public class BOBFlexStackRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("flex-stack");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Direction_DataAttribute_When_Not_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Direction, FlexStackDirection.Column));

        // Assert
        cut.Find("bob-component").GetAttribute("data-dir").Should().Be("column");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Wrap_DataAttribute_When_Not_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Wrap, FlexStackWrap.NoWrap));

        // Assert
        cut.Find("bob-component").GetAttribute("data-wrap").Should().Be("no-wrap");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_JustifyContent_DataAttribute_When_Not_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.JustifyContent, FlexStackJustifyContent.Center));

        // Assert
        cut.Find("bob-component").GetAttribute("data-justify").Should().Be("center");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AlignItems_DataAttribute_When_Not_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.AlignItems, FlexStackAlignItems.Center));

        // Assert
        cut.Find("bob-component").GetAttribute("data-align").Should().Be("center");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AlignContent_DataAttribute_When_Not_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.AlignContent, FlexStackAlignContent.SpaceBetween));

        // Assert
        cut.Find("bob-component").GetAttribute("data-align-content").Should().Be("space-between");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Gap_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Gap, "1.5rem"));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--gap: 1.5rem");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_FullWidth_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.FullWidth, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-fullwidth").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Children(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "Stack child")));

        // Assert
        cut.Find("bob-component").TextContent.Should().Contain("Stack child");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Spacing_CssVariables(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.P, "1rem")
            .Add(c => c.Mx, "auto"));

        // Assert
        string style = cut.Find("bob-component").GetAttribute("style")!;
        style.Should().Contain("--p: 1rem");
        style.Should().Contain("--mr: auto");
        style.Should().Contain("--ml: auto");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_BackgroundColor_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.BackgroundColor, "rgba(255,0,0,1)"));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-background: rgba(255,0,0,1)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Color_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Color, "rgba(0,255,0,1)"));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-color: rgba(0,255,0,1)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Shadow_DataAttribute_And_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Shadow, ShadowStyle.Create(4, 8)));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-shadow").Should().Be("true");
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-shadow:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Border_InlineVar(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBFlexStack> cut = ctx.Render<BOBFlexStack>(p => p
            .Add(c => c.Border, BorderStyle.Create().All("1px", BorderStyleType.Solid, "red")));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-border");
    }
}
