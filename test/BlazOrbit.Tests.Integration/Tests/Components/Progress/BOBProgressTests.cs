using AngleSharp.Dom;
using BlazOrbit.Components.Display;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Progress;

[Trait("Component Rendering", "BOBProgressBar")]
public class BOBProgressBarRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressBar> cut = ctx.Render<BOBProgressBar>(p => p
            .Add(c => c.Value, 40));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("progress-bar");
        cut.Find(".bob-progress").GetAttribute("aria-valuenow").Should().Be("40");
        cut.Find(".bob-progress__fill").GetAttribute("style").Should().Contain("width: 40");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Indeterminate(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressBar> cut = ctx.Render<BOBProgressBar>(p => p
            .Add(c => c.Indeterminate, true));

        cut.Find(".bob-progress").GetAttribute("data-bob-indeterminate").Should().Be("true");
        cut.Find(".bob-progress").GetAttribute("aria-valuenow").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Segments(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressBar> cut = ctx.Render<BOBProgressBar>(p => p
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 5)
            .Add(c => c.Value, 3)
            .Add(c => c.Segments, 5));

        IReadOnlyList<IElement> segs = cut.FindAll(".bob-progress__segment");
        segs.Should().HaveCount(5);
        segs[0].GetAttribute("data-bob-filled").Should().Be("true");
        segs[2].GetAttribute("data-bob-filled").Should().Be("true");
        segs[3].GetAttribute("data-bob-filled").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Percentage_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressBar> cut = ctx.Render<BOBProgressBar>(p => p
            .Add(c => c.Value, 73)
            .Add(c => c.ShowPercentage, true));

        cut.Find(".bob-progress__pct").TextContent.Should().Be("73%");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_Value_Outside_Range(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressBar> over = ctx.Render<BOBProgressBar>(p => p
            .Add(c => c.Value, 200));
        over.Find(".bob-progress").GetAttribute("aria-valuenow").Should().Be("100");

        IRenderedComponent<BOBProgressBar> under = ctx.Render<BOBProgressBar>(p => p
            .Add(c => c.Value, -50));
        under.Find(".bob-progress").GetAttribute("aria-valuenow").Should().Be("0");
    }
}

[Trait("Component Rendering", "BOBProgressRing")]
public class BOBProgressRingRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>(p => p
            .Add(c => c.Value, 50));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("progress-ring");
        cut.Find(".bob-progress-ring").GetAttribute("aria-valuenow").Should().Be("50");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Percentage(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>(p => p
            .Add(c => c.Value, 80)
            .Add(c => c.ShowPercentage, true));

        cut.Find(".bob-progress-ring__label").TextContent.Should().Be("80%");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Indeterminate(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>(p => p
            .Add(c => c.Indeterminate, true));

        cut.Find(".bob-progress-ring").GetAttribute("data-bob-indeterminate").Should().Be("true");
    }
}

[Trait("Component Snapshots", "BOBProgress")]
public class BOBProgressSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var bars = new[]
        {
            new
            {
                Name = "Bar_50",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBProgressBar>>)(p => p
                    .Add(c => c.Value, 50)
                    .Add(c => c.Label, "Loading")
                    .Add(c => c.ShowPercentage, true))
            },
            new
            {
                Name = "Bar_Indeterminate",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBProgressBar>>)(p => p
                    .Add(c => c.Indeterminate, true))
            },
            new
            {
                Name = "Bar_Segments",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBProgressBar>>)(p => p
                    .Add(c => c.Min, 0)
                    .Add(c => c.Max, 4)
                    .Add(c => c.Value, 2)
                    .Add(c => c.Segments, 4))
            }
        };

        var rings = new[]
        {
            new
            {
                Name = "Ring_30",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBProgressRing>>)(p => p
                    .Add(c => c.Value, 30)
                    .Add(c => c.ShowPercentage, true))
            },
            new
            {
                Name = "Ring_Indeterminate",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBProgressRing>>)(p => p
                    .Add(c => c.Indeterminate, true))
            }
        };

        var results = bars.Select(tc =>
        {
            IRenderedComponent<BOBProgressBar> cut = ctx.Render<BOBProgressBar>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).Concat(rings.Select(tc =>
        {
            IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        })).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}