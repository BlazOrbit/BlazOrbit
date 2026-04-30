using BlazOrbit.SyntaxHighlight.Tokens;
using System.Text;

namespace BlazOrbit.SyntaxHighlight.Rendering;

/// <summary>
/// Renders a list of tokens as HTML with syntax highlighting spans.
/// </summary>
public sealed class HtmlRenderer
{
    private const string CssPrefix = "SH";

    /// <summary>
    /// Renders the specified tokens as highlighted HTML.
    /// </summary>
    /// <param name="tokens">The tokens to render.</param>
    /// <param name="options">The render options, or <see langword="null"/> to use defaults.</param>
    /// <returns>The highlighted HTML string.</returns>
    public string Render(IReadOnlyList<Token> tokens, HtmlRenderOptions? options = null)
    {
        options ??= HtmlRenderOptions.Default;

        StringBuilder sb = new();

        if (options.IncludeStyles)
        {
            sb.AppendLine(GenerateStyles(options));
        }

        sb.Append($"<pre class=\"{CssPrefix}-container\">");
        sb.Append($"<code class=\"{CssPrefix}-code\">");

        foreach (Token token in tokens)
        {
            string escapedValue = EscapeHtml(token.Value);
            string className = GetClassName(token.Type);

            if (token.Type == TokenType.Text)
            {
                sb.Append(escapedValue);
            }
            else
            {
                sb.Append($"<span class=\"{className}\">{escapedValue}</span>");
            }
        }

        sb.Append("</code></pre>");

        return sb.ToString();
    }

    private static string EscapeHtml(string text)
    {
        StringBuilder sb = new(text.Length);
        foreach (char c in text)
        {
            sb.Append(c switch
            {
                '<' => "&lt;",
                '>' => "&gt;",
                '&' => "&amp;",
                '"' => "&quot;",
                '\'' => "&#39;",
                _ => c
            });
        }

        return sb.ToString();
    }

    private static string GenerateStyles(HtmlRenderOptions options)
    {
        StringBuilder sb = new();
        sb.AppendLine("<style>");
        sb.AppendLine($".{CssPrefix}-container {{");
        sb.AppendLine($"  background-color: {options.BackgroundColor};");
        sb.AppendLine($"  color: {options.DefaultColor};");
        sb.AppendLine($"  font-family: {options.FontFamily};");
        sb.AppendLine($"  font-size: {options.FontSize};");
        sb.AppendLine("  padding: 1em;");
        sb.AppendLine("  border-radius: 0px;");
        sb.AppendLine("  overflow-x: auto;");
        sb.AppendLine("  margin: 0;");
        sb.AppendLine("}");
        sb.AppendLine($".{CssPrefix}-code {{");
        sb.AppendLine("  display: block;");
        sb.AppendLine("  white-space: pre;");
        sb.AppendLine("  margin: 0;");
        sb.AppendLine("}");

        foreach ((TokenType type, string color) in options.TokenColors)
        {
            string className = GetClassName(type);
            sb.AppendLine($".{className} {{ color: {color}; }}");
        }

        sb.AppendLine("</style>");
        return sb.ToString();
    }

    private static string GetClassName(TokenType type)
    {
        return type switch
        {
            TokenType.Keyword => $"{CssPrefix}-kw",
            TokenType.ControlKeyword => $"{CssPrefix}-ckw",
            TokenType.Type => $"{CssPrefix}-type",
            TokenType.String => $"{CssPrefix}-str",
            TokenType.VerbatimString => $"{CssPrefix}-vstr",
            TokenType.InterpolatedString => $"{CssPrefix}-istr",
            TokenType.Char => $"{CssPrefix}-chr",
            TokenType.Number => $"{CssPrefix}-num",
            TokenType.Comment => $"{CssPrefix}-cmt",
            TokenType.Operator => $"{CssPrefix}-op",
            TokenType.Punctuation => $"{CssPrefix}-punc",
            TokenType.Method => $"{CssPrefix}-mth",
            TokenType.Property => $"{CssPrefix}-prop",
            TokenType.Field => $"{CssPrefix}-fld",
            TokenType.Parameter => $"{CssPrefix}-param",
            TokenType.Variable => $"{CssPrefix}-var",
            TokenType.Namespace => $"{CssPrefix}-ns",
            TokenType.Directive => $"{CssPrefix}-dir",
            TokenType.PreprocessorDirective => $"{CssPrefix}-pdir",
            TokenType.Attribute => $"{CssPrefix}-attr",
            TokenType.Tag => $"{CssPrefix}-tag",
            TokenType.TagName => $"{CssPrefix}-tname",
            TokenType.AttributeName => $"{CssPrefix}-aname",
            TokenType.AttributeValue => $"{CssPrefix}-aval",
            TokenType.CssSelector => $"{CssPrefix}-csel",
            TokenType.CssProperty => $"{CssPrefix}-cprop",
            TokenType.CssValue => $"{CssPrefix}-cval",
            TokenType.CssUnit => $"{CssPrefix}-cunit",
            TokenType.CssPseudo => $"{CssPrefix}-cpseudo",
            TokenType.RazorDelimiter => $"{CssPrefix}-rdel",
            TokenType.RazorExpression => $"{CssPrefix}-rexp",
            TokenType.RazorCodeBlock => $"{CssPrefix}-rblk",
            _ => $"{CssPrefix}-txt"
        };
    }
}
