using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Grid;

[Trait("Component State", "BOBGridItem")]
public class BOBGridItemStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Sized_Attribute_When_Span_Is_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .Add(c => c.Span, 6));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-sized").Should().Be("true");
        root.GetAttribute("style").Should().Contain("--span: 6");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Auto_Attribute_When_Auto_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .Add(c => c.Auto, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-auto").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Css_Vars_On_Reparameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .Add(c => c.Span, 4));
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--span: 4");

        // Act
        cut.Render(p => p.Add(c => c.Span, 8));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--span: 8");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Hide_And_Show_Breakpoint_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .Add(c => c.HideXs, true)
            .Add(c => c.ShowMd, true));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-hide-xs").Should().Be("true");
        root.GetAttribute("data-show-md").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Offset_Css_Variable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .Add(c => c.Offset, 3));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("style").Should().Contain("--offset: 3");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Responsive_Offset_Css_Variables(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBGridItem> cut = ctx.Render<BOBGridItem>(p => p
            .Add(c => c.OffsetXs, 1)
            .Add(c => c.OffsetMd, 4));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("style").Should().Contain("--offset-xs: 1");
        root.GetAttribute("style").Should().Contain("--offset-md: 4");
    }
}
