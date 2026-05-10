using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

[Trait("Component Interaction", "Charts.Streaming")]
public class StreamingTests
{
    private static IEnumerable<BOBChartSeries<int, double>> SeedSeries(int count = 3) => new[]
    {
        new BOBChartSeries<int, double>
        {
            Label = "S",
            Points = Enumerable.Range(1, count)
                .Select(i => new BOBChartPoint<int, double>(i, i * 1.0))
                .ToArray()
        }
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Streaming_Defaults_Should_Be_Off(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p.Add(c => c.Series, SeedSeries()));

        cut.Instance.StreamingWindow.Should().BeNull();
        cut.Instance.StreamingPaused.Should().BeFalse();
        cut.Instance.StreamingFollow.Should().BeTrue();
        cut.Instance.StreamingThrottle.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task AppendPointsAsync_Should_Add_Markers(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p.Add(c => c.Series, SeedSeries(3)));

        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(3);

        await cut.InvokeAsync(() => cut.Instance.AppendPointsAsync("S",
            new BOBChartPoint<int, double>(4, 4.0),
            new BOBChartPoint<int, double>(5, 5.0)));

        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(5);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task AppendPointsAsync_Empty_Batch_Should_NoOp(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool fired = false;
        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, SeedSeries(3))
                .Add(c => c.OnStreamUpdate, _ => fired = true));

        await cut.InvokeAsync(() => cut.Instance.AppendPointsAsync("S",
            Array.Empty<BOBChartPoint<int, double>>()));

        fired.Should().BeFalse();
        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task StreamingWindow_Should_Drop_Oldest_FIFO(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, SeedSeries(5))
                .Add(c => c.StreamingWindow, 4));

        // Append 3 → buffer has 5+3 = 8, window 4 keeps last 4 → points 6,7,8.
        await cut.InvokeAsync(() => cut.Instance.AppendPointsAsync("S",
            new BOBChartPoint<int, double>(6, 6.0),
            new BOBChartPoint<int, double>(7, 7.0),
            new BOBChartPoint<int, double>(8, 8.0)));

        IReadOnlyList<AngleSharp.Dom.IElement> markers = cut.FindAll("circle.bob-line-chart__marker");
        markers.Should().HaveCount(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task AppendPointsAsync_Should_Fire_OnStreamUpdate(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBChartStreamArgs<int, double>? captured = null;

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, SeedSeries(3))
                .Add(c => c.StreamingWindow, 4)
                .Add(c => c.OnStreamUpdate, args => captured = args));

        await cut.InvokeAsync(() => cut.Instance.AppendPointsAsync("S",
            new BOBChartPoint<int, double>(4, 4.0),
            new BOBChartPoint<int, double>(5, 5.0),
            new BOBChartPoint<int, double>(6, 6.0)));

        captured.Should().NotBeNull();
        captured!.SeriesLabel.Should().Be("S");
        captured.AppendedCount.Should().Be(3);
        captured.TotalCount.Should().Be(4, because: "window trimmed 6 down to 4");
        captured.DroppedByWindow.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ResetSeriesAsync_Should_Replace_Buffer(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p.Add(c => c.Series, SeedSeries(5)));

        await cut.InvokeAsync(() => cut.Instance.ResetSeriesAsync("S", new[]
        {
            new BOBChartPoint<int, double>(100, 1.0),
            new BOBChartPoint<int, double>(101, 2.0),
        }));

        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ClearStreamingAsync_Should_Revert_To_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p.Add(c => c.Series, SeedSeries(3)));

        await cut.InvokeAsync(() => cut.Instance.AppendPointsAsync("S",
            new BOBChartPoint<int, double>(4, 4.0),
            new BOBChartPoint<int, double>(5, 5.0)));
        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(5);

        await cut.InvokeAsync(() => cut.Instance.ClearStreamingAsync());
        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(3,
            because: "after clear, render falls back to the Series parameter");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task StreamingPaused_Should_Buffer_But_Not_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, SeedSeries(3))
                .Add(c => c.StreamingPaused, true));

        await cut.InvokeAsync(() => cut.Instance.AppendPointsAsync("S",
            new BOBChartPoint<int, double>(4, 4.0),
            new BOBChartPoint<int, double>(5, 5.0)));

        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(3,
            because: "paused stream buffers writes silently");

        // Un-pause + tiny additional append flushes the held state.
        cut.Render(p => p.Add(c => c.StreamingPaused, false));
        await cut.InvokeAsync(() => cut.Instance.AppendPointsAsync("S",
            new BOBChartPoint<int, double>(6, 6.0)));

        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(6);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task AppendPointsAsync_Bulk_Enumerable_Should_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p.Add(c => c.Series, SeedSeries(2)));

        IEnumerable<BOBChartPoint<int, double>> bulk = Enumerable.Range(3, 50)
            .Select(i => new BOBChartPoint<int, double>(i, i * 1.0));

        await cut.InvokeAsync(() => cut.Instance.AppendPointsAsync("S", bulk));

        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(52);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task AppendPointsAsync_Should_Seed_Empty_Series(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, new[]
                {
                    new BOBChartSeries<int, double>
                    {
                        Label = "T",
                        Points = Array.Empty<BOBChartPoint<int, double>>()
                    }
                }));

        await cut.InvokeAsync(() => cut.Instance.AppendPointsAsync("T",
            new BOBChartPoint<int, double>(1, 1.0),
            new BOBChartPoint<int, double>(2, 2.0)));

        cut.FindAll("circle.bob-line-chart__marker").Should().HaveCount(2);
    }
}
