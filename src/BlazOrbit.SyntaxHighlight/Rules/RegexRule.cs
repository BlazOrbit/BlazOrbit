using BlazOrbit.SyntaxHighlight.Tokens;
using System.Text.RegularExpressions;

namespace BlazOrbit.SyntaxHighlight.Rules;

/// <summary>
/// Matches tokens using a regular expression pattern.
/// </summary>
public sealed class RegexRule : ITokenRule
{
    private readonly Regex _regex;
    private readonly bool _requireWordBoundary;
    private readonly TokenType _tokenType;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegexRule"/> class.
    /// </summary>
    public RegexRule(
        TokenType tokenType,
        string pattern,
        bool requireWordBoundary = false,
        int priority = 0)
    {
        _tokenType = tokenType;
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
    /// Attempts to match a regex pattern at the specified position.
    /// </summary>
    public TokenMatch? TryMatch(string input, int position, TokenizerContext context)
    {
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
