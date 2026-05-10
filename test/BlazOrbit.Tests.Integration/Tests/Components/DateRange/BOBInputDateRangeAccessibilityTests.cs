using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DateRange;

[Trait("Component Accessibility", "BOBInputDateRange")]
public class BOBInputDateRangeAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Summary_As_Live_Region(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        DateOnly s = new(2026, 1, 1);
        DateOnly e = new(2026, 1, 7);
        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.Value, new global::BlazOrbit.Components.DateRange(s, e)));

        cut.Find(".bob-daterange__summary").GetAttribute("aria-live").Should().Be("polite");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Pane_Titles(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(p => p
            .Add(c => c.StartLabel, "From")
            .Add(c => c.EndLabel, "To"));

        IReadOnlyList<IElement> titles = cut.FindAll(".bob-daterange__pane-title");
        titles[0].TextContent.Should().Be("From");
        titles[1].TextContent.Should().Be("To");
    }
}
