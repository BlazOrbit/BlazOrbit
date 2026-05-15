namespace BlazOrbit.Components;

/// <summary>Matching strategy used by the BlazOrbit search pipeline.</summary>
public enum SearchMode
{
    /// <summary>Adaptive strategy combining prefix, word-start, contains and fuzzy heuristics.</summary>
    Smart,

    /// <summary>Match only when the candidate starts with the input.</summary>
    StartsWith,

    /// <summary>Match when the candidate contains the input anywhere.</summary>
    Contains,

    /// <summary>Approximate (typo-tolerant) match.</summary>
    Fuzzy
}
