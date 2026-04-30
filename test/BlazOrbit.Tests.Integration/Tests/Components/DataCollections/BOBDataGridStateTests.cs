using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component State", "BOBDataGrid")]
public class BOBDataGridStateTests
{
    private sealed record Person(string Name, int Age);

    private static readonly Expression<Func<Person, object?>> NameExpr = p => (object?)p.Name;

    private static RenderFragment ColumnsWithSort => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Sortable", true);
        b.AddAttribute(3, "Property", NameExpr);
        b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
        b.CloseComponent();
    };

    private static RenderFragment ColumnsWithFilter => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Filterable", true);
        b.AddAttribute(3, "Property", NameExpr);
        b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Rows_When_Items_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, [new Person("Alice", 30)])
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        cut.FindAll("[role='gridcell']").Should().HaveCount(1);

        // Act — add another item
        cut.Render(p => p
            .Add(c => c.Items, [new Person("Alice", 30), new Person("Bob", 25)])
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        // Assert
        cut.FindAll("[role='gridcell']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Filter_Items_By_Text(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, [new Person("Alice", 30), new Person("Bob", 25)])
            .Add(c => c.Filterable, true)
            .Add(c => c.Columns, ColumnsWithFilter));

        cut.FindAll("[role='gridcell']").Should().HaveCount(2);

        // Act — filter
        cut.Find("[aria-label='Search...']").Input("Ali");

        // Assert
        cut.FindAll("[role='gridcell']").Should().HaveCount(1);
        cut.Find("[role='gridcell']").TextContent.Should().Be("Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Sort_Items_Ascending_On_Header_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, [new Person("Bob", 25), new Person("Alice", 30)])
            .Add(c => c.Sortable, true)
            .Add(c => c.Columns, ColumnsWithSort));

        // Act
        cut.Find(".bob-datagrid__sort-btn").Click();

        // Assert — sorted ascending
        cut.FindAll("[role='gridcell']")[0].TextContent.Should().Be("Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Paginate_Items(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — 3 items, page size 2
        IEnumerable<Person> items = [new Person("A", 1), new Person("B", 2), new Person("C", 3)];

        // Act
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.PageSize, 2)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        // Assert — only 2 items shown on first page
        cut.FindAll("[role='gridcell']").Should().HaveCount(2);
    }
}
