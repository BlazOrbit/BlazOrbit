namespace BlazOrbit.SyntaxHighlight.Tokens;

/// <summary>
/// Represents the result of a token rule match attempt.
/// </summary>
public readonly record struct TokenMatch(
    TokenType Type,
    int StartIndex,
    int Length,
    IReadOnlyList<Token>? NestedTokens = null
)
{
    /// <summary>
    /// Gets the end index of the match in the source text.
    /// </summary>
    public int EndIndex => StartIndex + Length;

    /// <summary>
    /// Gets a value indicating whether the match contains nested tokens.
    /// </summary>
    public bool HasNestedTokens => NestedTokens is { Count: > 0 };
}
