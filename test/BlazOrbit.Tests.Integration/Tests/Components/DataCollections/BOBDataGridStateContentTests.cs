using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component State", "BOBDataGrid.LoadingError")]
public class BOBDataGridStateContentTests
{
    private sealed record Item(string Name);

    private static RenderFragment Cols => b =>
    {
        b.OpenComponent<BOBDataColumn<Item>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Property",
            (System.Linq.Expressions.Expression<Func<Item, object?>>)(i => i.Name));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Loading_Spinner_Should_Render_When_No_LoadingContent(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Item>> cut = ctx.Render<BOBDataGrid<Item>>(p => p
            .Add(c => c.Items, Array.Empty<Item>())
            .Add(c => c.Columns, Cols)
            .Add(c => c.Loading, true));

        cut.FindAll(".bob-dc__spinner").Should().HaveCount(1);
        cut.FindAll("table.bob-datagrid__table").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Loading_Skeleton_Should_Render_Animated_Rows(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Item>> cut = ctx.Render<BOBDataGrid<Item>>(p => p
            .Add(c => c.Items, Array.Empty<Item>())
            .Add(c => c.Columns, Cols)
            .Add(c => c.Loading, true)
            .Add(c => c.LoadingMode, LoadingMode.Skeleton)
            .Add(c => c.PageSize, 4));

        cut.FindAll(".bob-dc__skeleton-row").Should().HaveCount(4);
        cut.FindAll(".bob-dc__skeleton-cell").Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Custom_LoadingContent_Should_Win_Over_Skeleton(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Item>> cut = ctx.Render<BOBDataGrid<Item>>(p => p
            .Add(c => c.Items, Array.Empty<Item>())
            .Add(c => c.Columns, Cols)
            .Add(c => c.Loading, true)
            .Add(c => c.LoadingMode, LoadingMode.Skeleton)
            .Add(c => c.LoadingContent,
                (RenderFragment)(b => b.AddMarkupContent(0, "<div class=\"my-loader\">Loading…</div>"))));

        cut.FindAll(".my-loader").Should().HaveCount(1);
        cut.FindAll(".bob-dc__skeleton").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Error_Should_Render_Default_Alert(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Item>> cut = ctx.Render<BOBDataGrid<Item>>(p => p
            .Add(c => c.Items, Array.Empty<Item>())
            .Add(c => c.Columns, Cols)
            .Add(c => c.Error, "Something went wrong"));

        AngleSharp.Dom.IElement alert = cut.Find(".bob-dc__error");
        alert.GetAttribute("role").Should().Be("alert");
        alert.TextContent.Should().Contain("Something went wrong");
        cut.FindAll("table.bob-datagrid__table").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Custom_ErrorContent_Should_Win_Over_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Item>> cut = ctx.Render<BOBDataGrid<Item>>(p => p
            .Add(c => c.Items, Array.Empty<Item>())
            .Add(c => c.Columns, Cols)
            .Add(c => c.Error, "x")
            .Add(c => c.ErrorContent,
                (RenderFragment)(b => b.AddMarkupContent(0, "<div class=\"my-error\">Custom error</div>"))));

        cut.FindAll(".my-error").Should().HaveCount(1);
        cut.FindAll(".bob-dc__error-text").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Empty_Action_Template_Should_Render_Inside_Empty_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Item>> cut = ctx.Render<BOBDataGrid<Item>>(p => p
            .Add(c => c.Items, Array.Empty<Item>())
            .Add(c => c.Columns, Cols)
            .Add(c => c.EmptyActionTemplate,
                (RenderFragment)(b => b.AddMarkupContent(0, "<button class=\"my-cta\">Create first</button>"))));

        cut.FindAll(".bob-dc__empty .my-cta").Should().HaveCount(1);
    }
}
