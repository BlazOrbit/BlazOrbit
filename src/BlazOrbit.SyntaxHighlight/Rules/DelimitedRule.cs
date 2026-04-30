using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Rules;

/// <summary>
/// Matches tokens delimited by start and end sequences, such as strings or comments.
/// </summary>
public sealed class DelimitedRule : ITokenRule
{
    private readonly string _end;
    private readonly string? _escape;
    private readonly bool _multiline;
    private readonly string _start;
    private readonly TokenType _tokenType;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelimitedRule"/> class.
    /// </summary>
    public DelimitedRule(
        TokenType tokenType,
        string start,
        string end,
        string? escape = null,
        bool multiline = true,
        int priority = 0)
    {
        _tokenType = tokenType;
        _start = start;
        _end = end;
        _escape = escape;
        _multiline = multiline;
        Priority = priority;
    }

    /// <summary>
    /// Gets the priority of this rule. Higher values are evaluated first.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Attempts to match a delimited token at the specified position.
    /// </summary>
    public TokenMatch? TryMatch(string input, int position, TokenizerContext context)
    {
        if (position + _start.Length > input.Length)
        {
            return null;
        }

        StringComparison comparison = context.CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        if (!input.AsSpan(position).StartsWith(_start, comparison))
        {
            return null;
        }

        int searchStart = position + _start.Length;
        int endPos = FindEndPosition(input, searchStart, comparison, context.CaseSensitive);

        if (endPos == -1)
        {
            endPos = _multiline ? input.Length : FindLineEnd(input, searchStart);
        }
        else
        {
            endPos += _end.Length;
        }

        int length = endPos - position;

        return length <= 0 ? null : new TokenMatch(_tokenType, position, length);
    }

    private static int FindLineEnd(string input, int start)
    {
        for (int i = start; i < input.Length; i++)
        {
            if (input[i] is '\n' or '\r')
            {
                return i;
            }
        }

        return input.Length;
    }

    private int FindEndPosition(string input, int searchStart, StringComparison comparison, bool caseSensitive)
    {
        int pos = searchStart;
        int maxIterations = input.Length - searchStart + 1;
        int iterations = 0;

        while (pos < input.Length)
        {
            if (++iterations > maxIterations)
            {
                return -1;
            }

            if (!_multiline && (input[pos] == '\n' || input[pos] == '\r'))
            {
                return -1;
            }

            if (_escape != null && _escape.Length > 0 && pos + _escape.Length <= input.Length)
            {
                bool isEscape = true;
                for (int i = 0; i < _escape.Length && isEscape; i++)
                {
                    isEscape = caseSensitive ? input[pos + i] == _escape[i] : char.ToLowerInvariant(input[pos + i]) == char.ToLowerInvariant(_escape[i]);
                }

                if (isEscape)
                {
                    pos += _escape.Length;
                    if (pos < input.Length)
                    {
                        pos++;
                    }

                    continue;
                }
            }

            if (pos + _end.Length <= input.Length &&
                input.AsSpan(pos).StartsWith(_end, comparison))
            {
                return pos;
            }

            pos++;
        }

        return -1;
    }
}
