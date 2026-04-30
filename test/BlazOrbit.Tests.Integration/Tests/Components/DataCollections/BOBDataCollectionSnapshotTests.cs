using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Snapshots", "BOBDataCollections")]
public class BOBDataCollectionSnapshotTests
{
    private sealed record Person(string Name, int Age);

    private static IEnumerable<Person> Items => [new Person("Alice", 30), new Person("Bob", 25)];

    private static RenderFragment Columns => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
        b.CloseComponent();
        b.OpenComponent<BOBDataColumn<Person>>(3);
        b.AddAttribute(4, "Header", "Age");
        b.AddAttribute(5, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Age.ToString())));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_DataGrid_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Grid_Empty",
                Html = ctx.Render<BOBDataGrid<Person>>(p => p
                    .Add(c => c.Items, [])
                    .Add(c => c.Columns, Columns)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Grid_WithData",
                Html = ctx.Render<BOBDataGrid<Person>>(p => p
                    .Add(c => c.Items, Items)
                    .Add(c => c.Columns, Columns)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Cards_WithData",
                Html = ctx.Render<BOBDataCards<Person>>(p => p
                    .Add(c => c.Items, Items)
                    .Add(c => c.Columns, Columns)).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
