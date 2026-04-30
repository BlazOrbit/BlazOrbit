using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Rules;

/// <summary>
/// Matches balanced delimited blocks (e.g., braces) with optional nested tokenization.
/// </summary>
public sealed class BalancedRule : ITokenRule
{
    private readonly char _close;
    private readonly Func<string, IReadOnlyList<Token>>? _innerTokenizer;
    private readonly int _maxDepth;
    private readonly char _open;
    private readonly string _prefix;
    private readonly TokenType _tokenType;

    /// <summary>
    /// Initializes a new instance of the <see cref="BalancedRule"/> class.
    /// </summary>
    public BalancedRule(
        TokenType tokenType,
        string prefix,
        char open,
        char close,
        Func<string, IReadOnlyList<Token>>? innerTokenizer = null,
        int priority = 0,
        int maxDepth = 100)
    {
        _tokenType = tokenType;
        _prefix = prefix;
        _open = open;
        _close = close;
        _innerTokenizer = innerTokenizer;
        Priority = priority;
        _maxDepth = maxDepth;
    }

    /// <summary>
    /// Gets the priority of this rule. Higher values are evaluated first.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Attempts to match a balanced block at the specified position.
    /// </summary>
    public TokenMatch? TryMatch(string input, int position, TokenizerContext context)
    {
        if (position + _prefix.Length >= input.Length)
        {
            return null;
        }

        if (!input.AsSpan(position).StartsWith(_prefix))
        {
            return null;
        }

        int searchStart = position + _prefix.Length;

        while (searchStart < input.Length && char.IsWhiteSpace(input[searchStart]))
        {
            searchStart++;
        }

        if (searchStart >= input.Length || input[searchStart] != _open)
        {
            return null;
        }

        int openPos = searchStart;
        int depth = 1;
        int pos = openPos + 1;
        bool inString = false;
        bool inChar = false;
        char stringChar = '\0';
        bool escape = false;

        int maxIterations = input.Length - position;
        int iterations = 0;

        while (pos < input.Length && depth > 0)
        {
            if (++iterations > maxIterations)
            {
                return null;
            }

            if (depth > _maxDepth)
            {
                return null;
            }

            char c = input[pos];

            if (escape)
            {
                escape = false;
                pos++;
                continue;
            }

            if (c == '\\' && (inString || inChar))
            {
                escape = true;
                pos++;
                continue;
            }

            if (inString)
            {
                if (c == stringChar)
                {
                    inString = false;
                }

                pos++;
                continue;
            }

            if (inChar)
            {
                if (c == '\'')
                {
                    inChar = false;
                }

                pos++;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                stringChar = '"';
                pos++;
                continue;
            }

            if (c == '\'')
            {
                inChar = true;
                pos++;
                continue;
            }

            if (c == '/' && pos + 1 < input.Length)
            {
                if (input[pos + 1] == '/')
                {
                    while (pos < input.Length && input[pos] != '\n')
                    {
                        pos++;
                    }

                    continue;
                }

                if (input[pos + 1] == '*')
                {
                    pos += 2;
                    while (pos + 1 < input.Length && !(input[pos] == '*' && input[pos + 1] == '/'))
                    {
                        pos++;
                    }

                    pos += 2;
                    continue;
                }
            }

            if (c == _open)
            {
                depth++;
            }
            else if (c == _close)
            {
                depth--;
            }

            pos++;
        }

        if (depth != 0)
        {
            return null;
        }

        int length = pos - position;
        IReadOnlyList<Token>? nestedTokens = null;

        if (_innerTokenizer != null && openPos + 1 < pos - 1)
        {
            int innerStart = openPos + 1;
            int innerLength = pos - 1 - innerStart;
            if (innerLength > 0)
            {
                string innerContent = input.Substring(innerStart, innerLength);
                nestedTokens = _innerTokenizer(innerContent);
            }
        }

        return new TokenMatch(_tokenType, position, length, nestedTokens);
    }
}
