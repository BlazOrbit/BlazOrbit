using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.SidebarLayout;

[Trait("Component Rendering", "BOBSidebarLayout")]
public class BOBSidebarLayoutRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "main")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("sidebar-layout");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Sidebar_And_Main_Slots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.Sidebar, b => b.AddContent(0, "Sidebar content"))
            .Add(c => c.ChildContent, b => b.AddContent(0, "Main content")));

        // Assert
        cut.Find(".bob-sidebar-layout__sidebar-inner").TextContent.Should().Contain("Sidebar content");
        cut.Find(".bob-sidebar-layout__main").TextContent.Should().Contain("Main content");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Header_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.Header, b => b.AddContent(0, "App Header")));

        // Assert
        cut.Find(".bob-sidebar-layout__header-content").TextContent.Should().Contain("App Header");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Toggle_Button_When_ShowToggle(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.ShowToggle, true));

        // Assert
        cut.Find(".bob-sidebar-layout__toggle").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_SidebarSide_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.SidebarSide, SidebarSide.End));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-sidebar-side").Should().Be("end");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_SidebarWidth_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.SidebarWidth, "300px"));

        // Assert
        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-sidebar-width: 300px");
    }
}
