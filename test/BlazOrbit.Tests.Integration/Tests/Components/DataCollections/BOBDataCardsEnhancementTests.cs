using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Variant", "BOBDataCards.Phase1Enhancements")]
public class BOBDataCardsEnhancementTests
{
    private sealed record Person(string Name, int Age);
    private sealed record Order(string Region, decimal Amount);

    private static IEnumerable<Person> People => new[]
    {
        new Person("Alice", 30),
        new Person("Bob", 25),
        new Person("Carol", 35),
    };

    private static RenderFragment PersonCols => b =>
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
    public async Task RowActions_Should_Render_Per_Card(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, People)
            .Add(c => c.Columns, PersonCols)
            .Add(c => c.RowActions, new[]
            {
                new DataCollectionRowAction<Person> { Label = "Edit", Icon = BOBIconKeys.MaterialIconsOutlined.i_edit, OnClick = _ => Task.CompletedTask },
                new DataCollectionRowAction<Person> { Label = "Delete", Color = PaletteColor.Error, OnClick = _ => Task.CompletedTask },
            }));

        // 3 cards × 2 actions = 6 buttons in row-action strips.
        cut.FindAll(".bob-datacards__card-actions .bob-dc__row-actions button").Should().HaveCount(6);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Bulk_Actions_Should_Render_Toolbar_Buttons_When_Selected(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IReadOnlyCollection<Person>? captured = null;

        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, People)
            .Add(c => c.Columns, PersonCols)
            .Add(c => c.SelectionMode, SelectionMode.Multiple)
            .Add(c => c.BulkActions, new[]
            {
                new DataCollectionBulkAction<Person>
                {
                    Label = "Email",
                    Icon = BOBIconKeys.MaterialIconsOutlined.i_mail,
                    OnClick = sel => { captured = sel; return Task.CompletedTask; },
                },
            }));

        // Click first card to select it.
        await cut.InvokeAsync(() => cut.FindAll(".bob-datacards__card-select").First().Click());
        cut.FindAll(".bob-dc__selection-info button").Last().Click();

        captured.Should().NotBeNull();
        captured!.Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Row_Detail_Should_Toggle_Inline_Section(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, People)
            .Add(c => c.Columns, PersonCols)
            .Add(c => c.RowDetailTemplate,
                (RenderFragment<Person>)(person => b => b.AddContent(0, $"More about {person.Name}"))));

        // No detail open initially.
        cut.FindAll(".bob-datacards__card-detail").Should().BeEmpty();

        // Open Alice's detail.
        cut.FindAll(".bob-datacards__card-actions button").First().Click();

        IReadOnlyList<AngleSharp.Dom.IElement> details = cut.FindAll(".bob-datacards__card-detail");
        details.Should().HaveCount(1);
        details[0].TextContent.Should().Contain("Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Aggregate_Summary_Band_Should_Render_Below_Grid(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IEnumerable<Order> orders = new[]
        {
            new Order("EMEA", 100m),
            new Order("APAC", 250m),
            new Order("EMEA", 150m),
        };

        IRenderedComponent<BOBDataCards<Order>> cut = ctx.Render<BOBDataCards<Order>>(p => p
            .Add(c => c.Items, orders)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Order>>(0);
                b.AddAttribute(1, "Header", "Region");
                b.AddAttribute(2, "Property",
                    (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Region));
                b.AddAttribute(3, "Aggregate", AggregateFunction.Count);
                b.CloseComponent();
                b.OpenComponent<BOBDataColumn<Order>>(10);
                b.AddAttribute(11, "Header", "Amount");
                b.AddAttribute(12, "Property",
                    (System.Linq.Expressions.Expression<Func<Order, object?>>)(o => o.Amount));
                b.AddAttribute(13, "Aggregate", AggregateFunction.Sum);
                b.AddAttribute(14, "AggregateFormat", "0");
                b.CloseComponent();
            }));

        IReadOnlyList<AngleSharp.Dom.IElement> cells = cut.FindAll(".bob-datacards__summary-cell");
        cells.Should().HaveCount(2);
        cut.Find(".bob-datacards__summary").Should().NotBeNull();
        cells[0].TextContent.Should().Contain("Region").And.Contain("3");
        cells[1].TextContent.Should().Contain("Amount").And.Contain("500");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Skeleton_Loading_Should_Render_Placeholder_Cards(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, Array.Empty<Person>())
            .Add(c => c.Columns, PersonCols)
            .Add(c => c.Loading, true)
            .Add(c => c.LoadingMode, LoadingMode.Skeleton)
            .Add(c => c.PageSize, 4));

        cut.FindAll(".bob-dc__skeleton-card").Should().HaveCount(4);
        cut.FindAll(".bob-datacards__grid--skeleton").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Error_Should_Render_Alert(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, Array.Empty<Person>())
            .Add(c => c.Columns, PersonCols)
            .Add(c => c.Error, "Network down"));

        cut.Find(".bob-dc__error").TextContent.Should().Contain("Network down");
        cut.FindAll(".bob-datacards__grid").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Empty_CTA_Should_Render_Inside_Default_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataCards<Person>> cut = ctx.Render<BOBDataCards<Person>>(p => p
            .Add(c => c.Items, Array.Empty<Person>())
            .Add(c => c.Columns, PersonCols)
            .Add(c => c.EmptyActionTemplate,
                (RenderFragment)(b => b.AddMarkupContent(0, "<button class=\"my-cta\">Create first</button>"))));

        cut.FindAll(".bob-dc__empty .my-cta").Should().HaveCount(1);
    }
}
