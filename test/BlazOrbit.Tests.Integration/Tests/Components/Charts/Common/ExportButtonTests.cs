using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

/// <summary>
/// Cross-chart tests for the optional Export-PNG button on
/// <see cref="BOBChartBase{TX, TY}"/>. Exercises rendering presence/absence
/// and the host-attribute marker; the actual SVG → PNG rasterization runs
/// in JS interop and is exercised by the templates E2E suite.
/// </summary>
[Trait("Component Integration", "Charts.ExportButton")]
public class ExportButtonTests
{
    private static IEnumerable<BOBChartSeries<string, decimal>> SampleBarSeries() =>
    [
        new BOBChartSeries<string, decimal>
        {
            Label = "Sales", Points = [new BOBChartPoint<string, decimal>("Q1", 100m)]
        }
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Button_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, SampleBarSeries()));

        cut.FindAll("button.bob-chart__export-button").Should().BeEmpty();
        cut.Find("bob-component").HasAttribute("data-bob-has-export").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Button_When_ShowExportButton_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, SampleBarSeries())
                .Add(c => c.ShowExportButton, true));

        AngleSharp.Dom.IElement button = cut.Find("button.bob-chart__export-button");
        button.GetAttribute("type").Should().Be("button");
        button.GetAttribute("aria-label").Should().Be("Export chart as PNG");
        button.GetAttribute("title").Should().Be("Export PNG");

        // Host attribute exposed for CSS hooks (positioning, theming).
        cut.Find("bob-component").HasAttribute("data-bob-has-export").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Button_For_LineChart(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series,
                [
                    new BOBChartSeries<int, double>
                        {
                            Label = "T", Points = [new BOBChartPoint<int, double>(1, 1.0)]
                        }
                ])
                .Add(c => c.ShowExportButton, true));

        cut.FindAll("button.bob-chart__export-button").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Button_For_PieChart(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPieChart<decimal>> cut =
            ctx.Render<BOBPieChart<decimal>>(p => p
                .Add(c => c.Slices,
                [
                    new BOBChartSlice<decimal> { Label = "A", Value = 30m },
                        new BOBChartSlice<decimal> { Label = "B", Value = 70m }
                ])
                .Add(c => c.ShowExportButton, true));

        cut.FindAll("button.bob-chart__export-button").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_ExportFileName_To_chart(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBarChart<string, decimal>> cut =
            ctx.Render<BOBBarChart<string, decimal>>(p => p
                .Add(c => c.Series, SampleBarSeries()));

        cut.Instance.ExportFileName.Should().Be("chart");
    }
}
