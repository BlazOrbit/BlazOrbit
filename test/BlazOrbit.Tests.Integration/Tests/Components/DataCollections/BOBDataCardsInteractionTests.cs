using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Interaction", "BOBDataCards")]
public class BOBDataCardsInteractionTests
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
    public async Task Should_Fire_OnRowClick_When_Card_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Person? clicked = null;
        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, TwoItems)
            .Add(c => c.Columns, SimpleColumns)
            .Add(c => c.OnRowClick, person => clicked = person));

        // Act — click the first card
        cut.FindAll(".bob-datacards__card")[0].Click();

        // Assert
        clicked.Should().NotBeNull();
        clicked!.Name.Should().Be("Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_SelectedItemsChanged_When_Card_Selected(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        HashSet<Person>? captured = null;
        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, TwoItems)
            .Add(c => c.Columns, SimpleColumns)
            .Add(c => c.SelectionMode, SelectionMode.Single)
            .Add(c => c.SelectedItemsChanged, items => captured = items));

        // Act
        cut.FindAll(".bob-datacards__card")[0].Click();

        // Assert
        captured.Should().NotBeNull();
        captured!.Should().ContainSingle(p => p.Name == "Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Selection_In_Multiple_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, TwoItems)
            .Add(c => c.Columns, SimpleColumns)
            .Add(c => c.SelectionMode, SelectionMode.Multiple));

        // Act — select both, then deselect the first
        cut.FindAll(".bob-datacards__card")[0].Click();
        cut.FindAll(".bob-datacards__card")[1].Click();
        cut.FindAll(".bob-datacards__card")[0].Click();

        // Assert — only second is selected
        cut.FindAll(".bob-datacards__card")[0].ClassList.Should().NotContain("bob-datacards__card--selected");
        cut.FindAll(".bob-datacards__card")[1].ClassList.Should().Contain("bob-datacards__card--selected");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Sort_Cards_When_Sort_Toggle_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, [new Person("Bob", 25), new Person("Alice", 30)])
            .Add(c => c.Sortable, true)
            .Add(c => c.DefaultSortColumn, "Name")
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Sortable", true);
                b.AddAttribute(3, "Property", NameExpr);
                b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        // Assert — default ascending order: Alice first
        cut.FindAll(".bob-datacards__field-value")[0].TextContent.Should().Be("Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Filter_When_Close_Button_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, TwoItems)
            .Add(c => c.Filterable, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Filterable", true);
                b.AddAttribute(3, "Property", NameExpr);
                b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        cut.Find("[aria-label='Search...']").Input("Ali");
        cut.FindAll(".bob-datacards__card").Should().HaveCount(1);

        // Act — click the clear-filter button (aria-label="Clear filter")
        cut.Find("[aria-label='Clear filter']").Click();

        // Assert — all cards visible again
        cut.FindAll(".bob-datacards__card").Should().HaveCount(2);
    }
}
