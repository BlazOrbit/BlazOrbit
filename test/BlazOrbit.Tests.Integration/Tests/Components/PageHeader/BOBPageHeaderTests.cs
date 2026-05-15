using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.PageHeader;

[Trait("Component Rendering", "BOBPageHeader")]
public class BOBPageHeaderRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Title_As_H1(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPageHeader> cut = ctx.Render<BOBPageHeader>(p => p
            .Add(c => c.Title, "Dashboard"));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("page-header");
        cut.Find("h1.bob-page-header__title").TextContent.Should().Be("Dashboard");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Eyebrow_When_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPageHeader> cut = ctx.Render<BOBPageHeader>(p => p
            .Add(c => c.Title, "Users")
            .Add(c => c.Eyebrow, (RenderFragment)(b => b.AddMarkupContent(0, "ops"))));

        cut.Find(".bob-page-header__eyebrow").TextContent.Should().Be("ops");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Actions_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPageHeader> cut = ctx.Render<BOBPageHeader>(p => p
            .Add(c => c.Title, "Users")
            .Add(c => c.Actions, (RenderFragment)(b => b.AddMarkupContent(0, "<button>Refresh</button>"))));

        cut.Find(".bob-page-header__actions button").TextContent.Should().Be("Refresh");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Sticky_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPageHeader> cut = ctx.Render<BOBPageHeader>(p => p
            .Add(c => c.Title, "x")
            .Add(c => c.Sticky, true));

        cut.Find("bob-component").GetAttribute("data-bob-sticky").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Title_When_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPageHeader> cut = ctx.Render<BOBPageHeader>(p => p
            .Add(c => c.Lead, (RenderFragment)(b => b.AddMarkupContent(0, "lead-only"))));

        cut.FindAll("h1.bob-page-header__title").Should().BeEmpty();
        cut.Find(".bob-page-header__lead").TextContent.Should().Be("lead-only");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Breadcrumbs_Slot_Above_Title(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPageHeader> cut = ctx.Render<BOBPageHeader>(p => p
            .Add(c => c.Title, "Orders")
            .Add(c => c.Breadcrumbs, (RenderFragment)(b => b.AddMarkupContent(0, "<nav>Home / Orders</nav>"))));

        cut.Find(".bob-page-header__breadcrumbs").TextContent.Should().Contain("Home / Orders");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Omit_Sticky_Attribute_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPageHeader> cut = ctx.Render<BOBPageHeader>(p => p
            .Add(c => c.Title, "Static"));

        cut.Find("bob-component").HasAttribute("data-bob-sticky").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Eyebrow_Before_Title_In_Markup_Order(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPageHeader> cut = ctx.Render<BOBPageHeader>(p => p
            .Add(c => c.Title, "Title")
            .Add(c => c.Eyebrow, (RenderFragment)(b => b.AddMarkupContent(0, "context"))));

        string markup = cut.Markup;
        int eyebrowIdx = markup.IndexOf("page-header__eyebrow", StringComparison.Ordinal);
        int titleIdx = markup.IndexOf("page-header__title", StringComparison.Ordinal);
        eyebrowIdx.Should().BeGreaterThanOrEqualTo(0);
        titleIdx.Should().BeGreaterThan(eyebrowIdx);
    }
}

[Trait("Component Accessibility", "BOBPageHeader")]
public class BOBPageHeaderAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Actions_With_Toolbar_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBPageHeader> cut = ctx.Render<BOBPageHeader>(p => p
            .Add(c => c.Title, "x")
            .Add(c => c.Actions, (RenderFragment)(b => b.AddMarkupContent(0, "<span>a</span>"))));

        cut.Find(".bob-page-header__actions").GetAttribute("role").Should().Be("toolbar");
    }
}

[Trait("Component Snapshots", "BOBPageHeader")]
public class BOBPageHeaderSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "TitleOnly",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBPageHeader>>)(p => p
                    .Add(c => c.Title, "Dashboard"))
            },
            new
            {
                Name = "TitleEyebrowLead",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBPageHeader>>)(p => p
                    .Add(c => c.Title, "Users")
                    .Add(c => c.Eyebrow, (RenderFragment)(b => b.AddMarkupContent(0, "ops")))
                    .Add(c => c.Lead, (RenderFragment)(b => b.AddMarkupContent(0, "lead text"))))
            },
            new
            {
                Name = "TitleAndActions",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBPageHeader>>)(p => p
                    .Add(c => c.Title, "Orders")
                    .Add(c => c.Actions, (RenderFragment)(b => b.AddMarkupContent(0, "<button>Refresh</button>"))))
            }
        };

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBPageHeader> cut = ctx.Render<BOBPageHeader>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}
