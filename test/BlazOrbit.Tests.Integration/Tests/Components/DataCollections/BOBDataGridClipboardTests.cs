using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Interaction", "BOBDataGrid")]
public class BOBDataGridClipboardTests
{
    private sealed class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private static List<Person> Seed() =>
    [
        new() { Name = "Alice", Age = 30 },
        new() { Name = "Bob",   Age = 25 }
    ];

    private static RenderFragment Columns() => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Property", (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.Name));
        b.CloseComponent();
        b.OpenComponent<BOBDataColumn<Person>>(3);
        b.AddAttribute(4, "Header", "Age");
        b.AddAttribute(5, "Property", (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.Age));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Copy_Button_Renders_When_Copyable_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Seed())
            .Add(c => c.Columns, Columns())
            .Add(c => c.Copyable, true));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        AngleSharp.Dom.IElement? copyBtn = cut.FindAll("button")
            .FirstOrDefault(b => b.GetAttribute("aria-label") == "Copy to clipboard");

        copyBtn.Should().NotBeNull("Copy button must render in the toolbar when Copyable=true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Copy_Button_Hidden_When_Copyable_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Seed())
            .Add(c => c.Columns, Columns())
            .Add(c => c.Copyable, false));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        cut.FindAll("button")
            .Any(b => b.GetAttribute("aria-label") == "Copy to clipboard")
            .Should().BeFalse("Copy button must not render when Copyable=false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Copy_Click_Fires_OnCopy_With_Filtered_Items_And_Serialised_Payloads(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        BOBDataGridCopyEventArgs<Person>? captured = null;

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Seed())
            .Add(c => c.Columns, Columns())
            .Add(c => c.Copyable, true)
            .Add(c => c.OnCopy, EventCallback.Factory.Create<BOBDataGridCopyEventArgs<Person>>(this, a => captured = a)));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        AngleSharp.Dom.IElement copyBtn = cut.FindAll("button")
            .First(b => b.GetAttribute("aria-label") == "Copy to clipboard");
        await cut.InvokeAsync(() => copyBtn.Click());

        captured.Should().NotBeNull("OnCopy must fire after a copy click");
        captured!.Items.Should().HaveCount(2, "all unfiltered rows are part of the copy snapshot");
        captured.Tsv.Should().Contain("Name\tAge", "headers are present in the TSV payload");
        captured.Tsv.Should().Contain("Alice\t30").And.Contain("Bob\t25");
        captured.Html.Should().StartWith("<table");
        captured.Html.Should().Contain("<td>Alice</td>");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Copy_Snapshot_Respects_Active_Filter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        BOBDataGridCopyEventArgs<Person>? captured = null;

        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.Items, Seed())
            .Add(c => c.Columns, Columns())
            .Add(c => c.Filterable, true)
            .Add(c => c.Copyable, true)
            .Add(c => c.OnCopy, EventCallback.Factory.Create<BOBDataGridCopyEventArgs<Person>>(this, a => captured = a)));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 2, TimeSpan.FromSeconds(2));

        // Apply a free-text filter that matches only "Alice".
        AngleSharp.Dom.IElement filterInput = cut.Find("input[type='search'], .bob-dc__filter input, input.bob-input__field");
        await cut.InvokeAsync(() => filterInput.Input("Alice"));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1, TimeSpan.FromSeconds(2));

        AngleSharp.Dom.IElement copyBtn = cut.FindAll("button")
            .First(b => b.GetAttribute("aria-label") == "Copy to clipboard");
        await cut.InvokeAsync(() => copyBtn.Click());

        captured.Should().NotBeNull();
        captured!.Items.Should().HaveCount(1, "the copy must reflect the visible (filtered) row set, not the full source");
        captured.Tsv.Should().Contain("Alice");
        captured.Tsv.Should().NotContain("Bob");
    }
}
