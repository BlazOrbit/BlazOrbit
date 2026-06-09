using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.StatCard;

[Trait("Component Rendering", "BOBStatCard")]
public class BOBStatCardRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStatCard> cut = ctx.Render<BOBStatCard>(p => p
            .Add(c => c.Label, "Users")
            .Add(c => c.Value, "2,481"));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("stat-card");
        cut.Find(".bob-statcard__label").TextContent.Should().Be("Users");
        cut.Find(".bob-statcard__value").TextContent.Should().Contain("2,481");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Delta_With_Trend(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStatCard> cut = ctx.Render<BOBStatCard>(p => p
            .Add(c => c.Value, "1")
            .Add(c => c.Delta, "+12%")
            .Add(c => c.DeltaTrend, BOBStatTrend.Up));

        cut.Find(".bob-statcard__delta").GetAttribute("data-bob-trend").Should().Be("up");
        cut.Find(".bob-statcard__delta-value").TextContent.Should().Be("+12%");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Delta_Block_When_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStatCard> cut = ctx.Render<BOBStatCard>(p => p
            .Add(c => c.Value, "1"));

        cut.FindAll(".bob-statcard__delta").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Sparkline_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStatCard> cut = ctx.Render<BOBStatCard>(p => p
            .Add(c => c.Value, "1")
            .Add(c => c.Sparkline, b =>
            {
                b.OpenElement(0, "svg");
                b.AddAttribute(1, "class", "spark");
                b.CloseElement();
            }));

        cut.Find(".bob-statcard__sparkline svg.spark").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Footnote(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStatCard> cut = ctx.Render<BOBStatCard>(p => p
            .Add(c => c.Value, "1")
            .Add(c => c.Footnote, "Updated 5 min ago"));

        cut.Find(".bob-statcard__footnote").TextContent.Should().Contain("Updated 5 min ago");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Down_Trend_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStatCard> cut = ctx.Render<BOBStatCard>(p => p
            .Add(c => c.Value, "1")
            .Add(c => c.Delta, "-3")
            .Add(c => c.DeltaTrend, BOBStatTrend.Down));

        cut.Find(".bob-statcard__delta").GetAttribute("data-bob-trend").Should().Be("down");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Flat_Trend_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStatCard> cut = ctx.Render<BOBStatCard>(p => p
            .Add(c => c.Value, "1")
            .Add(c => c.Delta, "0%")
            .Add(c => c.DeltaTrend, BOBStatTrend.Flat));

        cut.Find(".bob-statcard__delta").GetAttribute("data-bob-trend").Should().Be("flat");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_DeltaCaption_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStatCard> cut = ctx.Render<BOBStatCard>(p => p
            .Add(c => c.Value, "1")
            .Add(c => c.Delta, "+5%")
            .Add(c => c.DeltaTrend, BOBStatTrend.Up)
            .Add(c => c.DeltaCaption, "vs last week"));

        cut.Find(".bob-statcard__delta-caption").TextContent.Should().Be("vs last week");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Prefer_ChildContent_Over_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBStatCard> cut = ctx.Render<BOBStatCard>(p => p
            .Add(c => c.Value, "should not appear")
            .Add(c => c.ChildContent, b => b.AddMarkupContent(0, "<strong>custom</strong>")));

        cut.Find(".bob-statcard__value").InnerHtml.Should().Contain("<strong>custom</strong>");
        cut.Find(".bob-statcard__value").TextContent.Should().NotContain("should not appear");
    }
}

[Trait("Component Snapshots", "BOBStatCard")]
public class BOBStatCardSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Plain",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBStatCard>>)(p => p
                    .Add(c => c.Label, "Users")
                    .Add(c => c.Value, "2,481"))
            },
            new
            {
                Name = "Up_Trend",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBStatCard>>)(p => p
                    .Add(c => c.Label, "Revenue")
                    .Add(c => c.Value, "$24,800")
                    .Add(c => c.Delta, "+12.4%")
                    .Add(c => c.DeltaCaption, "vs last week")
                    .Add(c => c.DeltaTrend, BOBStatTrend.Up))
            },
            new
            {
                Name = "Down_Trend",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBStatCard>>)(p => p
                    .Add(c => c.Label, "Errors")
                    .Add(c => c.Value, "3")
                    .Add(c => c.Delta, "-2")
                    .Add(c => c.DeltaTrend, BOBStatTrend.Down))
            },
            new
            {
                Name = "With_Footnote",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBStatCard>>)(p => p
                    .Add(c => c.Label, "Sessions")
                    .Add(c => c.Value, "9,128")
                    .Add(c => c.Footnote, "Updated 5 min ago"))
            }
        };

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBStatCard> cut = ctx.Render<BOBStatCard>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}
