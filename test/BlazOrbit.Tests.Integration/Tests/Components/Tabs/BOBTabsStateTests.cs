using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Tabs;

[Trait("Component State", "BOBTabs")]
public class BOBTabsStateTests
{
    private static RenderFragment OneTab => b =>
    {
        b.OpenComponent<BOBTab>(0);
        b.AddAttribute(1, "Id", "tab1");
        b.AddAttribute(2, "Label", "Tab One");
        b.AddAttribute(3, nameof(BOBTab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Content One")));
        b.CloseComponent();
    };

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

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Switch_Active_Tab_When_ActiveTab_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        cut.Find("[role='tabpanel']").TextContent.Should().Contain("Content One");

        // Act
        cut.Render(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab2"));

        // Assert
        cut.Find("[role='tabpanel']").TextContent.Should().Contain("Content Two");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Second_Tab_Active_When_Switched(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab2"));

        // Assert — second tab button has active
        IReadOnlyList<IElement> tabs = cut.FindAll("[role='tab']");
        tabs[0].GetAttribute("data-bob-active").Should().BeNull();
        tabs[1].GetAttribute("data-bob-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Re_Render_Header_When_Tab_Added_Dynamically(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — start with one tab
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, OneTab));

        cut.FindAll("[role='tab']").Should().HaveCount(1);

        // Act — add a second tab via parameter change
        cut.Render(p => p.Add(c => c.ChildContent, TwoTabs));

        // Assert — header re-rendered with both tabs (RegisterTab → StateHasChanged)
        cut.FindAll("[role='tab']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Re_Render_Header_When_Tab_Removed_Dynamically(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — start with two tabs
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs));

        cut.FindAll("[role='tab']").Should().HaveCount(2);

        // Act — remove the second tab via parameter change
        cut.Render(p => p.Add(c => c.ChildContent, OneTab));

        // Assert — header re-rendered with one tab (UnregisterTab → StateHasChanged)
        cut.FindAll("[role='tab']").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.Size, BOBSize.Small));

        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("small");

        // Act
        cut.Render(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.Size, BOBSize.Large));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
    }
}
