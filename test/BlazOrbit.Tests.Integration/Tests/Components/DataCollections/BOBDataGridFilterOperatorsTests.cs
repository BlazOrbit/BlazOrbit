using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Interaction", "BOBDataGrid")]
public class BOBDataGridFilterOperatorsTests
{
    private sealed record Person(string Name, int Age, DateTime BirthDay);

    private static readonly Expression<Func<Person, object?>> NameExpr = p => (object?)p.Name;
    private static readonly Expression<Func<Person, object?>> AgeExpr = p => (object?)p.Age;
    private static readonly Expression<Func<Person, object?>> BirthExpr = p => (object?)p.BirthDay;

    private static IEnumerable<Person> Items =>
    [
        new("Alice",   30, new DateTime(1995, 1, 15)),
        new("Bob",     25, new DateTime(2000, 6, 1)),
        new("Charlie", 40, new DateTime(1985, 12, 20)),
    ];

    private static int RowCount(IRenderedComponent<BOBDataGrid<Person>> cut) =>
        cut.FindAll("tbody [role='row']").Count;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Operator_Dropdown_For_Filterable_Column(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.ShowColumnFilters, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Property", NameExpr);
                b.AddAttribute(3, "Filterable", true);
                b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        cut.FindAll(".bob-datagrid__column-filter-op").Count.Should().Be(1);
        // Text columns expose 5 operators (Contains/StartsWith/EndsWith/Equals/NotEquals).
        cut.FindAll(".bob-datagrid__column-filter-op option").Count.Should().Be(5);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Filter_Numeric_Column_With_GreaterThan(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.ShowColumnFilters, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Age");
                b.AddAttribute(2, "Property", AgeExpr);
                b.AddAttribute(3, "Filterable", true);
                b.AddAttribute(4, "FilterMode", ColumnFilterMode.Numeric);
                b.AddAttribute(5, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Age.ToString())));
                b.CloseComponent();
            }));

        // Numeric columns expose 6 operators (= != > < >= <=).
        cut.FindAll(".bob-datagrid__column-filter-op option").Count.Should().Be(6);

        // Equals 30 matches Alice exactly (default operator on a numeric column is =).
        cut.Find(".bob-datagrid__column-filter").Input("30");
        RowCount(cut).Should().Be(1);
        cut.Markup.Should().Contain(">30<");

        // Switch operator to GreaterOrEqual on the same value — keeps Alice (30) and adds
        // Charlie (40). Bob (25) is still filtered out.
        cut.Find(".bob-datagrid__column-filter-op").Change(ColumnFilterOperator.GreaterOrEqual.ToString());
        RowCount(cut).Should().Be(2);
        cut.Markup.Should().Contain(">30<");
        cut.Markup.Should().Contain(">40<");
        cut.Markup.Should().NotContain(">25<");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Filter_Date_Column_With_LessThan(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.ShowColumnFilters, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "BirthDay");
                b.AddAttribute(2, "Property", BirthExpr);
                b.AddAttribute(3, "Filterable", true);
                b.AddAttribute(4, "FilterMode", ColumnFilterMode.Date);
                b.AddAttribute(5, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.BirthDay.ToString("yyyy-MM-dd"))));
                b.CloseComponent();
            }));

        // Type "1990-01-01" + LessThan → Charlie (1985-12-20) matches. Body cell renders
        // the formatted date (single-column grid), so assert on that.
        cut.Find(".bob-datagrid__column-filter").Input("1990-01-01");
        cut.Find(".bob-datagrid__column-filter-op").Change(ColumnFilterOperator.LessThan.ToString());

        RowCount(cut).Should().Be(1);
        cut.Markup.Should().Contain("1985-12-20");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Switch_Text_Operators_StartsWith_Then_EndsWith(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.ShowColumnFilters, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Property", NameExpr);
                b.AddAttribute(3, "Filterable", true);
                b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        // StartsWith "ch" → Charlie only.
        cut.Find(".bob-datagrid__column-filter").Input("ch");
        cut.Find(".bob-datagrid__column-filter-op").Change(ColumnFilterOperator.StartsWith.ToString());
        RowCount(cut).Should().Be(1);
        cut.Markup.Should().Contain("Charlie");

        // EndsWith "e" → Alice + Charlie.
        cut.Find(".bob-datagrid__column-filter").Input("e");
        cut.Find(".bob-datagrid__column-filter-op").Change(ColumnFilterOperator.EndsWith.ToString());
        RowCount(cut).Should().Be(2);
        cut.Markup.Should().Contain("Alice");
        cut.Markup.Should().Contain("Charlie");
    }

    [Fact]
    public void State_SetColumnFilter_Preserves_Operator_On_Text_Update()
    {
        DataCollectionState<Person> state = new();
        state.SetColumnFilter("Name", new ColumnFilterEntry("alice", ColumnFilterOperator.StartsWith, ColumnFilterMode.Text));

        // Updating just the text via the simple overload must keep the previously
        // configured operator + mode — typing should not reset the dropdown choice.
        state.SetColumnFilter("Name", "ali");

        state.ColumnFilters["Name"].Operator.Should().Be(ColumnFilterOperator.StartsWith);
        state.ColumnFilters["Name"].Mode.Should().Be(ColumnFilterMode.Text);
        state.ColumnFilters["Name"].Text.Should().Be("ali");
    }

    [Fact]
    public void State_SetColumnFilterOperator_Is_Noop_When_No_Active_Entry()
    {
        DataCollectionState<Person> state = new();
        state.SetColumnFilterOperator("Name", ColumnFilterOperator.StartsWith);
        state.ColumnFilters.Should().BeEmpty();
    }
}
