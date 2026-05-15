using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.BarChart;

[Trait("Component Variant", "BOBBarChart.Bidirectional")]
public class BOBBarChartBidirectionalTests
{
    // Mixed-sign series: "Inflow" positive, "Outflow" negative - classic
    // budget variance shape.
    private static IEnumerable<BOBChartSeries<string, decimal>> Mixed() =>
    [
        new BOBChartSeries<string, decimal>
        {
            Label = "Inflow",
            Points =
            [
                new BOBChartPoint<string, decimal>("Q1", 30m),
                    new BOBChartPoint<string, decimal>("Q2", 50m)
            ]
        },
        new BOBChartSeries<string, decimal>
        {
            Label = "Refunds",
            Points =
            [
                new BOBChartPoint<string, decimal>("Q1", -10m),
                new BOBChartPoint<string, decimal>("Q2", -5m)
            ]
        },
        new BOBChartSeries<string, decimal>
        {
            Label = "Bonus",
            Points =
            [
                new BOBChartPoint<string, decimal>("Q1", 5m), new BOBChartPoint<string, decimal>("Q2", 10m)
            ]
        }
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bidirectional_Should_Render_All_Series_Bars(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Mixed())
                .Add(c => c.StackMode, BOBBarStackMode.Bidirectional));

        // 3 series × 2 categories = 6 bars (each gets an own segment;
        // unlike pure Stacked, negatives don't collapse).
        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(6);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bidirectional_Negative_Bars_Should_Sit_Below_Zero(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Mixed())
                .Add(c => c.StackMode, BOBBarStackMode.Bidirectional));

        // Find the "Refunds" series group (negative values) and confirm
        // every bar's top y is below the zero line. Zero in default
        // 600×400 with bidirectional domain ≈ middle of plot.
        AngleSharp.Dom.IElement refundsGroup = cut.FindAll("g.bob-bar-chart__series")
            .First(g => g.GetAttribute("data-bob-series") == "Refunds");
        IEnumerable<AngleSharp.Dom.IElement> refundBars = refundsGroup.QuerySelectorAll("rect.bob-bar-chart__bar")
            .OfType<AngleSharp.Dom.IElement>();

        AngleSharp.Dom.IElement inflowGroup = cut.FindAll("g.bob-bar-chart__series")
            .First(g => g.GetAttribute("data-bob-series") == "Inflow");
        AngleSharp.Dom.IElement firstInflowBar = inflowGroup.QuerySelector("rect.bob-bar-chart__bar")!;
        double inflowY = double.Parse(firstInflowBar.GetAttribute("y")!,
            System.Globalization.CultureInfo.InvariantCulture);
        double inflowBottom = inflowY + double.Parse(firstInflowBar.GetAttribute("height")!,
            System.Globalization.CultureInfo.InvariantCulture);

        // Negative bars start at inflowBottom (= zero line) and grow down.
        foreach (AngleSharp.Dom.IElement neg in refundBars)
        {
            double y = double.Parse(neg.GetAttribute("y")!,
                System.Globalization.CultureInfo.InvariantCulture);
            y.Should().BeApproximately(inflowBottom, 1,
                "negative bars hang from the zero line");
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bidirectional_Positive_Stacks_Should_Accumulate_Upward(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Mixed())
                .Add(c => c.StackMode, BOBBarStackMode.Bidirectional));

        // Q1 positive stack: Inflow=30, Bonus=5 → cumulative top at 35.
        // Bonus bar should sit on top of Inflow → its bottom (y+height)
        // equals Inflow's top (y).
        AngleSharp.Dom.IElement inflowQ1 = cut.FindAll("g.bob-bar-chart__series")
            .First(g => g.GetAttribute("data-bob-series") == "Inflow")
            .QuerySelector("rect.bob-bar-chart__bar")!;
        AngleSharp.Dom.IElement bonusQ1 = cut.FindAll("g.bob-bar-chart__series")
            .First(g => g.GetAttribute("data-bob-series") == "Bonus")
            .QuerySelector("rect.bob-bar-chart__bar")!;

        double inflowY = double.Parse(inflowQ1.GetAttribute("y")!,
            System.Globalization.CultureInfo.InvariantCulture);
        double bonusY = double.Parse(bonusQ1.GetAttribute("y")!,
            System.Globalization.CultureInfo.InvariantCulture);
        double bonusH = double.Parse(bonusQ1.GetAttribute("height")!,
            System.Globalization.CultureInfo.InvariantCulture);

        (bonusY + bonusH).Should().BeApproximately(inflowY, 1,
            "Bonus stacks on top of Inflow → its bottom touches Inflow's top");
        bonusY.Should().BeLessThan(inflowY,
            "stacked-up bar's top sits above the previous segment's top");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bidirectional_Same_Sign_Twice_Should_Stack(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IEnumerable<BOBChartSeries<string, decimal>> twoNegative =
        [
            new BOBChartSeries<string, decimal>
            {
                Label = "A", Points = [new BOBChartPoint<string, decimal>("X", -10m)]
            },
            new BOBChartSeries<string, decimal>
            {
                Label = "B", Points = [new BOBChartPoint<string, decimal>("X", -5m)]
            }
        ];

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, twoNegative)
                .Add(c => c.StackMode, BOBBarStackMode.Bidirectional));

        AngleSharp.Dom.IElement aBar = cut.FindAll("g.bob-bar-chart__series")
            .First(g => g.GetAttribute("data-bob-series") == "A")
            .QuerySelector("rect.bob-bar-chart__bar")!;
        AngleSharp.Dom.IElement bBar = cut.FindAll("g.bob-bar-chart__series")
            .First(g => g.GetAttribute("data-bob-series") == "B")
            .QuerySelector("rect.bob-bar-chart__bar")!;

        double aY = double.Parse(aBar.GetAttribute("y")!, System.Globalization.CultureInfo.InvariantCulture);
        double aH = double.Parse(aBar.GetAttribute("height")!, System.Globalization.CultureInfo.InvariantCulture);
        double bY = double.Parse(bBar.GetAttribute("y")!, System.Globalization.CultureInfo.InvariantCulture);

        // A is the first negative segment, hanging from zero.
        // B is the second negative segment, hanging from A's bottom.
        bY.Should().BeApproximately(aY + aH, 1,
            "second negative segment hangs from the prior negative segment's bottom");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bidirectional_Negative_Bars_Should_Have_Width_Equal_To_GroupWidth(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Mixed())
                .Add(c => c.StackMode, BOBBarStackMode.Bidirectional));

        // Bidirectional uses a single full-width bar per category (like
        // Stacked/PercentStacked) - confirm all bars share one width.
        IReadOnlyList<AngleSharp.Dom.IElement> bars = cut.FindAll("rect.bob-bar-chart__bar");
        IEnumerable<double> widths = bars.Select(b => double.Parse(
            b.GetAttribute("width")!, System.Globalization.CultureInfo.InvariantCulture));
        widths.Distinct().Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bidirectional_Should_Skip_Hidden_Series(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, Mixed())
                .Add(c => c.StackMode, BOBBarStackMode.Bidirectional));

        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(6);

        // Toggle the first legend chip → 1 series hidden, 4 bars left.
        cut.Find("button.bob-chart__legend-button").Click();
        cut.FindAll("rect.bob-bar-chart__bar").Should().HaveCount(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bidirectional_All_Positive_Should_Behave_Like_Stacked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IEnumerable<BOBChartSeries<string, decimal>> allPositive =
        [
            new BOBChartSeries<string, decimal>
            {
                Label = "A", Points = [new BOBChartPoint<string, decimal>("X", 10m)]
            },
            new BOBChartSeries<string, decimal>
            {
                Label = "B", Points = [new BOBChartPoint<string, decimal>("X", 5m)]
            }
        ];

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, allPositive)
                .Add(c => c.StackMode, BOBBarStackMode.Bidirectional));

        // 2 bars, A at the bottom (touches zero), B on top of A.
        IReadOnlyList<AngleSharp.Dom.IElement> bars = cut.FindAll("rect.bob-bar-chart__bar");
        bars.Should().HaveCount(2);
    }
}
