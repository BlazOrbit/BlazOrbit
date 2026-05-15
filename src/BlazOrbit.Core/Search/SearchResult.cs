namespace BlazOrbit.Components;

/// <summary>Single hit returned by the BlazOrbit search pipeline.</summary>
/// <typeparam name="T">Item type being searched.</typeparam>
/// <param name="Item">Matched item.</param>
/// <param name="Score">Relevance score; higher is better.</param>
/// <param name="MatchType">How the item matched the query.</param>
public readonly record struct SearchResult<T>(T Item, double Score, SearchMatchType MatchType);

/// <summary>Classifies how a <see cref="SearchResult{T}"/> matched the input query.</summary>
public enum SearchMatchType
{
    /// <summary>Whole input equals the candidate value.</summary>
    Exact,

    /// <summary>Candidate starts with the input.</summary>
    StartsWith,

    /// <summary>Input matches the start of a word inside the candidate.</summary>
    WordStart,

    /// <summary>Input appears somewhere inside the candidate.</summary>
    Contains,

    /// <summary>Input matches the leading characters of consecutive words (acronym).</summary>
    Acronym,

    /// <summary>Approximate (edit-distance / typo-tolerant) match.</summary>
    Fuzzy
}
