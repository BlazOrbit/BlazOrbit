using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Badge;

[Trait("Component Snapshots", "BOBNotificationBadge")]
public class BOBNotificationBadgeSnapshotTests
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
                Name = "Default_Dot_TopRight",
                Html = ctx.Render<BOBNotificationBadge>().GetNormalizedMarkup()
            },
            new
            {
                Name = "With_Count_3_TopRight",
                Html = ctx.Render<BOBNotificationBadge>(p => p
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "3"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "With_Count_99_Plus",
                Html = ctx.Render<BOBNotificationBadge>(p => p
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "99+"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "BottomLeft_Position",
                Html = ctx.Render<BOBNotificationBadge>(p => p
                    .Add(c => c.Position, BadgePosition.BottomLeft)
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "1"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Large_Size",
                Html = ctx.Render<BOBNotificationBadge>(p => p
                    .Add(c => c.Size, BOBSize.Large)
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "5"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Non_Circular",
                Html = ctx.Render<BOBNotificationBadge>(p => p
                    .Add(c => c.Circular, false)
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "New"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "With_Host_Button",
                Html = ctx.Render<BOBNotificationBadge>(p => p
                    .Add(c => c.ChildContent, b =>
                    {
                        b.OpenElement(0, "button");
                        b.AddAttribute(1, "type", "button");
                        b.AddContent(2, "Inbox");
                        b.CloseElement();
                    })
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "3"))).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
