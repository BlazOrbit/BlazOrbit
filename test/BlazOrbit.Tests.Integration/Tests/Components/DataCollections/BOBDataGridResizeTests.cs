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
public class BOBDataGridResizeTests
{
    private sealed record Person(string Name, int Age);

    private static readonly Expression<Func<Person, object?>> NameExpr = p => (object?)p.Name;
    private static readonly Expression<Func<Person, object?>> AgeExpr = p => (object?)p.Age;

    private static IEnumerable<Person> Items =>
    [
        new("Alice", 30),
        new("Bob", 25),
        new("Charlie", 40),
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Resizer_When_Column_And_Grid_Are_Resizable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Resizable, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Property", NameExpr);
                b.AddAttribute(3, "Resizable", true);
                b.AddAttribute(4, "Width", "200px");
                b.AddAttribute(5, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();

                b.OpenComponent<BOBDataColumn<Person>>(6);
                b.AddAttribute(7, "Header", "Age");
                b.AddAttribute(8, "Property", AgeExpr);
                // Second column is NOT Resizable — the grid renders the handle only on
                // columns that explicitly opted in.
                b.AddAttribute(9, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Age.ToString())));
                b.CloseComponent();
            }));

        cut.FindAll(".bob-datagrid__col-resizer").Count.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Resizer_When_Grid_Resizable_Is_Off(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            // Grid-level toggle is the master switch — even an opted-in column stays static
            // until the grid is also Resizable.
            .Add(c => c.Resizable, false)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Property", NameExpr);
                b.AddAttribute(3, "Resizable", true);
                b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        cut.FindAll(".bob-datagrid__col-resizer").Count.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Header_Width_On_Pointer_Drag(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Resizable, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Property", NameExpr);
                b.AddAttribute(3, "Resizable", true);
                b.AddAttribute(4, "Width", "200px");
                b.AddAttribute(5, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        IElement resizer = cut.Find(".bob-datagrid__col-resizer");
        // Pointerdown captures start X + start width (200) — the overlay then handles the move.
        resizer.PointerDown(new PointerEventArgs { ClientX = 100 });

        // Drag overlay must materialise once the gesture starts so pointermove keeps firing
        // even when the cursor leaves the resizer rectangle.
        IElement overlay = cut.Find(".bob-datagrid__resize-overlay");
        overlay.PointerMove(new PointerEventArgs { ClientX = 175 });

        // Header style now reflects the resized width (200 + 75 = 275).
        IElement header = cut.Find("th.bob-datagrid__header-cell");
        header.GetAttribute("style").Should().Contain("width: 275");

        overlay.PointerUp(new PointerEventArgs { ClientX = 175 });

        // Overlay disappears after pointerup so the rest of the grid is interactive again.
        cut.FindAll(".bob-datagrid__resize-overlay").Count.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_Drag_To_MinWidth(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Resizable, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BOBDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Property", NameExpr);
                b.AddAttribute(3, "Resizable", true);
                b.AddAttribute(4, "Width", "200px");
                b.AddAttribute(5, "MinWidth", 80d);
                b.AddAttribute(6, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        cut.Find(".bob-datagrid__col-resizer").PointerDown(new PointerEventArgs { ClientX = 500 });
        // Drag 500px to the left — would land at -300, but the MinWidth=80 clamp kicks in.
        cut.Find(".bob-datagrid__resize-overlay").PointerMove(new PointerEventArgs { ClientX = 0 });

        cut.Find("th.bob-datagrid__header-cell").GetAttribute("style").Should().Contain("width: 80");
    }

    [Fact]
    public void State_ColumnWidths_RoundTrips_Through_Json()
    {
        DataCollectionState<Person> source = new();
        source.SetColumnWidth("Name", 240);
        source.SetColumnWidth("Age", 96);

        string json = source.ToJson();

        DataCollectionState<Person> target = new();
        target.LoadFromJson(json);

        target.ColumnWidths.Should().HaveCount(2);
        target.ColumnWidths["Name"].Should().Be(240);
        target.ColumnWidths["Age"].Should().Be(96);
    }

    [Fact]
    public void State_SetColumnWidth_With_Zero_Or_Negative_Removes_Entry()
    {
        DataCollectionState<Person> state = new();
        state.SetColumnWidth("Name", 200);
        state.ColumnWidths.Should().ContainKey("Name");

        // 0 / negative is the documented "drop the override" signal — useful for a
        // double-click "reset to default" affordance built on top of the same setter.
        state.SetColumnWidth("Name", 0);
        state.ColumnWidths.Should().NotContainKey("Name");
    }
}
