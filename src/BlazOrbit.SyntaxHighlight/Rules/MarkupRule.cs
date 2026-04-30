using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Rules;

/// <summary>
/// Matches HTML-like markup tags and tokenizes their contents.
/// </summary>
public sealed class MarkupRule : ITokenRule
{
    private const int MaxTagLength = 10000;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkupRule"/> class.
    /// </summary>
    /// <param name="priority">The rule priority.</param>
    public MarkupRule(int priority = 0) => Priority = priority;

    /// <summary>
    /// Gets the priority of this rule. Higher values are evaluated first.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Attempts to match a markup tag at the specified position.
    /// </summary>
    public TokenMatch? TryMatch(string input, int position, TokenizerContext context)
    {
        if (position >= input.Length || input[position] != '<')
        {
            return null;
        }

        if (position + 1 >= input.Length)
        {
            return null;
        }

        char nextChar = input[position + 1];

        if (nextChar == '!')
        {
            if (position + 4 <= input.Length && input.AsSpan(position, 4).SequenceEqual("<!--"))
            {
                return null;
            }
        }

        if (!char.IsLetter(nextChar) && nextChar != '/')
        {
            return null;
        }

        int closePos = FindTagClose(input, position);
        if (closePos == -1)
        {
            return null;
        }

        List<Token> tokens = ParseTagContent(input, position, closePos + 1);
        int totalLength = closePos + 1 - position;

        return new TokenMatch(TokenType.Tag, position, totalLength, tokens);
    }

    private static int FindTagClose(string input, int position)
    {
        int pos = position + 1;
        int maxPos = Math.Min(input.Length, position + MaxTagLength);
        int depth = 0;
        bool inDoubleQuote = false;
        bool inSingleQuote = false;

        while (pos < maxPos)
        {
            char c = input[pos];

            if (!inDoubleQuote && !inSingleQuote)
            {
                if (c == '"')
                {
                    inDoubleQuote = true;
                }
                else if (c == '\'')
                {
                    inSingleQuote = true;
                }
                else if (c == '(')
                {
                    depth++;
                }
                else if (c == ')')
                {
                    depth--;
                }
                else if (c == '>' && depth <= 0)
                {
                    return pos;
                }
            }
            else if (inDoubleQuote)
            {
                if (c == '"')
                {
                    inDoubleQuote = false;
                }
            }
            else if (inSingleQuote)
            {
                if (c == '\'')
                {
                    inSingleQuote = false;
                }
            }

            pos++;
        }

        return -1;
    }

    private static bool IsAttributeNameChar(char c)
        => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ':' || c == '@' || c == '.';

    private static bool IsTagNameChar(char c)
        => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.' || c == ':';

    private static List<Token> ParseTagContent(string input, int start, int end)
    {
        List<Token> tokens = [];
        int pos = start;

        tokens.Add(new Token(TokenType.Punctuation, "<", pos, 1));
        pos++;

        if (pos < end && input[pos] == '/')
        {
            tokens.Add(new Token(TokenType.Punctuation, "/", pos, 1));
            pos++;
        }

        int tagNameStart = pos;
        while (pos < end && IsTagNameChar(input[pos]))
        {
            pos++;
        }

        if (pos > tagNameStart)
        {
            string tagName = input[tagNameStart..pos];
            tokens.Add(new Token(TokenType.TagName, tagName, tagNameStart, tagName.Length));
        }

        while (pos < end - 1)
        {
            while (pos < end - 1 && char.IsWhiteSpace(input[pos]))
            {
                tokens.Add(new Token(TokenType.Text, input[pos].ToString(), pos, 1));
                pos++;
            }

            if (pos >= end - 1 || input[pos] == '>' || input[pos] == '/')
            {
                break;
            }

            int attrStart = pos;

            if (input[pos] is '"' or '\'')
            {
                char quote = input[pos];
                pos++;
                while (pos < end - 1 && input[pos] != quote)
                {
                    pos++;
                }

                if (pos < end - 1)
                {
                    pos++;
                }

                string attrValue = input[attrStart..pos];
                tokens.Add(new Token(TokenType.AttributeValue, attrValue, attrStart, attrValue.Length));
                continue;
            }

            int attrNameStart = pos;
            while (pos < end - 1 && IsAttributeNameChar(input[pos]))
            {
                pos++;
            }

            if (pos > attrNameStart)
            {
                string attrName = input[attrNameStart..pos];
                tokens.Add(new Token(TokenType.AttributeName, attrName, attrNameStart, attrName.Length));
            }
            else
            {
                pos++;
                continue;
            }

            while (pos < end - 1 && char.IsWhiteSpace(input[pos]))
            {
                tokens.Add(new Token(TokenType.Text, input[pos].ToString(), pos, 1));
                pos++;
            }

            if (pos < end - 1 && input[pos] == '=')
            {
                tokens.Add(new Token(TokenType.Punctuation, "=", pos, 1));
                pos++;

                while (pos < end - 1 && char.IsWhiteSpace(input[pos]))
                {
                    tokens.Add(new Token(TokenType.Text, input[pos].ToString(), pos, 1));
                    pos++;
                }

                if (pos < end - 1)
                {
                    int valueStart = pos;
                    char quoteChar = input[pos];

                    if (quoteChar is '"' or '\'')
                    {
                        pos++;
                        int parenDepth = 0;

                        while (pos < end - 1)
                        {
                            char c = input[pos];

                            if (c == '(')
                            {
                                parenDepth++;
                            }
                            else if (c == ')')
                            {
                                parenDepth--;
                            }
                            else if (c == quoteChar && parenDepth <= 0)
                            {
                                break;
                            }

                            pos++;
                        }

                        if (pos < end - 1 && input[pos] == quoteChar)
                        {
                            pos++;
                        }

                        string attrValue = input[valueStart..pos];
                        tokens.Add(new Token(TokenType.AttributeValue, attrValue, valueStart, attrValue.Length));
                    }
                    else
                    {
                        while (pos < end - 1 && !char.IsWhiteSpace(input[pos]) && input[pos] != '>' && input[pos] != '/')
                        {
                            pos++;
                        }

                        if (pos > valueStart)
                        {
                            string attrValue = input[valueStart..pos];
                            tokens.Add(new Token(TokenType.AttributeValue, attrValue, valueStart, attrValue.Length));
                        }
                    }
                }
            }
        }

        while (pos < end && char.IsWhiteSpace(input[pos]))
        {
            tokens.Add(new Token(TokenType.Text, input[pos].ToString(), pos, 1));
            pos++;
        }

        if (pos < end && input[pos] == '/')
        {
            tokens.Add(new Token(TokenType.Punctuation, "/", pos, 1));
            pos++;
        }

        if (pos < end && input[pos] == '>')
        {
            tokens.Add(new Token(TokenType.Punctuation, ">", pos, 1));
        }

        return tokens;
    }
}
