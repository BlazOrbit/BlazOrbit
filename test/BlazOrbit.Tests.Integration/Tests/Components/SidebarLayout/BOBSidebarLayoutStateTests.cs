using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.SidebarLayout;

[Trait("Component State", "BOBSidebarLayout")]
public class BOBSidebarLayoutStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Scrim_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.SidebarOpen, true)
            .Add(c => c.ShowToggle, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-sidebar-open").Should().Be("true");
        cut.FindAll(".bob-sidebar-layout__scrim").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Scrim_When_Closed(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.SidebarOpen, false));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-sidebar-open").Should().Be("false");
        cut.FindAll(".bob-sidebar-layout__scrim").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Open_State_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.SidebarOpen, false));

        // Act
        cut.Render(p => p.Add(c => c.SidebarOpen, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-sidebar-open").Should().Be("true");
    }
}
