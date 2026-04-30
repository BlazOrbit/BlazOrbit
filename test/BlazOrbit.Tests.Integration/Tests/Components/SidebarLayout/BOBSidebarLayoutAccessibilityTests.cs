using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.SidebarLayout;

[Trait("Component Accessibility", "BOBSidebarLayout")]
public class BOBSidebarLayoutAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Landmark_Elements_For_Regions(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.Header, b => b.AddContent(0, "h"))
            .Add(c => c.Sidebar, b => b.AddContent(0, "s"))
            .Add(c => c.ChildContent, b => b.AddContent(0, "m")));

        // Assert — header/aside/main are semantic landmarks
        cut.Find("header.bob-sidebar-layout__header").Should().NotBeNull();
        cut.Find("aside.bob-sidebar-layout__sidebar").Should().NotBeNull();
        cut.Find("main.bob-sidebar-layout__main").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Toggle_As_Button_With_AriaLabel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.ShowToggle, true));

        // Assert
        IElement toggle = cut.Find(".bob-sidebar-layout__toggle");
        toggle.TagName.Should().Be("BUTTON");
        toggle.GetAttribute("type").Should().Be("button");
        toggle.GetAttribute("aria-label").Should().Be("Toggle navigation");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Open_State_In_AriaExpanded(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.ShowToggle, true));
        cut.Find(".bob-sidebar-layout__toggle").GetAttribute("aria-expanded").Should().Be("false");

        // Act
        cut.Find(".bob-sidebar-layout__toggle").Click();

        // Assert
        cut.Find(".bob-sidebar-layout__toggle").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Scrim_As_AriaHidden(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — scrim appears only while sidebar open
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.ShowToggle, true));
        cut.Find(".bob-sidebar-layout__toggle").Click();

        // Assert — scrim is purely decorative
        cut.Find(".bob-sidebar-layout__scrim").GetAttribute("aria-hidden").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Toggle_When_ShowToggle_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.ShowToggle, false));

        // Assert — no orphan toggle semantics when feature disabled
        cut.FindAll(".bob-sidebar-layout__toggle").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_Heading_Semantics_In_Header_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.Header, b => b.AddMarkupContent(0, "<h1 class='app-title'>App</h1>")));

        // Assert — heading lands inside header landmark
        cut.Find("header .bob-sidebar-layout__header-content h1.app-title").TextContent.Should().Be("App");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Allow_Nav_Role_In_Sidebar_Via_Markup(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — consumer owns <nav> inside sidebar slot
        IRenderedComponent<BOBSidebarLayout> cut = ctx.Render<BOBSidebarLayout>(p => p
            .Add(c => c.Sidebar, b => b.AddMarkupContent(0,
                "<nav aria-label='Primary'><a href='/'>Home</a></nav>")));

        // Assert
        IElement nav = cut.Find("aside.bob-sidebar-layout__sidebar nav");
        nav.GetAttribute("aria-label").Should().Be("Primary");
    }
}
