using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.StackedLayout;

[Trait("Component Accessibility", "BOBStackedLayout")]
public class BOBStackedLayoutAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Landmark_Elements(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Header, b => b.AddContent(0, "h"))
            .Add(c => c.Nav, b => b.AddContent(0, "n"))
            .Add(c => c.ChildContent, b => b.AddContent(0, "m")));

        // Assert — header/nav/main are native landmarks
        cut.Find("header.bob-stacked-layout__header").Should().NotBeNull();
        cut.Find("nav.bob-stacked-layout__nav").Should().NotBeNull();
        cut.Find("main.bob-stacked-layout__main").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Toggle_As_Button_With_AriaLabel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddContent(0, "n")));

        // Assert
        IElement toggle = cut.Find(".bob-stacked-layout__toggle");
        toggle.TagName.Should().Be("BUTTON");
        toggle.GetAttribute("type").Should().Be("button");
        toggle.GetAttribute("aria-label").Should().Be("Toggle navigation");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Sync_AriaExpanded_With_NavOpen(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddContent(0, "n")));
        cut.Find(".bob-stacked-layout__toggle").GetAttribute("aria-expanded").Should().Be("false");

        // Act
        cut.Find(".bob-stacked-layout__toggle").Click();

        // Assert
        cut.Find(".bob-stacked-layout__toggle").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_Heading_Semantics_In_Header_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Header, b => b.AddMarkupContent(0, "<h1 class='brand'>Site</h1>")));

        // Assert
        cut.Find("header .bob-stacked-layout__header-content h1.brand").TextContent.Should().Be("Site");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Allow_AriaLabel_On_Nav_Via_Markup(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — consumer enriches the outer <nav> by placing an
        // inner labelled <nav> or relying on the existing landmark.
        IRenderedComponent<BOBStackedLayout> cut = ctx.Render<BOBStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddMarkupContent(0,
                "<ul aria-label='Primary'><li><a href='/'>Home</a></li></ul>")));

        // Assert
        cut.Find("nav.bob-stacked-layout__nav ul").GetAttribute("aria-label").Should().Be("Primary");
    }
}
