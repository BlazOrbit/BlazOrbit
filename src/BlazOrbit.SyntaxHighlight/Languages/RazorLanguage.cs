using BlazOrbit.SyntaxHighlight.Builder;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Languages;

/// <summary>
/// Provides the <see cref="LanguageDefinition"/> for Razor syntax highlighting.
/// </summary>
public static class RazorLanguage
{
    /// <summary>
    /// Gets the singleton Razor language definition instance.
    /// </summary>
    public static LanguageDefinition Instance => field ??= Create();

    private static LanguageDefinition Create()
    {
        return LanguageDefinition.Create("razor")
            .CaseSensitive()

            // Razor comments (highest priority)
            .AddDelimited(TokenType.Comment, "@*", "*@", priority: 2000)

            // HTML comments
            .AddDelimited(TokenType.Comment, "<!--", "-->", priority: 1999)
            .AddLineComment("//", priority: 1998)

            // Razor directives (simple line-based, no complex parsing)
            .AddSequences(TokenType.Directive, [
                "@page", "@using", "@inject", "@inherits", "@implements",
                "@namespace", "@layout", "@typeparam", "@attribute",
                "@preservewhitespace", "@rendermode", "@formname",
                "@model", "@addTagHelper", "@removeTagHelper", "@tagHelperPrefix"
            ], priority: 1800)

            // Razor control flow keywords
            .AddSequences(TokenType.ControlKeyword, [
                "@if", "@else", "@foreach", "@for", "@while",
                "@switch", "@try", "@catch", "@finally", "@lock", "@await"
            ], priority: 1750)

            // @code and @functions blocks (use sequences because @ is not a word char)
            .AddSequences(TokenType.RazorCodeBlock, ["@code", "@functions"], priority: 1700)

            // Inline code blocks @{ ... }
            .AddBalanced(TokenType.RazorCodeBlock, "@", '{', '}', priority: 1699)

            // Escaped @@
            .AddSequence(TokenType.RazorDelimiter, "@@", priority: 1699)

            // Explicit Razor expressions @( ... )
            .AddBalanced(TokenType.RazorExpression, "@", '(', ')', priority: 1698)

            // Razor implicit expressions @identifier.property
            .AddPattern(TokenType.RazorExpression, @"@[\w_][\w\d_]*(?:\?)?(?:\.[\w_][\w\d_]*(?:\?)?)*", priority: 1697)

            // HTML markup
            .AddMarkup(priority: 1500)

            // Strings (for attributes)
            .AddString("\"", "\"", escape: "\\", priority: 1400)
            .AddString("'", "'", escape: "\\", priority: 1399)

            // C# keywords (simplified set for display in Razor context)
            .AddKeywords(TokenType.Keyword, [
                "public", "private", "protected", "internal", "static",
                "readonly", "const", "async", "await", "new", "override",
                "virtual", "abstract", "sealed", "partial", "class",
                "struct", "interface", "enum", "record", "namespace",
                "using", "var", "get", "set", "true", "false", "null"
            ], priority: 800)

            .AddKeywords(TokenType.ControlKeyword, [
                "if", "else", "switch", "case", "default",
                "for", "foreach", "while", "do", "break",
                "continue", "return", "try", "catch", "finally",
                "throw", "yield", "in"
            ], priority: 799)

            .AddKeywords(TokenType.Type, [
                "bool", "byte", "sbyte", "char", "decimal", "double",
                "float", "int", "uint", "long", "ulong", "short",
                "ushort", "object", "string", "void", "dynamic"
            ], priority: 798)

            // Numbers
            .AddPattern(TokenType.Number, @"\d+\.?\d*", requireWordBoundary: true, priority: 700)

            // Operators
            .AddOperators(["=>", "??", "?.", "&&", "||", "==", "!=", "<=", ">="], priority: 500)
            .AddOperators("+-*/%&|^~!<>=?:", priority: 499)

            // Punctuation
            .AddPunctuation("{}[]();,.", priority: 400)

            .Build();
    }
}
