using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Badge;

[Trait("Component Rendering", "BOBNotificationBadge")]
public class BOBNotificationBadgeRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("notification-badge");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Indicator_Div(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>();

        // Assert
        cut.Find(".bob-notification-badge__indicator").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Inner_BOBBadge(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.BadgeContent, b => b.AddContent(0, "5")));

        // Assert — inner BOBBadge renders a span.bob-badge
        cut.Find("span.bob-badge").TextContent.Should().Contain("5");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Position_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.Position, BadgePosition.BottomLeft));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-position").Should().Be("bottomleft");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ChildContent_As_Host_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenElement(0, "button");
                b.AddContent(1, "Notifications");
                b.CloseElement();
            })
            .Add(c => c.BadgeContent, b => b.AddContent(0, "3")));

        // Assert — host content renders
        cut.Find("button").TextContent.Should().Contain("Notifications");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Default_Dot_Mode_In_Indicator_When_No_Badge_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — no BadgeContent = dot mode in inner badge
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>();

        // Assert — inner badge has dot attribute
        IElement innerBadge = cut.Find(".bob-notification-badge__indicator bob-component");
        innerBadge.GetAttribute("data-bob-dot").Should().Be("true");
    }
}
