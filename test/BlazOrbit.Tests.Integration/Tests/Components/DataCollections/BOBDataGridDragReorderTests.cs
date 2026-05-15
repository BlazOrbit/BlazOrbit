using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Interaction", "BOBDataGrid")]
public class BOBDataGridDragReorderTests
{
    private sealed record Person(string Name, int Age);

    private static readonly Expression<Func<Person, object?>> NameExpr = p => (object?)p.Name;
    private static readonly Expression<Func<Person, object?>> AgeExpr = p => (object?)p.Age;

    private static IEnumerable<Person> Items =>
    [
        new("Alice", 30),
        new("Bob", 25)
    ];

    private static IRenderedComponent<BOBDataGrid<Person>> RenderGrid(BlazorTestContextBase ctx,
        bool reorderable = true) =>
        ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Reorderable, reorderable)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Property", NameExpr);
                b.AddAttribute(3, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();

                b.OpenComponent<BOBDataColumn<Person>>(4);
                b.AddAttribute(5, "Header", "Age");
                b.AddAttribute(6, "Property", AgeExpr);
                b.AddAttribute(7, "Template",
                    (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Age.ToString())));
                b.CloseComponent();
            }));

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Header_Draggable_When_Reorderable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBDataGrid<Person>> cut = RenderGrid(ctx);

        IReadOnlyList<IElement> headers = cut.FindAll("th.bob-datagrid__header-cell");
        // Both columns participate - keyboard tab order + drag affordance are paired.
        headers.Should().AllSatisfy(h => h.GetAttribute("draggable").Should().Be("true"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Mark_Header_Draggable_When_Reorderable_Off(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBDataGrid<Person>> cut = RenderGrid(ctx, false);

        cut.Find("th.bob-datagrid__header-cell").GetAttribute("draggable").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reorder_Columns_When_Header_Dropped_On_Peer(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBDataGrid<Person>> cut = RenderGrid(ctx);

        // Initial order: Name (0) | Age (1).
        IReadOnlyList<IElement> before = cut.FindAll("th.bob-datagrid__header-cell");
        before[0].TextContent.Should().Contain("Name");
        before[1].TextContent.Should().Contain("Age");

        // Pick up "Age" and drop it onto "Name" - Age should land at index 0. We re-find
        // the drop target after DragStart so Bunit dispatches to the freshest element ref
        // (DragStart triggers a re-render that invalidates the previous handler IDs).
        before[1].DragStart(new Microsoft.AspNetCore.Components.Web.DragEventArgs { DataTransfer = new DataTransfer() });
        cut.FindAll("th.bob-datagrid__header-cell")[0]
            .Drop(new Microsoft.AspNetCore.Components.Web.DragEventArgs { DataTransfer = new DataTransfer() });

        IReadOnlyList<IElement> after = cut.FindAll("th.bob-datagrid__header-cell");
        after[0].TextContent.Should().Contain("Age");
        after[1].TextContent.Should().Contain("Name");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Be_Noop_When_Dropped_On_Same_Column(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BOBDataGrid<Person>> cut = RenderGrid(ctx);

        IReadOnlyList<IElement> before = cut.FindAll("th.bob-datagrid__header-cell");
        // Self-drop is a common UX accident - the grid should silently ignore it instead
        // of looping the column through MoveColumn(0).
        before[0].DragStart(new Microsoft.AspNetCore.Components.Web.DragEventArgs { DataTransfer = new DataTransfer() });
        cut.FindAll("th.bob-datagrid__header-cell")[0]
            .Drop(new Microsoft.AspNetCore.Components.Web.DragEventArgs { DataTransfer = new DataTransfer() });

        IReadOnlyList<IElement> after = cut.FindAll("th.bob-datagrid__header-cell");
        after[0].TextContent.Should().Contain("Name");
        after[1].TextContent.Should().Contain("Age");
    }
}
