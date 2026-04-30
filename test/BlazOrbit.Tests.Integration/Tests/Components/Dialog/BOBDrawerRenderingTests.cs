using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Rendering", "BOBDrawer")]
public class BOBDrawerRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_When_Closed(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, false));

        // Assert
        cut.FindAll("[role='dialog']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true));

        // Assert
        cut.Find("[role='dialog']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Right_Position_Class_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true));

        // Assert
        cut.Find(".bob-drawer--right").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Left_Position_Class(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Position, DrawerPosition.Left));

        // Assert
        cut.Find(".bob-drawer--left").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBDrawer> cut = ctx.Render<BOBDrawer>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "Drawer content")));

        // Assert
        cut.Find(".bob-drawer__content").TextContent.Should().Contain("Drawer content");
    }
}
