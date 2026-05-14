using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

[Trait("Component Variant", "Charts.AxisFormat")]
public class AxisFormatTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Integer_Domain_Should_Render_Without_Decimals(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, [
                    new BOBChartSeries<int, double>
                    {
                        Label = "S",
                        Points = Enumerable.Range(0, 5)
                            .Select(i => new BOBChartPoint<int, double>(i, i * 10.0))
                            .ToArray()
                    }
                ]));

        IEnumerable<string> labels = cut.FindAll("text").Select(t => t.TextContent);
        labels.Where(l => double.TryParse(l, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out _))
            .Should().NotContain(l => l.Contains('.'),
                "an integer-only domain should never produce decimal-suffixed labels");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Fractional_Domain_Should_Limit_Decimals(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Domain 0..1 with 5 ticks → step 0.2 → 1 decimal expected.
        IRenderedComponent<BOBLineChart<double, double>> cut =
            ctx.Render<BOBLineChart<double, double>>(p => p
                .Add(c => c.Series,
                [
                    new BOBChartSeries<double, double>
                        {
                            Label = "S",
                            Points =
                            [
                                new BOBChartPoint<double, double>(0.0, 0.1),
                                new BOBChartPoint<double, double>(0.5, 0.5),
                                new BOBChartPoint<double, double>(1.0, 1.0)
                            ]
                        }
                ]));

        // No label should have more than 4 chars after the decimal point —
        // protects against the previous "0.20000000000000001" output.
        IEnumerable<string> labels = cut.FindAll("text").Select(t => t.TextContent);
        foreach (string label in labels)
        {
            int dot = label.IndexOf('.');
            if (dot < 0)
            {
                continue;
            }

            (label.Length - dot - 1).Should().BeLessThan(5,
                $"label '{label}' should not show floating-point noise");
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Zoom_Should_Clip_Series_To_Plot_Rect(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, [
                    new BOBChartSeries<int, double>
                    {
                        Label = "S",
                        Points = Enumerable.Range(1, 20)
                            .Select(i => new BOBChartPoint<int, double>(i, i * 1.0))
                            .ToArray()
                    }
                ])
                .Add(c => c.ZoomEnabled, true));

        // <defs><clipPath id="..."><rect …/></clipPath></defs> should exist.
        cut.FindAll("clipPath").Should().HaveCount(1);
        cut.FindAll("clipPath rect").Should().HaveCount(1);

        // The series group should reference the clip-path.
        AngleSharp.Dom.IElement seriesGroup = cut.Find("g.bob-line-chart__series");
        string? clip = seriesGroup.GetAttribute("clip-path");
        clip.Should().NotBeNull().And.StartWith("url(#bob-line-clip-");
    }
}