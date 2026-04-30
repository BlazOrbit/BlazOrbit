using BlazOrbit.Components;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Direct unit tests for <see cref="SearchAlgorithms" />.
/// Pins the matrix of <see cref="SearchMode" /> × query behaviours used by
/// Dropdown / Tree / autocomplete surfaces.
/// </summary>
[Trait("Core", "SearchAlgorithms")]
public class SearchAlgorithmsTests
{
    private static readonly string[] Fruits =
    [
        "Apple",
        "apricot",
        "Banana",
        "blueberry",
        "Cherry",
        "date",
        "Elderberry"
    ];

    // ─────────── Empty / whitespace query ───────────

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Empty_Or_Whitespace_Query_Should_Return_All_Items_With_Exact_Match(string? query)
    {
        SearchResult<string>[] results = SearchAlgorithms
            .Search(Fruits, query!, x => x)
            .ToArray();

        results.Length.Should().Be(Fruits.Length);
        results.Should().OnlyContain(r => r.MatchType == SearchMatchType.Exact && r.Score == 1.0);
    }

    // ─────────── StartsWith mode ───────────

    [Fact]
    public void StartsWith_Should_Match_Case_Insensitively()
    {
        SearchResult<string>[] results = SearchAlgorithms
            .Search(Fruits, "AP", x => x, SearchMode.StartsWith)
            .ToArray();

        results.Select(r => r.Item).Should().BeEquivalentTo(new[] { "Apple", "apricot" });
        results.Should().OnlyContain(r => r.MatchType == SearchMatchType.StartsWith);
    }

    [Fact]
    public void StartsWith_Should_Return_Empty_When_No_Prefix_Matches()
    {
        SearchResult<string>[] results = SearchAlgorithms
            .Search(Fruits, "xyz", x => x, SearchMode.StartsWith)
            .ToArray();

        results.Should().BeEmpty();
    }

    [Fact]
    public void StartsWith_Should_Trim_Query_Whitespace()
    {
        SearchResult<string>[] results = SearchAlgorithms
            .Search(Fruits, "  ap  ", x => x, SearchMode.StartsWith)
            .ToArray();

        results.Select(r => r.Item).Should().BeEquivalentTo(new[] { "Apple", "apricot" });
    }

    // ─────────── Contains mode ───────────

    [Fact]
    public void Contains_Should_Match_Substring_Case_Insensitively()
    {
        SearchResult<string>[] results = SearchAlgorithms
            .Search(Fruits, "err", x => x, SearchMode.Contains)
            .ToArray();

        results.Select(r => r.Item).Should().BeEquivalentTo(new[] { "Cherry", "Elderberry", "blueberry" });
        results.Should().OnlyContain(r => r.MatchType == SearchMatchType.Contains);
    }

    [Fact]
    public void Contains_Should_Return_Empty_When_No_Substring_Matches()
    {
        SearchResult<string>[] results = SearchAlgorithms
            .Search(Fruits, "zzz", x => x, SearchMode.Contains)
            .ToArray();

        results.Should().BeEmpty();
    }

    // ─────────── Fuzzy mode ───────────

