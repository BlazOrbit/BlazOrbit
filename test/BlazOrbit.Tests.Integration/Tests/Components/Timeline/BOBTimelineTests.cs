using BlazOrbit.Components;
using BlazOrbit.Components.Navigation;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Timeline;

[Trait("Component Rendering", "BOBTimeline")]
public class BOBTimelineRenderingTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 5, 1, 9, 0, 0, TimeSpan.Zero);

    private static readonly IReadOnlyList<TimelineItem> SampleItems =
    [
        new(Anchor, "Kickoff", "scope agreed"),
        new(Anchor.AddHours(2), "Build"),
        new(Anchor.AddHours(5), "Done"),
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Each_Item(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTimeline> cut = ctx.Render<BOBTimeline>(p => p
            .Add(c => c.Items, SampleItems));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("timeline");
        cut.FindAll(".bob-timeline__entry").Count.Should().Be(3);
        cut.Find(".bob-timeline__entry .bob-timeline__title").TextContent.Should().Be("Kickoff");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_Vertical(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTimeline> cut = ctx.Render<BOBTimeline>(p => p
            .Add(c => c.Items, SampleItems));

        cut.Find("bob-component").GetAttribute("data-bob-orientation").Should().Be("vertical");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Horizontal_Token(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTimeline> cut = ctx.Render<BOBTimeline>(p => p
            .Add(c => c.Items, SampleItems)
            .Add(c => c.Orientation, BOBTimelineOrientation.Horizontal));

        cut.Find("bob-component").GetAttribute("data-bob-orientation").Should().Be("horizontal");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Alternating_When_Vertical(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTimeline> cut = ctx.Render<BOBTimeline>(p => p
            .Add(c => c.Items, SampleItems)
            .Add(c => c.Alternating, true));

        cut.Find("bob-component").GetAttribute("data-bob-alternating").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Ignore_Alternating_When_Horizontal(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Alternating only makes sense on the vertical rail; the horizontal layout
        // suppresses the data-bob attribute so CSS selectors stay simple.
        IRenderedComponent<BOBTimeline> cut = ctx.Render<BOBTimeline>(p => p
            .Add(c => c.Items, SampleItems)
            .Add(c => c.Orientation, BOBTimelineOrientation.Horizontal)
            .Add(c => c.Alternating, true));

        cut.Find("bob-component").GetAttribute("data-bob-alternating").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Per_Day_Headers_When_GroupByDate(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<TimelineItem> twoDays =
        [
            new(Anchor, "Mon morning"),
            new(Anchor.AddHours(4), "Mon noon"),
            new(Anchor.AddDays(1), "Tue morning"),
        ];

        IRenderedComponent<BOBTimeline> cut = ctx.Render<BOBTimeline>(p => p
            .Add(c => c.Items, twoDays)
            .Add(c => c.GroupByDate, true));

        cut.FindAll(".bob-timeline__group-header").Count.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Item_Color_As_Inline_Var(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<TimelineItem> colored =
        [
            new(Anchor, "Marker", Color: "#ff00ff"),
        ];

        IRenderedComponent<BOBTimeline> cut = ctx.Render<BOBTimeline>(p => p
            .Add(c => c.Items, colored));

        cut.Find(".bob-timeline__entry").GetAttribute("style").Should().Contain("--bob-inline-color:#ff00ff");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_For_Empty_Items(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBTimeline> cut = ctx.Render<BOBTimeline>(p => p
            .Add(c => c.Items, []));

        cut.FindAll(".bob-timeline__entry").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_ItemTemplate_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Build a RenderFragment<TimelineItem> directly so bUnit's ParameterCollection
        // resolves the typed templated overload instead of the EventCallback one.
        Microsoft.AspNetCore.Components.RenderFragment<TimelineItem> template = item => b =>
        {
            b.OpenElement(0, "span");
            b.AddAttribute(1, "class", "custom");
            b.AddContent(2, item.Title);
            b.CloseElement();
        };

        IRenderedComponent<BOBTimeline> cut = ctx.Render<BOBTimeline>(p => p
            .Add(c => c.Items, SampleItems)
            .Add(c => c.ItemTemplate, template));

        cut.FindAll("span.custom").Count.Should().Be(3);
        cut.FindAll(".bob-timeline__title").Should().BeEmpty();
    }
}

[Trait("Component Snapshots", "BOBTimeline")]
public class BOBTimelineSnapshotTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 5, 1, 9, 0, 0, TimeSpan.Zero);

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<TimelineItem> items =
        [
            new(Anchor, "Kickoff", "scope agreed", BOBIconKeys.MaterialIconsOutlined.i_flag),
            new(Anchor.AddHours(4), "Build"),
            new(Anchor.AddDays(1), "Done", null, null, "#00aa00"),
        ];

        var testCases = new[]
        {
            new { Name = "Vertical", Builder = (Action<ComponentParameterCollectionBuilder<BOBTimeline>>)(p => p
                .Add(c => c.Items, items)) },
            new { Name = "Alternating", Builder = (Action<ComponentParameterCollectionBuilder<BOBTimeline>>)(p => p
                .Add(c => c.Items, items)
                .Add(c => c.Alternating, true)) },
            new { Name = "Horizontal", Builder = (Action<ComponentParameterCollectionBuilder<BOBTimeline>>)(p => p
                .Add(c => c.Items, items)
                .Add(c => c.Orientation, BOBTimelineOrientation.Horizontal)) },
            new { Name = "GroupByDate", Builder = (Action<ComponentParameterCollectionBuilder<BOBTimeline>>)(p => p
                .Add(c => c.Items, items)
                .Add(c => c.GroupByDate, true)) },
        };

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBTimeline> cut = ctx.Render<BOBTimeline>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
