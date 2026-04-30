using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Interaction", "BOBDataColumn")]
public class BOBDataColumnInteractionTests
{
    private sealed record Person(string Name, int Age);
    private static readonly Expression<Func<Person, object?>> NameExpr = p => (object?)p.Name;
    private static readonly Expression<Func<Person, object?>> AgeExpr = p => (object?)p.Age;

    private static IEnumerable<Person> TwoItems => [new Person("Bob", 25), new Person("Alice", 30)];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnSort_With_Column_Header_When_Sort_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DataCollectionSortEventArgs? captured = null;
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, TwoItems)
            .Add(c => c.Sortable, true)
            .Add(c => c.OnSort, args => captured = args)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Sortable", true);
                b.AddAttribute(3, "Property", NameExpr);
                b.CloseComponent();
            }));

        // Act
        cut.Find(".bob-datagrid__sort-btn").Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.ColumnName.Should().Be("Name");
        captured.Direction.Should().Be(SortDirection.Ascending);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Sort_Direction_On_Repeated_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, TwoItems)
            .Add(c => c.Sortable, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Sortable", true);
                b.AddAttribute(3, "Property", NameExpr);
                b.CloseComponent();
            }));

        // Act — first click ascending
        cut.Find(".bob-datagrid__sort-btn").Click();
        cut.Find("[role='columnheader']").GetAttribute("aria-sort").Should().Be("ascending");

        // Act — second click descending
        cut.Find(".bob-datagrid__sort-btn").Click();

        // Assert
        cut.Find("[role='columnheader']").GetAttribute("aria-sort").Should().Be("descending");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Sort_By_Correct_Column_When_Multiple_Sortable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — two sortable columns; click Age sort only
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, TwoItems)
            .Add(c => c.Sortable, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Sortable", true);
                b.AddAttribute(3, "Property", NameExpr);
                b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
                b.OpenComponent<BOBDataColumn<Person>>(5);
                b.AddAttribute(6, "Header", "Age");
                b.AddAttribute(7, "Sortable", true);
                b.AddAttribute(8, "Property", AgeExpr);
                b.AddAttribute(9, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Age.ToString())));
                b.CloseComponent();
            }));

        // Act — click the Age sort button (second)
        cut.FindAll(".bob-datagrid__sort-btn")[1].Click();

        // Assert — Bob (25) first, Alice (30) second
        cut.FindAll("[role='gridcell']")[1].TextContent.Should().Be("25");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Custom_Comparer_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — reverse-length comparer on Name
        Func<Person, Person, int> comparer = (a, b) => a.Name.Length.CompareTo(b.Name.Length);

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, [new Person("Alice", 30), new Person("Bo", 25), new Person("Charlie", 40)])
            .Add(c => c.Sortable, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Sortable", true);
                b.AddAttribute(3, "Property", NameExpr);
                b.AddAttribute(4, "CustomComparer", comparer);
                b.AddAttribute(5, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        // Act
        cut.Find(".bob-datagrid__sort-btn").Click();

        // Assert — shortest name first
        cut.FindAll("[role='gridcell']")[0].TextContent.Should().Be("Bo");
        cut.FindAll("[role='gridcell']")[2].TextContent.Should().Be("Charlie");
    }
}
