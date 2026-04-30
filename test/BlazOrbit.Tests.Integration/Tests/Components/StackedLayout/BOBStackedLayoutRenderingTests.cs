using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.StackedLayout;

[Trait("Component Rendering", "BOBStackedLayout")]
public class BOBStackedLayoutRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "main")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("stacked-layout");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Header_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Header, b => b.AddContent(0, "App Header")));

        // Assert
        cut.Find(".bob-stacked-layout__header-content").TextContent.Should().Contain("App Header");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nav_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddContent(0, "Nav links")));

        // Assert
        cut.Find(".bob-stacked-layout__nav-inner").TextContent.Should().Contain("Nav links");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Content_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "Page content")));

        // Assert
        cut.Find(".bob-stacked-layout__main-inner").TextContent.Should().Contain("Page content");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Toggle_When_Nav_Present_And_ShowToggle(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddContent(0, "nav"))
            .Add(c => c.ShowToggle, true));

        // Assert
        cut.Find(".bob-stacked-layout__toggle").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Nav_When_Not_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "main")));

        // Assert
        cut.FindAll(".bob-stacked-layout__nav").Should().BeEmpty();
    }
}
