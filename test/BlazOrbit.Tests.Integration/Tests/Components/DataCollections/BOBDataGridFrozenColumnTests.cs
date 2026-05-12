using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Rendering", "BOBDataGrid")]
public class BOBDataGridFrozenColumnTests
{
    private sealed record Person(string Name, int Age, string Country);

    private static IEnumerable<Person> Items =>
    [
        new("Alice", 30, "ES"),
        new("Bob", 25, "DE")
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Frozen_Start_Class_And_Left_Offset_Zero(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Width", "180px");
                b.AddAttribute(3, "Freeze", ColumnFreeze.Start);
                b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();

                b.OpenComponent<BOBDataColumn<Person>>(5);
                b.AddAttribute(6, "Header", "Age");
                b.AddAttribute(7, "Template",
                    (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Age.ToString())));
                b.CloseComponent();
            }));

        // Header cell of the frozen column carries both the modifier classes + a left:0 offset
        // because it is the first frozen column on the start edge.
        AngleSharp.Dom.IElement header = cut.FindAll(".bob-datagrid__header-cell")[0];
        header.ClassList.Should().Contain("bob-datagrid__cell--frozen");
        header.ClassList.Should().Contain("bob-datagrid__cell--frozen-start");
        header.GetAttribute("style").Should().Contain("left: 0px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Stack_Frozen_Start_Offsets(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Width", "180px");
                b.AddAttribute(3, "Freeze", ColumnFreeze.Start);
                b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();

                b.OpenComponent<BOBDataColumn<Person>>(5);
                b.AddAttribute(6, "Header", "Age");
                b.AddAttribute(7, "Width", "100px");
                b.AddAttribute(8, "Freeze", ColumnFreeze.Start);
                b.AddAttribute(9, "Template",
                    (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Age.ToString())));
                b.CloseComponent();

                b.OpenComponent<BOBDataColumn<Person>>(10);
                b.AddAttribute(11, "Header", "Country");
                b.AddAttribute(12, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Country)));
                b.CloseComponent();
            }));

        // Second frozen-start column should pick up the cumulative offset (180px).
        IReadOnlyList<IElement> headers = cut.FindAll(".bob-datagrid__header-cell");
        headers[0].GetAttribute("style").Should().Contain("left: 0px");
        headers[1].GetAttribute("style").Should().Contain("left: 180px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Frozen_End_Right_Offset(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();

                b.OpenComponent<BOBDataColumn<Person>>(3);
                b.AddAttribute(4, "Header", "Country");
                b.AddAttribute(5, "Width", "120px");
                b.AddAttribute(6, "Freeze", ColumnFreeze.End);
                b.AddAttribute(7, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Country)));
                b.CloseComponent();
            }));

        // Last column frozen at end gets right:0 (nothing further right is frozen).
        IReadOnlyList<IElement> headers = cut.FindAll(".bob-datagrid__header-cell");
        AngleSharp.Dom.IElement endHeader = headers[headers.Count - 1];
        endHeader.ClassList.Should().Contain("bob-datagrid__cell--frozen-end");
        endHeader.GetAttribute("style").Should().Contain("right: 0px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Frozen_To_Body_Cells(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Width", "200px");
                b.AddAttribute(3, "Freeze", ColumnFreeze.Start);
                b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();

                b.OpenComponent<BOBDataColumn<Person>>(5);
                b.AddAttribute(6, "Header", "Age");
                b.AddAttribute(7, "Template",
                    (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Age.ToString())));
                b.CloseComponent();
            }));

        // Every body cell in the frozen column carries the modifier so sticky positioning
        // applies row-by-row, not just on the header.
        IReadOnlyList<IElement> bodyCells = cut.FindAll("tbody [role='gridcell']");
        // 2 rows × 2 cols = 4 cells; the first cell of each row is the frozen one.
        bodyCells[0].ClassList.Should().Contain("bob-datagrid__cell--frozen-start");
        bodyCells[2].ClassList.Should().Contain("bob-datagrid__cell--frozen-start");
        // The other ones (not frozen) should not have the modifier.
        bodyCells[1].ClassList.Should().NotContain("bob-datagrid__cell--frozen");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Apply_Frozen_Class_When_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        cut.FindAll(".bob-datagrid__cell--frozen").Should().BeEmpty();
    }
}