    [Fact]
    public void Fuzzy_Should_Match_Similar_Strings_Within_Distance_Threshold()
    {
        SearchResult<string>[] results = SearchAlgorithms
            .Search(new[] { "apple" }, "aple", x => x, SearchMode.Fuzzy)
            .ToArray();

        results.Should().HaveCount(1);
        results[0].MatchType.Should().Be(SearchMatchType.Fuzzy);
        results[0].Score.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void Fuzzy_Should_Reject_Strings_Exceeding_Distance_Threshold()
    {
        SearchResult<string>[] results = SearchAlgorithms
            .Search(new[] { "zebra" }, "apple", x => x, SearchMode.Fuzzy)
            .ToArray();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Fuzzy_Should_Order_By_Distance_Ascending()
    {
        string[] items = ["apple", "aaple", "aple"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "apple", x => x, SearchMode.Fuzzy)
            .ToArray();

        results.Should().HaveCountGreaterThan(0);
        results[0].Item.Should().Be("apple");
    }

    [Fact]
    public void Fuzzy_Short_Query_Should_Use_Minimum_Distance_Of_One()
    {
        SearchResult<string>[] results = SearchAlgorithms
            .Search(new[] { "ab" }, "ac", x => x, SearchMode.Fuzzy)
            .ToArray();

        results.Should().HaveCount(1);
    }

    // ─────────── Smart mode (default) ───────────

    [Fact]
    public void Smart_Should_Rank_Exact_Match_Highest()
    {
        string[] items = ["banana", "bananas", "bandana"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "banana", x => x)
            .ToArray();

        results[0].Item.Should().Be("banana");
        results[0].MatchType.Should().Be(SearchMatchType.Exact);
        results[0].Score.Should().Be(1.0);
    }

    [Fact]
    public void Smart_Should_Rank_StartsWith_Above_Contains()
    {
        string[] items = ["cupcake", "bancup"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "cup", x => x)
            .ToArray();

        results.Should().HaveCount(2);
        results[0].Item.Should().Be("cupcake");
        results[0].MatchType.Should().Be(SearchMatchType.StartsWith);
        results[1].Item.Should().Be("bancup");
        results[1].MatchType.Should().Be(SearchMatchType.Contains);
    }

    [Fact]
    public void Smart_Should_Detect_Acronym_Match()
    {
        string[] items = ["Visual Studio Code", "Vim Script", "Other Tool"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "vsc", x => x)
            .ToArray();

        results.Select(r => r.Item).Should().Contain("Visual Studio Code");
        SearchResult<string> vsc = results.First(r => r.Item == "Visual Studio Code");
        vsc.MatchType.Should().BeOneOf(SearchMatchType.Acronym, SearchMatchType.WordStart);
    }

    [Fact]
    public void Smart_Should_Detect_Acronym_With_Underscore_Dot_Separators()
    {
        string[] items = ["hello_world_foo", "hello.world.foo", "hello-world-foo"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "hwf", x => x)
            .ToArray();

        results.Should().HaveCount(3);
    }

    [Fact]
    public void Smart_StartsWith_Score_Should_Favor_Shorter_Texts()
    {
        string[] items = ["apple", "application"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "app", x => x)
            .ToArray();

        results.Should().HaveCount(2);
        results[0].Item.Should().Be("apple");
        results[0].Score.Should().BeGreaterThan(results[1].Score);
    }

    [Fact]
    public void Smart_Contains_Score_Should_Favor_Earlier_Position()
    {
        string[] items = ["xxxabc", "abcxxx"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "abc", x => x)
            .ToArray();

        results.Should().HaveCount(2);
        SearchResult<string> startsWithMatch = results.Single(r => r.Item == "abcxxx");
        SearchResult<string> midMatch = results.Single(r => r.Item == "xxxabc");
        startsWithMatch.Score.Should().BeGreaterThan(midMatch.Score);
    }

    [Fact]
    public void Smart_Should_Fall_Back_To_Fuzzy_For_Typos()
    {
        string[] items = ["banana"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "banaan", x => x)
            .ToArray();

        results.Should().HaveCount(1);
        results[0].MatchType.Should().Be(SearchMatchType.Fuzzy);
    }

    [Fact]
    public void Smart_Should_Skip_Items_Outside_All_Thresholds()
    {
        string[] items = ["abcdef"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "xyz", x => x)
            .ToArray();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Smart_Ties_Should_Break_By_Text_Alphabetically()
    {
        string[] items = ["zeta", "alpha", "mango"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "z", x => x)
            .ToArray();

        results.Should().HaveCount(1);
        results[0].Item.Should().Be("zeta");
    }

    [Fact]
    public void Smart_Equal_Scores_Should_Order_Alphabetically()
    {
        string[] items = ["charlie team", "bravo team", "alpha team"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "t", x => x)
            .ToArray();

        results.Should().HaveCount(3);
        results[0].Item.Should().Be("alpha team");
        results[1].Item.Should().Be("bravo team");
        results[2].Item.Should().Be("charlie team");
    }

    // ─────────── Case sensitivity / normalization ───────────

    [Fact]
    public void Case_Should_Not_Affect_Matching()
    {
        string[] items = ["APPLE", "apple", "Apple"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "APPLE", x => x)
            .ToArray();

        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.MatchType == SearchMatchType.Exact);
    }

    [Fact]
    public void Diacritics_Should_Not_Be_Normalized()
    {
        string[] items = ["café", "cafe"];

        SearchResult<string>[] results = SearchAlgorithms
            .Search(items, "cafe", x => x, SearchMode.Contains)
            .ToArray();

        results.Should().HaveCount(1);
        results[0].Item.Should().Be("cafe");
    }

    // ─────────── Generic item / custom selector ───────────

    [Fact]
    public void Should_Use_Custom_Text_Selector_For_Complex_Types()
    {
        Person[] items =
        [
            new("Alice", 30),
            new("Bob", 25),
            new("Charlie", 35)
        ];

        SearchResult<Person>[] results = SearchAlgorithms
            .Search(items, "ali", p => p.Name, SearchMode.StartsWith)
            .ToArray();

        results.Should().HaveCount(1);
        results[0].Item.Name.Should().Be("Alice");
    }

    [Fact]
    public void SearchResult_Should_Carry_Item_Reference()
    {
        Person alice = new("Alice", 30);
        SearchResult<Person>[] results = SearchAlgorithms
            .Search(new[] { alice }, "ali", p => p.Name, SearchMode.StartsWith)
            .ToArray();

        results[0].Item.Should().BeSameAs(alice);
    }

    private sealed record Person(string Name, int Age);
}
