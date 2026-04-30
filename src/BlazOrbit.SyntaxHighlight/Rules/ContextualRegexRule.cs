using BlazOrbit.SyntaxHighlight.Tokens;
using System.Text.RegularExpressions;

namespace BlazOrbit.SyntaxHighlight.Rules;

/// <summary>
/// Matches a regex pattern only when a context predicate is satisfied.
/// </summary>
public sealed class ContextualRegexRule : ITokenRule
{
    private readonly Func<string, int, bool> _contextPredicate;
    private readonly Regex _regex;
    private readonly bool _requireWordBoundary;
    private readonly TokenType _tokenType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextualRegexRule"/> class.
    /// </summary>
    public ContextualRegexRule(
        TokenType tokenType,
        string pattern,
        Func<string, int, bool> contextPredicate,
        bool requireWordBoundary = false,
        int priority = 0)
    {
        _tokenType = tokenType;
        _contextPredicate = contextPredicate;
        _requireWordBoundary = requireWordBoundary;
        Priority = priority;

        _regex = new Regex(
            $"\\G({pattern})",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(100));
    }

    /// <summary>
    /// Gets the priority of this rule. Higher values are evaluated first.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Attempts to match a contextual regex pattern at the specified position.
    /// </summary>
    public TokenMatch? TryMatch(string input, int position, TokenizerContext context)
    {
        if (!_contextPredicate(input, position))
        {
            return null;
        }

        if (_requireWordBoundary && position > 0 && IsWordChar(input[position - 1]))
        {
            return null;
        }

        try
        {
            Match match = _regex.Match(input, position);
            if (!match.Success || match.Index != position)
            {
                return null;
            }

            if (_requireWordBoundary)
            {
                int endPos = position + match.Length;
                if (endPos < input.Length && IsWordChar(input[endPos]))
                {
                    return null;
                }
            }

            return new TokenMatch(_tokenType, position, match.Length);
        }
        catch (RegexMatchTimeoutException)
        {
            return null;
        }
    }

    private static bool IsWordChar(char c)
        => char.IsLetterOrDigit(c) || c == '_';
}
