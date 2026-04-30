namespace BlazOrbit.SyntaxHighlight.Tokens;

/// <summary>
/// Represents a single token produced by the tokenizer.
/// </summary>
public readonly record struct Token(
    TokenType Type,
    string Value,
    int StartIndex,
    int Length
)
{
    /// <summary>
    /// Gets the end index of the token in the source text.
    /// </summary>
    public int EndIndex => StartIndex + Length;

    /// <summary>
    /// Creates a text token at the specified start index.
    /// </summary>
    /// <param name="value">The token value.</param>
    /// <param name="startIndex">The start index in the source text.</param>
    /// <returns>A new text token.</returns>
    public static Token Text(string value, int startIndex)
        => new(TokenType.Text, value, startIndex, value.Length);
}
