using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component State", "BOBDataCards")]
public class BOBDataCardsStateTests
{
    private sealed record Person(string Name, int Age);
    private static readonly Expression<Func<Person, object?>> NameExpr = p => (object?)p.Name;

    private static RenderFragment Columns => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Cards_When_Items_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, [new Person("Alice", 30)])
            .Add(c => c.Columns, Columns));

        cut.FindAll(".bob-datacards__card").Should().HaveCount(1);

        // Act
        cut.Render(p => p
            .Add(c => c.Items, [new Person("Alice", 30), new Person("Bob", 25)])
            .Add(c => c.Columns, Columns));

        // Assert
        cut.FindAll(".bob-datacards__card").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Filter_Cards_By_Text(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, [new Person("Alice", 30), new Person("Bob", 25)])
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

        // Act — type in filter
        cut.Find("[aria-label='Search...']").Input("Ali");

        // Assert
        cut.FindAll(".bob-datacards__card").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Select_Card_On_Click_In_Single_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, [new Person("Alice", 30)])
            .Add(c => c.Columns, Columns)
            .Add(c => c.SelectionMode, SelectionMode.Single));

        // Act
        cut.Find(".bob-datacards__card").Click();

        // Assert
        cut.Find(".bob-datacards__card").ClassList.Should().Contain("bob-datacards__card--selected");
    }
}
