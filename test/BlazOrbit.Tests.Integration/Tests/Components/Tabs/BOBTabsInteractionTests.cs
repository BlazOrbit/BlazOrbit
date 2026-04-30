using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Tabs;

[Trait("Component Interaction", "BOBTabs")]
public class BOBTabsInteractionTests
{
    private static RenderFragment TwoTabs => b =>
    {
        b.OpenComponent<BOBTab>(0);
        b.AddAttribute(1, "Id", "tab1");
        b.AddAttribute(2, "Label", "Tab One");
        b.AddAttribute(3, nameof(BOBTab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Content One")));
        b.CloseComponent();
        b.OpenComponent<BOBTab>(4);
        b.AddAttribute(5, "Id", "tab2");
        b.AddAttribute(6, "Label", "Tab Two");
        b.AddAttribute(7, nameof(BOBTab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Content Two")));
        b.CloseComponent();
    };

    private static RenderFragment ThreeTabsWithDisabled => b =>
    {
        b.OpenComponent<BOBTab>(0);
        b.AddAttribute(1, "Id", "tab1");
        b.AddAttribute(2, "Label", "Tab One");
        b.AddAttribute(3, nameof(BOBTab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "C1")));
        b.CloseComponent();
        b.OpenComponent<BOBTab>(4);
        b.AddAttribute(5, "Id", "tab2");
        b.AddAttribute(6, "Label", "Tab Two");
        b.AddAttribute(7, "Disabled", true);
        b.AddAttribute(8, nameof(BOBTab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "C2")));
        b.CloseComponent();
        b.OpenComponent<BOBTab>(9);
        b.AddAttribute(10, "Id", "tab3");
        b.AddAttribute(11, "Label", "Tab Three");
        b.AddAttribute(12, nameof(BOBTab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "C3")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Switch_Tab_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? capturedTab = null;
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTabChanged, t => capturedTab = t));

        // Act — click second tab
        cut.FindAll("[role='tab']")[1].Click();

        // Assert
        capturedTab.Should().Be("tab2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_Right_With_ArrowRight(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? capturedTab = null;
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1")
            .Add(c => c.ActiveTabChanged, t => capturedTab = t));

        // Act — press ArrowRight on first tab
        cut.FindAll("[role='tab']")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        capturedTab.Should().Be("tab2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_Left_With_ArrowLeft(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? capturedTab = null;
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab2")
            .Add(c => c.ActiveTabChanged, t => capturedTab = t));

        // Act — press ArrowLeft on second tab
        cut.FindAll("[role='tab']")[1].KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        capturedTab.Should().Be("tab1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Jump_To_Last_With_End_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? capturedTab = null;
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1")
            .Add(c => c.ActiveTabChanged, t => capturedTab = t));

        // Act
        cut.FindAll("[role='tab']")[0].KeyDown(new KeyboardEventArgs { Key = "End" });

        // Assert
        capturedTab.Should().Be("tab2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Disabled_Tab_With_ArrowRight(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? capturedTab = null;
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, ThreeTabsWithDisabled)
            .Add(c => c.ActiveTab, "tab1")
            .Add(c => c.ActiveTabChanged, t => capturedTab = t));

        // Act — ArrowRight should skip disabled tab2 → land on tab3
        cut.FindAll("[role='tab']")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        capturedTab.Should().Be("tab3");
    }
}
