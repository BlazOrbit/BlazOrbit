using BlazOrbit.SyntaxHighlight.Builder;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Languages;

/// <summary>
/// Provides the <see cref="LanguageDefinition"/> for CSS syntax highlighting.
/// </summary>
public static class CssLanguage
{
    /// <summary>
    /// Gets the singleton CSS language definition instance.
    /// </summary>
    public static LanguageDefinition Instance => field ??= Create();

    private static LanguageDefinition Create()
    {
        return LanguageDefinition.Create("css")
            .CaseSensitive(false)

            // Comments
            .AddBlockComment("/*", "*/", priority: 1000)

            // Strings
            .AddString("\"", "\"", escape: "\\", priority: 999)
            .AddString("'", "'", escape: "\\", priority: 998)

            // URLs
            .AddPattern(TokenType.String, @"url\([^)]+\)", priority: 997)

            // At-rules (use sequences because @ is not a word char)
            .AddSequences(TokenType.Directive, [
                "@import", "@media", "@font-face", "@keyframes", "@supports",
                "@page", "@namespace", "@charset", "@viewport", "@counter-style",
                "@font-feature-values", "@property", "@layer", "@container",
                "@scope", "@starting-style"
            ], priority: 900)

            // Pseudo-elements and pseudo-classes
            .AddPattern(TokenType.CssPseudo, @"::[\w-]+", priority: 850)
            .AddPattern(TokenType.CssPseudo, @":[\w-]+(?:\([^)]*\))?", priority: 849)

            // ID selectors
            .AddPattern(TokenType.CssSelector, @"#[\w-]+", priority: 800)

            // Class selectors
            .AddPattern(TokenType.CssSelector, @"\.[\w-]+", priority: 799)

            // Element selectors (simplified)
            .AddKeywords(TokenType.TagName, [
                "html", "head", "body", "div", "span", "p", "a", "img",
                "ul", "ol", "li", "table", "tr", "td", "th", "thead", "tbody",
                "form", "input", "button", "select", "textarea", "label",
                "header", "footer", "nav", "main", "section", "article", "aside",
                "h1", "h2", "h3", "h4", "h5", "h6", "strong", "em", "code", "pre",
                "blockquote", "figure", "figcaption", "video", "audio", "canvas",
                "svg", "path", "circle", "rect", "line", "polygon", "polyline"
            ], priority: 798)

            // Contextual keywords: values after ':'
            .AddContextualKeywords(
                TokenType.CssValue,
                ["flex", "grid"],
                IsAfterColon,
                priority: 751)

            // Contextual keywords: properties after '{' or ';'
            .AddContextualKeywords(
                TokenType.CssProperty,
                ["flex", "grid"],
                IsAfterPropertyDelimiter,
                priority: 750)

            // CSS properties (match any identifier followed by : in property context)
            .AddPattern(TokenType.CssProperty, @"[\w-]+(?=\s*:)", priority: 700)

            // CSS values (common ones - excluding ambiguous)
            .AddKeywords(TokenType.CssValue, [
                "none", "auto", "inherit", "initial", "unset", "revert",
                "block", "inline", "inline-block", "inline-flex", "inline-grid",
                "absolute", "relative", "fixed", "sticky", "static",
                "hidden", "visible", "scroll", "clip",
                "solid", "dashed", "dotted", "double", "groove", "ridge",
                "center", "left", "right", "top", "bottom", "middle",
                "bold", "normal", "italic", "uppercase", "lowercase", "capitalize",
                "row", "column", "wrap", "nowrap", "space-between", "space-around",
                "stretch", "baseline", "start", "end", "flex-start", "flex-end",
                "pointer", "default", "not-allowed", "grab", "grabbing",
                "transparent", "currentColor"
            ], priority: 699)

            // Colors
            .AddContextualPattern(
                TokenType.CssValue,
                @"#[0-9a-fA-F]{3,8}\b",
                IsAfterColon,
                priority: 801)
            .AddPattern(TokenType.CssValue, @"(?:rgb|rgba|hsl|hsla|hwb|lab|lch|oklab|oklch|color)\([^)]+\)", priority: 697)
            .AddKeywords(TokenType.CssValue, [
                "black", "white", "red", "green", "blue", "yellow", "cyan", "magenta",
                "gray", "grey", "orange", "purple", "pink", "brown", "navy", "teal",
                "aqua", "lime", "olive", "maroon", "silver", "fuchsia"
            ], priority: 696)

            // Units Units (% doesn't need word boundary, others do)
            .AddPattern(TokenType.CssUnit, @"\d+\.?\d*%", priority: 651)
            .AddPattern(TokenType.CssUnit, @"\d+\.?\d*(?:px|em|rem|vh|vw|vmin|vmax|ch|ex|cm|mm|in|pt|pc|deg|rad|turn|s|ms|fr)\b", priority: 650)

            // Numbers
            .AddPattern(TokenType.Number, @"\d+\.?\d*", requireWordBoundary: true, priority: 600)

            // Variables
            .AddPattern(TokenType.Variable, @"--[\w-]+", priority: 550)
            .AddPattern(TokenType.Variable, @"var\(--[\w-]+\)", priority: 549)

            // Important
            .AddPattern(TokenType.Keyword, @"!important\b", priority: 500)

            // Operators and punctuation
            .AddOperators([">", "+", "~", "*", "=", "^=", "$=", "*=", "|=", "~="], priority: 400)
            .AddPunctuation("{}[]();:,.", priority: 399)

            .Build();
    }

    private static bool IsAfterColon(string input, int position)
    {
        for (int i = position - 1; i >= 0; i--)
        {
            char c = input[i];
            if (char.IsWhiteSpace(c))
            {
                continue;
            }

            return c == ':';
        }

        return false;
    }

    private static bool IsAfterPropertyDelimiter(string input, int position)
    {
        for (int i = position - 1; i >= 0; i--)
        {
            char c = input[i];
            if (char.IsWhiteSpace(c))
            {
                continue;
            }

            return c is '{' or ';';
        }

        return true;
    }
}
