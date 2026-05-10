using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Variant", "BOBDataGrid.RowActions")]
public class BOBDataGridActionsTests
{
    private sealed record Person(string Name, int Age, bool ReadOnly = false);

    private static IEnumerable<Person> Items => new[]
    {
        new Person("Alice", 30),
        new Person("Bob", 25, ReadOnly: true),
        new Person("Carol", 35),
    };

    private static RenderFragment SimpleColumns => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Property",
            (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.Name));
        b.CloseComponent();
        b.OpenComponent<BOBDataColumn<Person>>(10);
        b.AddAttribute(11, "Header", "Age");
        b.AddAttribute(12, "Property",
            (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.Age));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Without_Actions_Should_Not_Render_Action_Column(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, SimpleColumns));

        cut.FindAll("td.bob-datagrid__actions-cell").Should().BeEmpty();
        cut.FindAll("th.bob-datagrid__actions-cell").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Row_Actions_Should_Render_Per_Row(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, SimpleColumns)
            .Add(c => c.RowActions, new[]
            {
                new DataCollectionRowAction<Person>
                {
                    Label = "Edit",
                    Icon = BOBIconKeys.MaterialIconsOutlined.i_edit,
                    OnClick = _ => Task.CompletedTask,
                },
                new DataCollectionRowAction<Person>
                {
                    Label = "Delete",
                    Icon = BOBIconKeys.MaterialIconsOutlined.i_delete,
                    Color = PaletteColor.Error,
                    OnClick = _ => Task.CompletedTask,
                },
            }));

        // 3 rows × 2 actions each = 6 buttons, plus 1 header action cell.
        cut.FindAll("th.bob-datagrid__actions-cell").Should().HaveCount(1);
        cut.FindAll("td.bob-datagrid__actions-cell").Should().HaveCount(3);
        cut.FindAll("td.bob-datagrid__actions-cell button").Should().HaveCount(6);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Row_Action_Visible_Predicate_Should_Hide_Per_Row(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, SimpleColumns)
            .Add(c => c.RowActions, new[]
            {
                new DataCollectionRowAction<Person>
                {
                    Label = "Delete",
                    Visible = item => !item.ReadOnly,
                    OnClick = _ => Task.CompletedTask,
                },
            }));

        // 3 rows but Bob is ReadOnly → only 2 buttons.
        cut.FindAll("td.bob-datagrid__actions-cell button").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Row_Action_Click_Should_Invoke_Handler(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        Person? captured = null;

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, SimpleColumns)
            .Add(c => c.RowActions, new[]
            {
                new DataCollectionRowAction<Person>
                {
                    Label = "Edit",
                    OnClick = item => { captured = item; return Task.CompletedTask; },
                },
            }));

        cut.FindAll("td.bob-datagrid__actions-cell button").First().Click();

        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bulk_Actions_Should_Render_When_Selected(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IReadOnlyCollection<Person>? captured = null;

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, SimpleColumns)
            .Add(c => c.SelectionMode, SelectionMode.Multiple)
            .Add(c => c.BulkActions, new[]
            {
                new DataCollectionBulkAction<Person>
                {
                    Label = "Delete selected",
                    Icon = BOBIconKeys.MaterialIconsOutlined.i_delete,
                    Color = PaletteColor.Error,
                    OnClick = sel => { captured = sel; return Task.CompletedTask; },
                },
            }));

        // Toolbar empty before selection.
        cut.FindAll(".bob-dc__selection-info button").Should().HaveCount(0);

        // Select first row's checkbox.
        await cut.InvokeAsync(() =>
            cut.FindAll("td.bob-datagrid__select-cell").First().Click());

        // Now toolbar shows: "Clear" + the bulk action button = 2.
        cut.FindAll(".bob-dc__selection-info button").Count.Should().BeGreaterThanOrEqualTo(2);

        // Click "Delete selected" — last button in the selection info.
        cut.FindAll(".bob-dc__selection-info button").Last().Click();

        captured.Should().NotBeNull();
        captured!.Should().HaveCount(1);
    }
}
