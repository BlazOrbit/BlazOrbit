using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Variant", "BOBDataGrid.AggregateFooter")]
public class BOBDataGridAggregateTests
{
    private sealed record Order(string Region, decimal Amount, int Quantity);

    private static IEnumerable<Order> Orders =>
    [
        new Order("EMEA", 120m, 3), new Order("Americas", 240m, 5), new Order("APAC", 180m, 4),
        new Order("EMEA", 60m, 2)
    ];

    private static RenderFragment ColumnsWithAggregates(
        AggregateFunction amountAgg = AggregateFunction.Sum,
        AggregateFunction qtyAgg = AggregateFunction.Average,
        string? amountFormat = "C0",
        string? amountLabel = null) => b =>
    {
        b.OpenComponent<BOBDataColumn<Order>>(0);
        b.AddAttribute(1, "Header", "Region");
        b.AddAttribute(2, "Property",
            (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Region));
        b.AddAttribute(3, "Aggregate", AggregateFunction.Count);
        b.AddAttribute(4, "AggregateLabel", "Total: ");
        b.CloseComponent();

        b.OpenComponent<BOBDataColumn<Order>>(10);
        b.AddAttribute(11, "Header", "Amount");
        b.AddAttribute(12, "Property",
            (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Amount));
        b.AddAttribute(13, "Aggregate", amountAgg);
        b.AddAttribute(14, "AggregateFormat", amountFormat);
        if (amountLabel is not null)
        {
            b.AddAttribute(15, "AggregateLabel", amountLabel);
        }

        b.CloseComponent();

        b.OpenComponent<BOBDataColumn<Order>>(20);
        b.AddAttribute(21, "Header", "Quantity");
        b.AddAttribute(22, "Property",
            (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Quantity));
        b.AddAttribute(23, "Aggregate", qtyAgg);
        b.AddAttribute(24, "AggregateFormat", "F1");
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task No_Aggregates_Should_Hide_Footer_Row(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Order>> cut = ctx.Render<BOBDataGrid<Order>>(p => p
            .Add(c => c.Items, Orders)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Order>>(0);
                b.AddAttribute(1, "Header", "Region");
                b.AddAttribute(2, "Property",
                    (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Region));
                b.CloseComponent();
            }));

        cut.FindAll("tfoot.bob-datagrid__footer--aggregate").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Sum_Aggregate_Should_Render_Total(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Order>> cut = ctx.Render<BOBDataGrid<Order>>(p => p
            .Add(c => c.Items, Orders)
            .Add(c => c.Columns, ColumnsWithAggregates(amountLabel: "Total: ")));

        IReadOnlyList<AngleSharp.Dom.IElement> cells =
            cut.FindAll("tfoot.bob-datagrid__footer--aggregate td.bob-datagrid__aggregate-cell");
        cells.Should().HaveCount(3);
        // Region: COUNT = 4 with "Total: " prefix
        cells[0].TextContent.Trim().Should().Be("Total: 4");
        // Amount: SUM = 600 currency, 0 decimals
        cells[1].TextContent.Trim().Should().Contain("Total: ").And.Contain("600");
        // Quantity: AVG = 3.5 (3+5+4+2 = 14 / 4)
        cells[2].TextContent.Trim().Should().Be("3.5");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Aggregate_Should_Reflect_Filtered_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Order>> cut = ctx.Render<BOBDataGrid<Order>>(p => p
            .Add(c => c.Items, Orders)
            .Add(c => c.Filterable, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Order>>(0);
                b.AddAttribute(1, "Header", "Region");
                b.AddAttribute(2, "Property",
                    (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Region));
                b.AddAttribute(3, "Filterable", true);
                b.CloseComponent();
                b.OpenComponent<BOBDataColumn<Order>>(10);
                b.AddAttribute(11, "Header", "Amount");
                b.AddAttribute(12, "Property",
                    (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Amount));
                b.AddAttribute(13, "Aggregate", AggregateFunction.Sum);
                b.AddAttribute(14, "AggregateFormat", "0");
                b.CloseComponent();
            }));

        // Sum all 4 → 600.
        cut.FindAll("td.bob-datagrid__aggregate-cell").Last().TextContent.Trim().Should().Be("600");

        // Filter "EMEA" → only 2 orders, sum = 180.
        await cut.InvokeAsync(() => cut.Instance.Filter("EMEA"));
        cut.FindAll("td.bob-datagrid__aggregate-cell").Last().TextContent.Trim().Should().Be("180");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Custom_Aggregate_Should_Use_Delegate(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Order>> cut = ctx.Render<BOBDataGrid<Order>>(p => p
            .Add(c => c.Items, Orders)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Order>>(0);
                b.AddAttribute(1, "Header", "Region");
                b.AddAttribute(2, "Property",
                    (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Region));
                b.AddAttribute(3, "Aggregate", AggregateFunction.Custom);
                b.AddAttribute(4, "CustomAggregate",
                    (Func<IEnumerable<Order>, object?>)(items =>
                        items.Select(o => o.Region).Distinct().Count() + " regions"));
                b.CloseComponent();
            }));

        cut.Find("td.bob-datagrid__aggregate-cell").TextContent.Trim().Should().Be("3 regions");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Min_Max_Should_Pick_Edge_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Order>> cut = ctx.Render<BOBDataGrid<Order>>(p => p
            .Add(c => c.Items, Orders)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Order>>(0);
                b.AddAttribute(1, "Header", "Min amt");
                b.AddAttribute(2, "Property",
                    (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Amount));
                b.AddAttribute(3, "Aggregate", AggregateFunction.Min);
                b.AddAttribute(4, "AggregateFormat", "0");
                b.CloseComponent();
                b.OpenComponent<BOBDataColumn<Order>>(10);
                b.AddAttribute(11, "Header", "Max amt");
                b.AddAttribute(12, "Property",
                    (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Amount));
                b.AddAttribute(13, "Aggregate", AggregateFunction.Max);
                b.AddAttribute(14, "AggregateFormat", "0");
                b.CloseComponent();
            }));

        IReadOnlyList<AngleSharp.Dom.IElement> cells =
            cut.FindAll("td.bob-datagrid__aggregate-cell");
        cells[0].TextContent.Trim().Should().Be("60");
        cells[1].TextContent.Trim().Should().Be("240");
    }
}