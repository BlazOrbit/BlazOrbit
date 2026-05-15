using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Interaction", "BOBDataGrid")]
public class BOBDataGridReorderTests
{
    private sealed record Person(string Name, int Age, string Country);

    private static IEnumerable<Person> Items =>
    [
        new("Alice", 30, "ES"),
        new("Bob", 25, "DE")
    ];

    private static RenderFragment ThreeColumns => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
        b.CloseComponent();

        b.OpenComponent<BOBDataColumn<Person>>(3);
        b.AddAttribute(4, "Header", "Age");
        b.AddAttribute(5, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Age.ToString())));
        b.CloseComponent();

        b.OpenComponent<BOBDataColumn<Person>>(6);
        b.AddAttribute(7, "Header", "Country");
        b.AddAttribute(8, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Country)));
        b.CloseComponent();
    };

    private static IReadOnlyList<string> HeaderTexts(IRenderedComponent<BOBDataGrid<Person>> cut) =>
        [.. cut.FindAll(".bob-datagrid__header-cell").Select(h => h.TextContent.Trim())];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Headers_In_Registration_Order_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, ThreeColumns));

        HeaderTexts(cut).Should().BeEquivalentTo(["Name", "Age", "Country"], o => o.WithStrictOrdering());
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Tabindex_When_Reorderable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> off = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, ThreeColumns));

        off.FindAll(".bob-datagrid__header-cell")[0].GetAttribute("tabindex").Should().BeNull();

        IRenderedComponent<BOBDataGrid<Person>> on = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, ThreeColumns)
            .Add(c => c.Reorderable, true));

        on.FindAll(".bob-datagrid__header-cell")[0].GetAttribute("tabindex").Should().Be("0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Move_Column_Right_On_Alt_ArrowRight(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, ThreeColumns)
            .Add(c => c.Reorderable, true));

        // Focus the first header (Name) and press Alt+ArrowRight → Name moves to index 1.
        cut.FindAll(".bob-datagrid__header-cell")[0]
            .KeyDown(new KeyboardEventArgs { Key = "ArrowRight", AltKey = true });

        HeaderTexts(cut).Should().BeEquivalentTo(["Age", "Name", "Country"], o => o.WithStrictOrdering());
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Move_Column_Left_On_Alt_ArrowLeft(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, ThreeColumns)
            .Add(c => c.Reorderable, true));

        // Focus the third header (Country) and press Alt+ArrowLeft → Country moves to index 1.
        cut.FindAll(".bob-datagrid__header-cell")[2].KeyDown(new KeyboardEventArgs
        {
            Key = "ArrowLeft", AltKey = true
        });

        HeaderTexts(cut).Should().BeEquivalentTo(["Name", "Country", "Age"], o => o.WithStrictOrdering());
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Ignore_Arrow_Without_Alt(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, ThreeColumns)
            .Add(c => c.Reorderable, true));

        cut.FindAll(".bob-datagrid__header-cell")[0].KeyDown(new KeyboardEventArgs
        {
            Key = "ArrowRight", AltKey = false
        });

        // No reorder happened - the bare arrow keys are reserved for future native focus
        // navigation between headers.
        HeaderTexts(cut).Should().BeEquivalentTo(["Name", "Age", "Country"], o => o.WithStrictOrdering());
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_At_Boundaries(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, ThreeColumns)
            .Add(c => c.Reorderable, true));

        // Try to move the first column further left - clamped to index 0.
        cut.FindAll(".bob-datagrid__header-cell")[0].KeyDown(new KeyboardEventArgs
        {
            Key = "ArrowLeft", AltKey = true
        });

        HeaderTexts(cut).Should().BeEquivalentTo(["Name", "Age", "Country"], o => o.WithStrictOrdering());

        // Same for the last column moving right.
        cut.FindAll(".bob-datagrid__header-cell")[2]
            .KeyDown(new KeyboardEventArgs { Key = "ArrowRight", AltKey = true });

        HeaderTexts(cut).Should().BeEquivalentTo(["Name", "Age", "Country"], o => o.WithStrictOrdering());
    }

    [Fact]
    public void State_MoveColumn_Lazily_Seeds_From_Reference_Order()
    {
        DataCollectionState<Person> state = new();
        string[] reference = ["Name", "Age", "Country"];

        state.MoveColumn("Age", -1, reference);

        state.ColumnOrder.Should().BeEquivalentTo(["Age", "Name", "Country"], o => o.WithStrictOrdering());
    }

    [Fact]
    public void State_ClearColumnOrder_Reverts_To_Empty()
    {
        DataCollectionState<Person> state = new();
        state.SetColumnOrder(["A", "B", "C"]);
        state.ColumnOrder.Should().HaveCount(3);

        state.ClearColumnOrder();

        state.ColumnOrder.Should().BeEmpty();
    }
}
