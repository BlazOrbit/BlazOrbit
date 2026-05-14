using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Navigation;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Breadcrumbs;

[Trait("Component Rendering", "BOBBreadcrumbs")]
public class BOBBreadcrumbsRenderingTests
{
    private static readonly IReadOnlyList<BreadcrumbItem> SampleItems =
    [
        new("Home", "/"),
        new("Components", "/components"),
        new("Breadcrumbs")
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Each_Item_In_Order(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBreadcrumbs> cut = ctx.Render<BOBBreadcrumbs>(p => p
            .Add(c => c.Items, SampleItems));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("breadcrumbs");
        IReadOnlyList<IElement> entries = cut.FindAll(".bob-breadcrumbs__entry");
        entries.Count.Should().Be(SampleItems.Count);
        entries[0].TextContent.Trim().Should().Be("Home");
        entries[2].TextContent.Trim().Should().Be("Breadcrumbs");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Linked_Entries_As_Anchors(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBreadcrumbs> cut = ctx.Render<BOBBreadcrumbs>(p => p
            .Add(c => c.Items, SampleItems));

        cut.FindAll("a.bob-breadcrumbs__link").Count.Should().Be(2);
        cut.FindAll("a.bob-breadcrumbs__link")[0].GetAttribute("href").Should().Be("/");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Current_Entry_Without_Link(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBreadcrumbs> cut = ctx.Render<BOBBreadcrumbs>(p => p
            .Add(c => c.Items, SampleItems));

        IElement current = cut.FindAll(".bob-breadcrumbs__entry").Last();
        current.GetAttribute("data-bob-current").Should().Be("true");
        current.QuerySelector("a").Should().BeNull();
        current.QuerySelector("[aria-current='page']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Separator_Between_Entries(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBreadcrumbs> cut = ctx.Render<BOBBreadcrumbs>(p => p
            .Add(c => c.Items, SampleItems));

        // 3 entries -> 2 separators between them.
        cut.FindAll(".bob-breadcrumbs__separator").Count.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Separator(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBreadcrumbs> cut = ctx.Render<BOBBreadcrumbs>(p => p
            .Add(c => c.Items, SampleItems)
            .Add(c => c.Separator, b => b.AddMarkupContent(0, "<span class=\"slash\">/</span>")));

        cut.FindAll(".slash").Count.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Truncate_When_MaxVisible_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<BreadcrumbItem> longTrail =
        [
            new("Root", "/"),
            new("A", "/a"),
            new("B", "/a/b"),
            new("C", "/a/b/c"),
            new("D")
        ];

        IRenderedComponent<BOBBreadcrumbs> cut = ctx.Render<BOBBreadcrumbs>(p => p
            .Add(c => c.Items, longTrail)
            .Add(c => c.MaxVisible, 3));

        // Visible: Root, ellipsis, C, D -> 4 entries (3 real + 1 ellipsis tile).
        cut.FindAll(".bob-breadcrumbs__entry").Count.Should().Be(4);
        cut.FindAll(".bob-breadcrumbs__entry--ellipsis").Count.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_For_Empty_Items(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBreadcrumbs> cut = ctx.Render<BOBBreadcrumbs>(p => p
            .Add(c => c.Items, []));

        cut.FindAll(".bob-breadcrumbs__entry").Should().BeEmpty();
    }
}

[Trait("Component Accessibility", "BOBBreadcrumbs")]
public class BOBBreadcrumbsAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nav_With_Aria_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBreadcrumbs> cut = ctx.Render<BOBBreadcrumbs>(p => p
            .Add(c => c.Items, [new BreadcrumbItem("Home", "/"), new BreadcrumbItem("Now")])
            .Add(c => c.AriaLabel, "Sitio"));

        IElement nav = cut.Find("nav.bob-breadcrumbs");
        nav.GetAttribute("aria-label").Should().Be("Sitio");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Separators_As_Aria_Hidden(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBBreadcrumbs> cut = ctx.Render<BOBBreadcrumbs>(p => p
            .Add(c => c.Items, [new BreadcrumbItem("Home", "/"), new BreadcrumbItem("Now")]));

        IElement separator = cut.Find(".bob-breadcrumbs__separator");
        separator.GetAttribute("aria-hidden").Should().Be("true");
    }
}

[Trait("Component Snapshots", "BOBBreadcrumbs")]
public class BOBBreadcrumbsSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<BreadcrumbItem> basic =
        [
            new("Home", "/"),
            new("Components", "/components"),
            new("Breadcrumbs")
        ];

        IReadOnlyList<BreadcrumbItem> longTrail =
        [
            new("Root", "/"),
            new("A", "/a"),
            new("B", "/a/b"),
            new("C", "/a/b/c"),
            new("D")
        ];

        var testCases = new[]
        {
            new
            {
                Name = "Basic",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBBreadcrumbs>>)(p => p
                    .Add(c => c.Items, basic))
            },
            new
            {
                Name = "Truncated",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBBreadcrumbs>>)(p => p
                    .Add(c => c.Items, longTrail)
                    .Add(c => c.MaxVisible, 3))
            },
            new
            {
                Name = "WithIcons",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBBreadcrumbs>>)(p => p
                    .Add(c => c.Items,
                    [
                        new BreadcrumbItem("Home", "/", BOBIconKeys.MaterialIconsOutlined.i_home),
                            new BreadcrumbItem("Now")
                    ]))
            }
        };

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBBreadcrumbs> cut = ctx.Render<BOBBreadcrumbs>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}