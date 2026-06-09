using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Variant", "BOBDataGrid.MultiSort")]
public class BOBDataGridMultiSortTests
{
    private sealed record Person(string Department, string Name, int Age);

    private static IEnumerable<Person> Items =>
    [
        new Person("Eng", "Carol", 35), new Person("Sales", "Alice", 30), new Person("Eng", "Bob", 25),
        new Person("Sales", "Dan", 40)
    ];

    private static RenderFragment Columns => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Department");
        b.AddAttribute(2, "Property",
            (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.Department));
        b.AddAttribute(3, "Sortable", true);
        b.CloseComponent();
        b.OpenComponent<BOBDataColumn<Person>>(10);
        b.AddAttribute(11, "Header", "Name");
        b.AddAttribute(12, "Property",
            (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.Name));
        b.AddAttribute(13, "Sortable", true);
        b.CloseComponent();
        b.OpenComponent<BOBDataColumn<Person>>(20);
        b.AddAttribute(21, "Header", "Age");
        b.AddAttribute(22, "Property",
            (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.Age));
        b.AddAttribute(23, "Sortable", true);
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ToggleSort_State_Should_Cycle_AscDescNone(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        DataCollectionState<Person> state = new();
        state.ToggleSort("Name");
        state.SortDirection.Should().Be(SortDirection.Ascending);
        state.SortColumn.Should().Be("Name");

        state.ToggleSort("Name");
        state.SortDirection.Should().Be(SortDirection.Descending);

        state.ToggleSort("Name");
        state.SortColumn.Should().BeNull();
        state.SortDescriptors.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Append_Sort_Should_Stack_Descriptors(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        DataCollectionState<Person> state = new();
        state.ToggleSort("Department", false);
        state.ToggleSort("Name", true);

        state.SortDescriptors.Should().HaveCount(2);
        state.SortDescriptors[0].ColumnName.Should().Be("Department");
        state.SortDescriptors[0].Priority.Should().Be(1);
        state.SortDescriptors[1].ColumnName.Should().Be("Name");
        state.SortDescriptors[1].Priority.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ShiftClick_Header_Should_Add_Secondary_Sort(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns)
            .Add(c => c.Sortable, true));

        // Click "Department" header - primary sort, ascending.
        cut.FindAll("button.bob-datagrid__sort-btn")[0].Click();
        // Shift+Click "Name" - append as secondary.
        cut.FindAll("button.bob-datagrid__sort-btn")[1].Click(new MouseEventArgs { ShiftKey = true });

        // First column has no priority badge (single sort would be just "1");
        // multi-sort shows "1" and "2".
        IReadOnlyList<AngleSharp.Dom.IElement> badges = cut.FindAll(".bob-datagrid__sort-priority");
        badges.Should().HaveCount(2);
        badges[0].TextContent.Trim().Should().Be("1");
        badges[1].TextContent.Trim().Should().Be("2");

        // Rendered order: dept asc → name asc → so Eng/Bob, Eng/Carol, Sales/Alice, Sales/Dan.
        IReadOnlyList<AngleSharp.Dom.IElement> firstCells = cut.FindAll("tbody tr td:nth-child(2)");
        firstCells[0].TextContent.Trim().Should().Be("Bob");
        firstCells[1].TextContent.Trim().Should().Be("Carol");
        firstCells[2].TextContent.Trim().Should().Be("Alice");
        firstCells[3].TextContent.Trim().Should().Be("Dan");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task SingleSort_Should_Hide_Priority_Badge(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns)
            .Add(c => c.Sortable, true));

        cut.FindAll("button.bob-datagrid__sort-btn")[0].Click();
        cut.FindAll(".bob-datagrid__sort-priority").Should().BeEmpty();
    }
}
