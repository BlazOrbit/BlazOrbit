using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Integration", "BOBDataGrid")]
public class BOBDataGridDataSourceTests
{
    private sealed record Person(int Id, string Name);

    private static List<Person> Seed => Enumerable.Range(1, 25)
        .Select(i => new Person(i, $"Person {i:D2}"))
        .ToList();

    private static RenderFragment Columns => b =>
    {
        b.OpenComponent<BOBDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Id");
        b.AddAttribute(2, "Property", (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.Id));
        b.CloseComponent();
        b.OpenComponent<BOBDataColumn<Person>>(3);
        b.AddAttribute(4, "Header", "Name");
        b.AddAttribute(5, "Property", (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.Name));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task InMemoryDataSource_Renders_Same_Slice_As_Items(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        InMemoryDataSource<Person> source = new(Seed);
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.DataSource, source)
            .Add(c => c.Columns, Columns)
            .Add(c => c.PageSize, 5));

        cut.WaitForState(() => cut.FindAll("tbody tr").Count >= 5, TimeSpan.FromSeconds(2));

        // Assert - first page of 5 visible rows.
        cut.FindAll("tbody tr").Should().HaveCountGreaterThanOrEqualTo(5);
        cut.Markup.Should().Contain("Person 01");
        cut.Markup.Should().Contain("Person 05");
        cut.Markup.Should().NotContain("Person 06");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task RemoteDataSource_Provider_Receives_DataRequest_With_StartIndex_And_Count(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange - capture the request the grid forwards to the provider.
        DataRequest? captured = null;
        RemoteDataSource<Person> source = new((DataRequest req, CancellationToken _) =>
        {
            captured = req;
            List<Person> page = Seed.Skip(req.StartIndex).Take(req.Count ?? Seed.Count).ToList();
            return Task.FromResult(new DataResult<Person>(page, Seed.Count));
        });

        // Act
        ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.DataSource, source)
            .Add(c => c.Columns, Columns)
            .Add(c => c.PageSize, 10));

        // Wait for the fire-and-forget LoadAsync to complete.
        await Task.Delay(80, Xunit.TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.StartIndex.Should().Be(0);
        captured.Count.Should().Be(10);
        captured.Page.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task RemoteDataSource_TotalCount_Drives_Pagination_Math(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange - server reports 100 total rows; only 10 returned per page.
        RemoteDataSource<Person> source = new((DataRequest req, CancellationToken _) =>
        {
            List<Person> page = Seed.Take(req.Count ?? 10).ToList();
            return Task.FromResult(new DataResult<Person>(page, 100));
        });

        // Act
        IRenderedComponent<BOBDataGrid<Person>> cut = ctx.Render<BOBDataGrid<Person>>(p => p
            .Add(c => c.DataSource, source)
            .Add(c => c.Columns, Columns)
            .Add(c => c.PageSize, 10));

        await Task.Delay(80, Xunit.TestContext.Current.CancellationToken);
        cut.Render();

        // Assert - _BOBInPagination renders when TotalPages > 1; presence of any pagination
        // button is the observable surface for "the grid computed multiple pages".
        cut.FindAll("button").Should().NotBeEmpty();
        cut.Markup.Should().Contain("Person 01");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task InMemoryDataSource_LoadAsync_Honours_StartIndex_And_Count(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Standalone source test - no component wiring; exercises LoadAsync directly.
        InMemoryDataSource<Person> source = new(Seed);
        DataRequest req = new(
            StartIndex: 5,
            Count: 5,
            GlobalFilter: null,
            Sorts: [],
            ColumnFilters: new Dictionary<string, ColumnFilterEntry>());

        DataResult<Person> result = await source.LoadAsync(req, Xunit.TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(25);
        result.Items.Should().HaveCount(5);
        result.Items[0].Id.Should().Be(6);
        result.Items[4].Id.Should().Be(10);
    }
}
