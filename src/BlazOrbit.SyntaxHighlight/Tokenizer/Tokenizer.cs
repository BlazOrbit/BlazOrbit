using BlazOrbit.SyntaxHighlight.Rules;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tokenizer;

/// <summary>
/// Tokenizes input text using a prioritized list of token rules.
/// </summary>
public sealed class Tokenizer
{
    private readonly TokenizerContext _context;
    private readonly List<ITokenRule> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tokenizer"/> class.
    /// </summary>
    /// <param name="rules">The token rules to apply.</param>
    /// <param name="context">The tokenizer context.</param>
    public Tokenizer(IEnumerable<ITokenRule> rules, TokenizerContext context)
    {
        _rules = [.. rules.OrderByDescending(r => r.Priority)];
        _context = context;
    }

    /// <summary>
    /// Tokenizes the specified input into a list of tokens.
    /// </summary>
    /// <param name="input">The source text to tokenize.</param>
    /// <returns>A read-only list of tokens.</returns>
    public IReadOnlyList<Token> Tokenize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        List<Token> tokens = [];
        int position = 0;
        int textStart = 0;
        int lastPosition = -1;
        int stuckCount = 0;

        while (position < input.Length)
        {
            if (position == lastPosition)
            {
                stuckCount++;
                if (stuckCount > 10)
                {
                    position++;
                    textStart = position;
                    stuckCount = 0;
                    continue;
                }
            }
            else
            {
                lastPosition = position;
                stuckCount = 0;
            }

            TokenMatch? match = TryMatchRule(input, position);

            if (match.HasValue && match.Value.Length > 0)
            {
                if (position > textStart)
                {
                    string textValue = input[textStart..position];
                    tokens.Add(Token.Text(textValue, textStart));
                }

                if (match.Value.HasNestedTokens)
                {
                    foreach (Token nestedToken in match.Value.NestedTokens!)
                    {
                        tokens.Add(nestedToken with
                        {
                            StartIndex = nestedToken.StartIndex + match.Value.StartIndex
                        });
                    }
                }
                else
                {
                    string value = input.Substring(match.Value.StartIndex, match.Value.Length);
                    tokens.Add(new Token(match.Value.Type, value, match.Value.StartIndex, match.Value.Length));
                }

                position = match.Value.EndIndex;
                textStart = position;
            }
            else
            {
                position++;
            }
        }

        if (position > textStart)
        {
            string textValue = input[textStart..position];
            tokens.Add(Token.Text(textValue, textStart));
        }

        return tokens;
    }

    private TokenMatch? TryMatchRule(string input, int position)
    {
        foreach (ITokenRule rule in _rules)
        {
            try
            {
                TokenMatch? match = rule.TryMatch(input, position, _context);
                if (match.HasValue && match.Value.Length > 0)
                {
                    return match;
                }
            }
            catch
            {
                continue;
            }
        }

        return null;
    }
}
