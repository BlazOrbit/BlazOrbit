using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Rules;

/// <summary>
/// Matches an exact character sequence.
/// </summary>
public sealed class SequenceRule : ITokenRule
{
    private readonly string _sequence;
    private readonly TokenType _tokenType;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceRule"/> class.
    /// </summary>
    public SequenceRule(TokenType tokenType, string sequence, int priority = 0)
    {
        _tokenType = tokenType;
        _sequence = sequence;
        Priority = priority;
    }

    /// <summary>
    /// Gets the priority of this rule. Higher values are evaluated first.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Attempts to match the sequence at the specified position.
    /// </summary>
    public TokenMatch? TryMatch(string input, int position, TokenizerContext context)
    {
        StringComparison comparison = context.CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        return input.AsSpan(position).StartsWith(_sequence, comparison) ? new TokenMatch(_tokenType, position, _sequence.Length) : null;
    }
}
