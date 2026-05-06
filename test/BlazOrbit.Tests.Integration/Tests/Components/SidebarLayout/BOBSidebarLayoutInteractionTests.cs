using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.SidebarLayout;

[Trait("Component Interaction", "BOBSidebarLayout")]
public class BOBSidebarLayoutInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Open_On_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.SidebarOpen, false)
            .Add(c => c.ShowToggle, true));

        cut.Find("bob-component").GetAttribute("data-bob-sidebar-open").Should().BeNull();

        // Act
        cut.Find(".bob-sidebar-layout__toggle").Click();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-sidebar-open").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_SidebarOpenChanged_On_Toggle(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? captured = null;
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.ShowToggle, true)
            .Add(c => c.SidebarOpenChanged, v => captured = v));

        // Act
        cut.Find(".bob-sidebar-layout__toggle").Click();

        // Assert
        captured.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_On_Scrim_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? captured = null;
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.SidebarOpen, true)
            .Add(c => c.SidebarOpenChanged, v => captured = v));

        // Act
        cut.Find(".bob-sidebar-layout__scrim").Click();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-sidebar-open").Should().BeNull();
        captured.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Button_Have_Aria_Expanded(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.SidebarOpen, true)
            .Add(c => c.ShowToggle, true));

        // Assert
        cut.Find(".bob-sidebar-layout__toggle").GetAttribute("aria-expanded").Should().Be("true");
    }
}
