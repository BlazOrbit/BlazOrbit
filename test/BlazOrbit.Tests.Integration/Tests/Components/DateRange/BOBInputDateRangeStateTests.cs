using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DateRange;

[Trait("Component State", "BOBInputDateRange")]
public class BOBInputDateRangeStateTests
{
    [Fact]
    public void DateRange_DayCount_Returns_Inclusive_Span()
    {
        DateOnly s = new(2026, 1, 1);
        DateOnly e = new(2026, 1, 7);
        new global::BlazOrbit.Components.DateRange(s, e).DayCount.Should().Be(7);
    }

    [Fact]
    public void DateRange_DayCount_Is_Zero_When_Open_Ended()
    {
        DateOnly s = new(2026, 1, 1);
        new global::BlazOrbit.Components.DateRange(s, null).DayCount.Should().Be(0);
        new global::BlazOrbit.Components.DateRange(null, null).DayCount.Should().Be(0);
    }

    [Fact]
    public void DateRange_Contains_Honours_Open_Ended_Bounds()
    {
        DateOnly start = new(2026, 1, 5);
        global::BlazOrbit.Components.DateRange openEnd = new(start, null);

        openEnd.Contains(new DateOnly(2026, 1, 4)).Should().BeFalse();
        openEnd.Contains(new DateOnly(2026, 1, 5)).Should().BeTrue();
        openEnd.Contains(new DateOnly(2030, 12, 31)).Should().BeTrue();
    }

    [Fact]
    public void DateRange_Last7Days_Spans_Seven_Days_Ending_Today()
    {
        global::BlazOrbit.Components.DateRange r = global::BlazOrbit.Components.DateRange.Last7Days();
        r.DayCount.Should().Be(7);
        r.End.Should().Be(DateOnly.FromDateTime(DateTime.Today));
    }

    [Fact]
    public void DateRange_ThisMonth_Spans_Calendar_Month()
    {
        global::BlazOrbit.Components.DateRange r = global::BlazOrbit.Components.DateRange.ThisMonth();
        r.Start!.Value.Day.Should().Be(1);
        r.Start.Value.Month.Should().Be(DateTime.Today.Month);
        r.End!.Value.AddDays(1).Day.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Summary_When_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>();

        cut.FindAll(".bob-daterange__summary").Should().BeEmpty();

        DateOnly s = new(2026, 1, 1);
        DateOnly e = new(2026, 1, 31);
        cut.Render(p => p.Add(c => c.Value, new global::BlazOrbit.Components.DateRange(s, e)));

        cut.Find(".bob-daterange__summary").TextContent.Should().Contain("31");
    }
}
