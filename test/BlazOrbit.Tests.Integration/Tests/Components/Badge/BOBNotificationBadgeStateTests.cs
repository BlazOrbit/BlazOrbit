using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Badge;

[Trait("Component State", "BOBNotificationBadge")]
public class BOBNotificationBadgeStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Badge_Content_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.BadgeContent, b => b.AddContent(0, "1")));

        cut.Find("span.bob-badge").TextContent.Should().Contain("1");

        // Act
        cut.Render(p => p.Add(c => c.BadgeContent, b => b.AddContent(0, "99+")));

        // Assert
        cut.Find("span.bob-badge").TextContent.Should().Contain("99+");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Size_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.Size, BOBSize.Small));

        cut.Find(".bob-notification-badge__indicator bob-component")
            .GetAttribute("data-bob-size").Should().Be("small");

        // Act
        cut.Render(p => p.Add(c => c.Size, BOBSize.Large));

        // Assert
        cut.Find(".bob-notification-badge__indicator bob-component")
            .GetAttribute("data-bob-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Position_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.Position, BadgePosition.TopRight));

        cut.Find("bob-component").GetAttribute("data-bob-position").Should().Be("topright");

        // Act
        cut.Render(p => p.Add(c => c.Position, BadgePosition.BottomLeft));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-position").Should().Be("bottomleft");
    }
}
