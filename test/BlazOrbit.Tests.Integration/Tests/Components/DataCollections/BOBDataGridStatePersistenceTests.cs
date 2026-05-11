using BlazOrbit.Components;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component State", "BOBDataGrid")]
public class DataCollectionStatePersistenceTests
{
    private sealed record Person(string Name, int Age);

    [Fact]
    public void ToJson_LoadFromJson_RoundTrips_Filter_Sort_Page_ColumnFilters_ColumnOrder()
    {
        DataCollectionState<Person> source = new();
        source.FilterText = "global";
        source.SetColumnFilter("Name", "alice");
        source.SetColumnFilter("Age", "30");
        source.SetColumnOrder(["Age", "Name"]);
        source.ToggleSort("Name");
        source.ToggleSort("Age", append: true);
        source.CurrentPage = 3;
        source.PageSize = 25;

        string json = source.ToJson();

        DataCollectionState<Person> target = new();
        target.LoadFromJson(json);

        target.FilterText.Should().Be("global");
        target.ColumnFilters.Should().ContainKey("Name").WhoseValue.Text.Should().Be("alice");
        target.ColumnFilters.Should().ContainKey("Age").WhoseValue.Text.Should().Be("30");
        target.ColumnOrder.Should().BeEquivalentTo(new[] { "Age", "Name" }, o => o.WithStrictOrdering());
        target.SortDescriptors.Should().HaveCount(2);
        target.SortDescriptors[0].ColumnName.Should().Be("Name");
        target.SortDescriptors[1].ColumnName.Should().Be("Age");
        target.CurrentPage.Should().Be(3);
        target.PageSize.Should().Be(25);
    }

    [Fact]
    public void LoadFromJson_Drops_Whitespace_ColumnFilters()
    {
        DataCollectionState<Person> state = new();
        // Hand-crafted payload that contains a whitespace-only value for one column. The
        // persistence layer treats whitespace as empty and skips the entry on load.
        state.LoadFromJson("""
            {
              "filterText": "",
              "currentPage": 1,
              "pageSize": 20,
              "columnFilters": {
                "Name": { "text": "alice", "operator": 0, "mode": 0 },
                "Age":  { "text": "   ",   "operator": 0, "mode": 0 }
              }
            }
            """);

        state.ColumnFilters.Should().HaveCount(1);
        state.ColumnFilters.Should().ContainKey("Name");
        state.ColumnFilters.Should().NotContainKey("Age");
    }

    [Fact]
    public void LoadFromJson_Silently_Drops_Malformed_Input()
    {
        DataCollectionState<Person> state = new();
        state.FilterText = "preserved";

        state.LoadFromJson("not-json");
        state.LoadFromJson("");
        state.LoadFromJson(null);

        // None of the malformed loads should overwrite the existing state.
        state.FilterText.Should().Be("preserved");
        state.ColumnFilters.Should().BeEmpty();
        state.ColumnOrder.Should().BeEmpty();
    }

    [Fact]
    public void ToJson_Excludes_Selection()
    {
        DataCollectionState<Person> state = new();
        state.SelectItem(new Person("Alice", 30), SelectionMode.Multiple);

        string json = state.ToJson();

        // Selection references live data so we deliberately omit it from the persisted
        // payload — round-tripping the selection across reloads would resurrect items
        // that may have been deleted on the server.
        json.Should().NotContain("Alice");
    }

    [Fact]
    public async Task NullStatePersistence_Returns_Null_And_Drops_Saves()
    {
        IDataCollectionStatePersistence svc = new NullStatePersistence();

        await svc.SaveAsync("k", "v");
        string? loaded = await svc.LoadAsync("k");

        loaded.Should().BeNull();
    }
}
