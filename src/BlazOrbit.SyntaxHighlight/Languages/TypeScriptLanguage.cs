using BlazOrbit.SyntaxHighlight.Builder;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Languages;

/// <summary>
/// Provides the <see cref="LanguageDefinition"/> for TypeScript syntax highlighting.
/// </summary>
public static class TypeScriptLanguage
{
    /// <summary>
    /// Gets the singleton TypeScript language definition instance.
    /// </summary>
    public static LanguageDefinition Instance => field ??= Create();

    private static LanguageDefinition Create()
    {
        return LanguageDefinition.Create("typescript")
            .CaseSensitive()

            // Comments
            .AddLineComment("//", priority: 1000)
            .AddBlockComment("/*", "*/", priority: 999)

            // Strings
            .AddString("`", "`", escape: "\\", tokenType: TokenType.InterpolatedString, priority: 998)
            .AddString("\"", "\"", escape: "\\", priority: 997)
            .AddString("'", "'", escape: "\\", priority: 996)

            // Regular expressions (simplified)
            .AddPattern(TokenType.String, @"/(?!\*)(?:[^/\\]|\\.)+/[gimsuy]*", priority: 950)

            // Control keywords
            .AddKeywords(TokenType.ControlKeyword, [
                "if", "else", "switch", "case", "default",
                "for", "while", "do", "break", "continue",
                "return", "throw", "try", "catch", "finally",
                "await", "yield", "with"
            ], priority: 800)

            // TypeScript specific keywords
            .AddKeywords(TokenType.Keyword, [
                "abstract", "as", "async", "class", "const", "constructor",
                "declare", "delete", "enum", "export", "extends",
                "function", "get", "implements", "import", "in",
                "infer", "instanceof", "interface", "is", "keyof",
                "let", "module", "namespace", "new", "of",
                "override", "private", "protected", "public", "readonly",
                "require", "set", "static", "super", "this",
                "type", "typeof", "var", "asserts", "satisfies"
            ], priority: 799)

            // Built-in types
            .AddKeywords(TokenType.Type, [
                "any", "bigint", "boolean", "never", "null",
                "number", "object", "string", "symbol", "undefined",
                "unknown", "void", "Array", "Map", "Set",
                "Promise", "Record", "Partial", "Required", "Readonly",
                "Pick", "Omit", "Exclude", "Extract", "NonNullable",
                "ReturnType", "InstanceType", "Parameters", "ConstructorParameters"
            ], priority: 798)

            // Literals
            .AddKeywords(TokenType.Keyword, [
                "true", "false", "null", "undefined", "NaN", "Infinity"
            ], priority: 797)

            // Numbers
            .AddPattern(TokenType.Number, @"0[xX][0-9a-fA-F_]+n?", priority: 700)
            .AddPattern(TokenType.Number, @"0[oO][0-7_]+n?", priority: 699)
            .AddPattern(TokenType.Number, @"0[bB][01_]+n?", priority: 698)
            .AddPattern(TokenType.Number, @"\d[\d_]*\.[\d_]*([eE][+-]?\d+)?", priority: 697)
            .AddPattern(TokenType.Number, @"\.[\d_]+([eE][+-]?\d+)?", priority: 696)
            .AddPattern(TokenType.Number, @"\d[\d_]*([eE][+-]?\d+)", priority: 695)
            .AddPattern(TokenType.Number, @"\d[\d_]*n?", requireWordBoundary: true, priority: 694)

            // Decorators
            .AddPattern(TokenType.Attribute, @"@[\w$]+", priority: 600)

            // Operators
            .AddOperators([
                "??=", "??", "?.", "=>", "&&", "||", "++", "--",
                "<<", ">>", ">>>", "<=", ">=", "===", "!==", "==", "!=",
                "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=",
                "**=", "&&=", "||=", "??=", "<<=", ">>=", ">>>=", "..."
            ], priority: 500)
            .AddOperators("+-*/%&|^~!<>=?:", priority: 499)

            // Punctuation
            .AddPunctuation("{}[]();,.", priority: 400)

            .Build();
    }
}
