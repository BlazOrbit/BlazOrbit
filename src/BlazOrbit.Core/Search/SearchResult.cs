namespace BlazOrbit.Components;

public readonly record struct SearchResult<T>(T Item, double Score, SearchMatchType MatchType);

public enum SearchMatchType
{
    Exact,
    StartsWith,
    WordStart,
    Contains,
    Acronym,
    Fuzzy
}