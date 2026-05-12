using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Variant", "BOBDataGrid.RowDetail")]
public class BOBDataGridRowDetailTests
{
    private sealed record Order(int Id, string Customer, decimal Total);

    private static IEnumerable<Order> Items => [new Order(1, "ACME", 120m), new Order(2, "Globex", 240m)];

    private static RenderFragment Columns => b =>
    {
        b.OpenComponent<BOBDataColumn<Order>>(0);
        b.AddAttribute(1, "Header", "Customer");
        b.AddAttribute(2, "Property",
            (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Customer));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Without_Detail_Template_Should_Not_Render_Expand_Cells(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Order>> cut = ctx.Render<BOBDataGrid<Order>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        cut.FindAll("td.bob-datagrid__expand-cell").Should().BeEmpty();
        cut.FindAll("th.bob-datagrid__expand-cell").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task With_Detail_Template_Should_Render_Expand_Toggle_Per_Row(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Order>> cut = ctx.Render<BOBDataGrid<Order>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns)
            .Add(c => c.RowDetailTemplate,
                (RenderFragment<Order>)(o => b => b.AddContent(0, $"Detail for {o.Customer}"))));

        cut.FindAll("th.bob-datagrid__expand-cell").Should().HaveCount(1);
        cut.FindAll("td.bob-datagrid__expand-cell").Should().HaveCount(2);
        // Detail rows are not rendered until expanded.
        cut.FindAll("tr.bob-datagrid__detail-row").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Click_Expand_Should_Show_Detail_Row(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Order>> cut = ctx.Render<BOBDataGrid<Order>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns)
            .Add(c => c.RowDetailTemplate,
                (RenderFragment<Order>)(o => b => b.AddContent(0, $"Detail for {o.Customer}"))));

        // Expand first row.
        cut.FindAll("td.bob-datagrid__expand-cell button").First().Click();

        IReadOnlyList<AngleSharp.Dom.IElement> detailRows = cut.FindAll("tr.bob-datagrid__detail-row");
        detailRows.Should().HaveCount(1);
        detailRows[0].TextContent.Should().Contain("Detail for ACME");

        // Toggle off.
        cut.FindAll("td.bob-datagrid__expand-cell button").First().Click();
        cut.FindAll("tr.bob-datagrid__detail-row").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Detail_Row_Should_Span_All_Columns(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Order>> cut = ctx.Render<BOBDataGrid<Order>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns)
            .Add(c => c.SelectionMode, SelectionMode.Multiple)
            .Add(c => c.RowDetailTemplate,
                (RenderFragment<Order>)(o => b => b.AddContent(0, "x"))));

        cut.FindAll("td.bob-datagrid__expand-cell button").First().Click();

        // 1 column + selection cell + expand cell = 3 columns total.
        AngleSharp.Dom.IElement detailCell = cut.Find("td.bob-datagrid__detail-cell");
        detailCell.GetAttribute("colspan").Should().Be("3");
    }
}