using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Badge;

[Trait("Component Accessibility", "BOBNotificationBadge")]
public class BOBNotificationBadgeAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Accept_Role_Status_Via_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — consumers project role="status" on the host so AT
        // announces count changes without overriding library-emitted data-bob-*.
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.BadgeContent, b => b.AddContent(0, "3"))
            .AddUnmatched("role", "status"));

        // Assert
        cut.Find("bob-component").GetAttribute("role").Should().Be("status");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Accept_Aria_Live_Via_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.BadgeContent, b => b.AddContent(0, "3"))
            .AddUnmatched("aria-live", "polite"));

        // Assert
        cut.Find("bob-component").GetAttribute("aria-live").Should().Be("polite");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Accept_Aria_Label_Describing_Count(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.BadgeContent, b => b.AddContent(0, "7"))
            .AddUnmatched("aria-label", "7 unread notifications"));

        // Assert
        cut.Find("bob-component").GetAttribute("aria-label").Should().Be("7 unread notifications");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Indicator_As_Non_Interactive_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.BadgeContent, b => b.AddContent(0, "5")));

        // Assert — indicator wrapper is a plain div and inner badge is a span,
        // so the badge itself does not steal keyboard focus from the host child.
        cut.Find(".bob-notification-badge__indicator").TagName.Should().Be("DIV");
        cut.FindAll(".bob-notification-badge__indicator button").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Keep_Host_ChildContent_Interactive(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — if the host is a button, it remains focusable alongside the badge.
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenElement(0, "button");
                b.AddAttribute(1, "type", "button");
                b.AddContent(2, "Open notifications");
                b.CloseElement();
            })
            .Add(c => c.BadgeContent, b => b.AddContent(0, "3")));

        // Assert
        cut.Find("button[type='button']").TextContent.Should().Contain("Open notifications");
    }
}
