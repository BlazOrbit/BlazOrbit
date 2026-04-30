using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Tabs;

[Trait("Component Accessibility", "BOBTabs")]
public class BOBTabsAccessibilityTests
{
    private static RenderFragment TwoTabs => b =>
    {
        b.OpenComponent<BOBTab>(0);
        b.AddAttribute(1, "Id", "tab1");
        b.AddAttribute(2, "Label", "Tab One");
        b.AddAttribute(3, nameof(BOBTab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "C1")));
        b.CloseComponent();
        b.OpenComponent<BOBTab>(4);
        b.AddAttribute(5, "Id", "tab2");
        b.AddAttribute(6, "Label", "Tab Two");
        b.AddAttribute(7, nameof(BOBTab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "C2")));
        b.CloseComponent();
    };

    private static RenderFragment ThreeTabs_Middle_Disabled => b =>
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
    public async Task Should_Active_Tab_Have_Aria_Selected_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Assert
        cut.FindAll("[role='tab']")[0].GetAttribute("aria-selected").Should().Be("true");
        cut.FindAll("[role='tab']")[1].GetAttribute("aria-selected").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Tab_Have_Aria_Controls_Pointing_To_Panel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Assert — aria-controls matches panel id
        cut.FindAll("[role='tab']")[0].GetAttribute("aria-controls")
            .Should().Be("bob-tabpanel-tab1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Active_Tab_Have_Tabindex_Zero(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Assert — active tab tabindex=0, inactive=-1 (roving tabindex)
        cut.FindAll("[role='tab']")[0].GetAttribute("tabindex").Should().Be("0");
        cut.FindAll("[role='tab']")[1].GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Panel_Have_Role_Tabpanel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs));

        // Assert
        cut.Find("[role='tabpanel']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Panel_Have_Aria_LabelledBy_Pointing_To_Tab(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Assert
        cut.Find("[role='tabpanel']").GetAttribute("aria-labelledby")
            .Should().Be("bob-tab-tab1");
    }

    // ──────────────────────────────────────────────────────────────────
    // WAI-ARIA Tabs keyboard navigation (A11Y-08)
    // ──────────────────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Activate_Next_Tab_On_ArrowRight(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Act
        cut.FindAll("[role='tab']")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        cut.Instance.ActiveTab.Should().Be("tab2");
        cut.FindAll("[role='tab']")[1].GetAttribute("aria-selected").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Activate_Previous_Tab_On_ArrowLeft(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab2"));

        // Act
        cut.FindAll("[role='tab']")[1].KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        cut.Instance.ActiveTab.Should().Be("tab1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Wrap_To_First_When_ArrowRight_From_Last(BlazorScenario scenario)
    {
        // WAI-ARIA Tabs allow either wrapping or stopping at the boundary;
        // BlazOrbit chose wrapping (FindNextEnabled uses modulo arithmetic).
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — start on the last tab
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab2"));

        // Act
        cut.FindAll("[role='tab']")[1].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert — wraps to first
        cut.Instance.ActiveTab.Should().Be("tab1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Wrap_To_Last_When_ArrowLeft_From_First(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — start on the first tab
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Act
        cut.FindAll("[role='tab']")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert — wraps to last
        cut.Instance.ActiveTab.Should().Be("tab2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Activate_First_Tab_On_Home(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — focus on last tab
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab2"));

        // Act
        cut.FindAll("[role='tab']")[1].KeyDown(new KeyboardEventArgs { Key = "Home" });

        // Assert
        cut.Instance.ActiveTab.Should().Be("tab1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Activate_Last_Tab_On_End(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — focus on first tab
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Act
        cut.FindAll("[role='tab']")[0].KeyDown(new KeyboardEventArgs { Key = "End" });

        // Assert
        cut.Instance.ActiveTab.Should().Be("tab2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Disabled_Tabs_On_ArrowRight(BlazorScenario scenario)
    {
        // WAI-ARIA Tabs spec: arrow navigation skips over disabled tabs so
        // the user never lands on a non-actionable target.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — tab2 is disabled, focus starts on tab1
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, ThreeTabs_Middle_Disabled)
            .Add(c => c.ActiveTab, "tab1"));

        // Act
        cut.FindAll("[role='tab']")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert — jumps over tab2 directly to tab3
        cut.Instance.ActiveTab.Should().Be("tab3");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Ignore_Unrelated_Keys(BlazorScenario scenario)
    {
        // Tab navigation must not eat keys outside the spec — Enter/Space/etc.
        // belong to the tab's click semantics, not to navigation.
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Act
        cut.FindAll("[role='tab']")[0].KeyDown(new KeyboardEventArgs { Key = "Tab" });
        cut.FindAll("[role='tab']")[0].KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert — active tab unchanged
        cut.Instance.ActiveTab.Should().Be("tab1");
    }
}
