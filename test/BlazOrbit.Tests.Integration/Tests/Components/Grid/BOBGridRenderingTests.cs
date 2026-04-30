using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Grid;

[Trait("Component Rendering", "BOBGrid")]
public class BOBGridRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGrid> cut = ctx.Render<BOBGrid>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("grid");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Columns_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGrid> cut = ctx.Render<BOBGrid>(p => p
            .Add(c => c.Columns, 3));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--columns: 3");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Gap_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGrid> cut = ctx.Render<BOBGrid>(p => p
            .Add(c => c.Gap, "1rem"));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--gap: 1rem");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_MaxWidth_DataAttribute_And_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGrid> cut = ctx.Render<BOBGrid>(p => p
            .Add(c => c.MaxWidth, "1200px"));

        // Assert
        cut.Find("bob-component").GetAttribute("data-contained").Should().Be("true");
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--max-w: 1200px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Direction_DataAttribute_When_Not_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGrid> cut = ctx.Render<BOBGrid>(p => p
            .Add(c => c.Direction, GridDirection.Column));

        // Assert
        cut.Find("bob-component").GetAttribute("data-dir").Should().Be("column");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Children(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGrid> cut = ctx.Render<BOBGrid>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "Grid child")));

        // Assert
        cut.Find("bob-component").TextContent.Should().Contain("Grid child");
    }
}
