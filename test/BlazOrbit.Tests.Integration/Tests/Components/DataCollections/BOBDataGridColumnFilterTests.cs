using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Interaction", "BOBDataGrid")]
public class BOBDataGridColumnFilterTests
{
    private sealed record Person(string Name, int Age);

    private static readonly Expression<Func<Person, object?>> NameExpr = p => (object?)p.Name;
    private static readonly Expression<Func<Person, object?>> AgeExpr = p => (object?)p.Age;

    private static IEnumerable<Person> ThreeItems =>
    [
        new("Alice", 30),
        new("Bob", 25),
        new("Charlie", 40)
    ];

    private static RenderFragment NameAndAgeColumns => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Property", NameExpr);
        b.AddAttribute(3, "Filterable", true);
        b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
        b.CloseComponent();

        b.OpenComponent<BOBDataColumn<Person>>(5);
        b.AddAttribute(6, "Header", "Age");
        b.AddAttribute(7, "Property", AgeExpr);
        b.AddAttribute(8, "Filterable", true);
        b.AddAttribute(9, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Age.ToString())));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Column_Filter_Inputs_Only_When_ShowColumnFilters(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> off = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, ThreeItems)
            .Add(c => c.Columns, NameAndAgeColumns));

        off.FindAll(".bob-datagrid__column-filter").Should().BeEmpty();

        IRenderedComponent<BOBDataGrid<Person>> on = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, ThreeItems)
            .Add(c => c.Columns, NameAndAgeColumns)
            .Add(c => c.ShowColumnFilters, true));

        // Both columns are Filterable so we expect one input per column.
        on.FindAll(".bob-datagrid__column-filter").Count.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Filter_Rows_By_Single_Column(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, ThreeItems)
            .Add(c => c.Columns, NameAndAgeColumns)
            .Add(c => c.ShowColumnFilters, true));

        // Type "li" into the Name filter - should keep Alice (A-l-i-ce) + Charlie
        // (Char-li-e) and drop Bob.
        cut.FindAll(".bob-datagrid__column-filter")[0].Input("li");

        cut.FindAll("tbody [role='row']").Count.Should().Be(2);
        cut.Markup.Should().Contain("Alice");
        cut.Markup.Should().Contain("Charlie");
        cut.Markup.Should().NotContain(">Bob<");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_AND_Combine_Multiple_Column_Filters(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, ThreeItems)
            .Add(c => c.Columns, NameAndAgeColumns)
            .Add(c => c.ShowColumnFilters, true));

        // Name contains "a" → Alice + Charlie. AND Age contains "3" → Alice (30) only.
        cut.FindAll(".bob-datagrid__column-filter")[0].Input("a");
        cut.FindAll(".bob-datagrid__column-filter")[1].Input("3");

        cut.FindAll("tbody [role='row']").Count.Should().Be(1);
        cut.Markup.Should().Contain("Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_AND_Combine_With_Global_FilterText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, ThreeItems)
            .Add(c => c.Columns, NameAndAgeColumns)
            .Add(c => c.ShowColumnFilters, true)
            .Add(c => c.Filterable, true));

        // Toolbar search "a" matches Alice + Charlie. Per-column Age filter "30" narrows
        // to Alice only.
        cut.Find("input[type='text']").Input("a");
        cut.FindAll(".bob-datagrid__column-filter")[1].Input("30");

        cut.FindAll("tbody [role='row']").Count.Should().Be(1);
        cut.Markup.Should().Contain("Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Column_Filter_When_Input_Emptied(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, ThreeItems)
            .Add(c => c.Columns, NameAndAgeColumns)
            .Add(c => c.ShowColumnFilters, true));

        cut.FindAll(".bob-datagrid__column-filter")[0].Input("Alice");
        cut.FindAll("tbody [role='row']").Count.Should().Be(1);

        cut.FindAll(".bob-datagrid__column-filter")[0].Input("");

        cut.FindAll("tbody [role='row']").Count.Should().Be(3);
    }

    [Fact]
    public void State_Set_Column_Filter_Drops_On_Whitespace()
    {
        DataCollectionState<Person> state = new();
        state.SetColumnFilter("Name", "alice");
        state.ColumnFilters.Should().ContainKey("Name");

        // Whitespace / empty == clear (matches the docstring contract).
        state.SetColumnFilter("Name", "  ");
        state.ColumnFilters.Should().NotContainKey("Name");
    }

    [Fact]
    public void State_ClearAll_Preserves_Global_FilterText()
    {
        DataCollectionState<Person> state = new();
        state.FilterText = "global";
        state.SetColumnFilter("Name", "alice");
        state.SetColumnFilter("Age", "30");

        state.ClearAllColumnFilters();

        state.ColumnFilters.Should().BeEmpty();
        state.FilterText.Should().Be("global");
    }
}
