using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Badge;

[Trait("Component Snapshots", "BOBBadge")]
public class BOBBadgeSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Dot_Mode",
                Html = ctx.Render<BOBBadge>().GetNormalizedMarkup()
            },
            new
            {
                Name = "With_Content",
                Html = ctx.Render<BOBBadge>(p => p
                    .Add(c => c.ChildContent, b => b.AddContent(0, "5"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Circular_With_Content",
                Html = ctx.Render<BOBBadge>(p => p
                    .Add(c => c.Circular, true)
                    .Add(c => c.ChildContent, b => b.AddContent(0, "99"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Large_Size",
                Html = ctx.Render<BOBBadge>(p => p
                    .Add(c => c.Size, BOBSize.Large)
                    .Add(c => c.ChildContent, b => b.AddContent(0, "New"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Notification_Badge_TopRight",
                Html = ctx.Render<BOBNotificationBadge>(p => p
                    .Add(c => c.Position, BadgePosition.TopRight)
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "3"))).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
