using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Grid;

[Trait("Component Rendering", "BOBGridItem")]
public class BOBGridItemRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("grid-item");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Span_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .Add(c => c.Span, 2));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--span: 2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataSized_When_Span_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .Add(c => c.Span, 4));

        // Assert
        cut.Find("bob-component").GetAttribute("data-sized").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataAuto_When_Auto(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .Add(c => c.Auto, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-auto").Should().Be("true");
    }
}
