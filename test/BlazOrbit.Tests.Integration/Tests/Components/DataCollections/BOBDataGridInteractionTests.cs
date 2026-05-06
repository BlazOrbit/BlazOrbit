using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Interaction", "BOBDataGrid")]
public class BOBDataGridInteractionTests
{
    private sealed record Person(string Name, int Age);
    private static readonly Expression<Func<Person, object?>> NameExpr = p => (object?)p.Name;

    private static IEnumerable<Person> TwoItems => [new Person("Alice", 30), new Person("Bob", 25)];

    private static RenderFragment SimpleColumns => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnRowClick_When_Row_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Person? clicked = null;
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, TwoItems)
            .Add(c => c.Columns, SimpleColumns)
            .Add(c => c.OnRowClick, person => clicked = person));

        // Act
        cut.FindAll("[role='row']")[1].Click(); // [0] = header, [1] = first data row

        // Assert
        clicked.Should().NotBeNull();
        clicked!.Name.Should().Be("Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Select_Row_On_Click_In_Single_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, TwoItems)
            .Add(c => c.Columns, SimpleColumns)
            .Add(c => c.SelectionMode, SelectionMode.Single));

        // Act
        cut.FindAll("[role='row']")[1].Click();

        // Assert — first data row selected
        cut.FindAll("[role='row']")[1].GetAttribute("data-bob-selected").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Sort_Ascending_Then_Descending_On_Header_Clicks(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, [new Person("Bob", 25), new Person("Alice", 30)])
            .Add(c => c.Sortable, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Sortable", true);
                b.AddAttribute(3, "Property", NameExpr);
                b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        // Act — first click: ascending
        cut.Find(".bob-datagrid__sort-btn").Click();
        cut.FindAll("[role='gridcell']")[0].TextContent.Should().Be("Alice");

        // Act — second click: descending
        cut.Find(".bob-datagrid__sort-btn").Click();

        // Assert
        cut.FindAll("[role='gridcell']")[0].TextContent.Should().Be("Bob");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_SelectedItemsChanged_When_Row_Selected(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        HashSet<Person>? captured = null;
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, TwoItems)
            .Add(c => c.Columns, SimpleColumns)
            .Add(c => c.SelectionMode, SelectionMode.Single)
            .Add(c => c.SelectedItemsChanged, items => captured = items));

        // Act
        cut.FindAll("[role='row']")[1].Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.Should().ContainSingle(p => p.Name == "Alice");
    }
}
