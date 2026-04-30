using BlazOrbit.SyntaxHighlight.Builder;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Languages;

/// <summary>
/// Provides the <see cref="LanguageDefinition"/> for C# syntax highlighting.
/// </summary>
public static class CSharpLanguage
{
    /// <summary>
    /// Gets the singleton C# language definition instance.
    /// </summary>
    public static LanguageDefinition Instance => field ??= Create();

    private static LanguageDefinition Create()
    {
        return LanguageDefinition.Create("csharp")
            .CaseSensitive()

            // Comments (highest priority)
            .AddLineComment("//", priority: 1000)
            .AddBlockComment("/*", "*/", priority: 999)

            // Strings
            .AddString("@\"", "\"", escape: "\"\"", tokenType: TokenType.VerbatimString, priority: 998)
            .AddString("$@\"", "\"", escape: "\"\"", tokenType: TokenType.InterpolatedString, priority: 997)
            .AddString("@$\"", "\"", escape: "\"\"", tokenType: TokenType.InterpolatedString, priority: 996)
            .AddString("$\"", "\"", escape: "\\", tokenType: TokenType.InterpolatedString, priority: 995)
            .AddString("\"\"\"", "\"\"\"", escape: null, tokenType: TokenType.VerbatimString, priority: 994)
            .AddString("\"", "\"", escape: "\\", priority: 993)
            .AddDelimited(TokenType.Char, "'", "'", escape: "\\", priority: 992)

            // Preprocessor directives
            .AddSequences(TokenType.PreprocessorDirective, [
                "#if", "#else", "#elif", "#endif", "#define", "#undef",
                "#warning", "#error", "#line", "#region", "#endregion",
                "#pragma", "#nullable"
            ], priority: 900)

            // Control keywords
            .AddKeywords(TokenType.ControlKeyword, [
                "if", "else", "switch", "case", "default",
                "for", "foreach", "while", "do",
                "break", "continue", "goto", "return",
                "try", "catch", "finally", "throw",
                "yield", "await", "when", "where"
            ], priority: 800)

            // Keywords
            .AddKeywords(TokenType.Keyword, [
                "abstract", "as", "base", "checked", "class", "const",
                "delegate", "enum", "event", "explicit", "extern",
                "fixed", "implicit", "in", "interface", "internal",
                "is", "lock", "namespace", "new", "operator",
                "out", "override", "params", "partial", "private",
                "protected", "public", "readonly", "ref", "sealed",
                "sizeof", "stackalloc", "static", "struct", "this",
                "typeof", "unchecked", "unsafe", "using", "virtual",
                "volatile", "async", "record", "with", "init",
                "required", "file", "scoped", "var", "get", "set",
                "add", "remove", "value", "nameof", "global"
            ], priority: 799)

            // Built-in types
            .AddKeywords(TokenType.Type, [
                "bool", "byte", "sbyte", "char", "decimal", "double",
                "float", "int", "uint", "long", "ulong", "short",
                "ushort", "object", "string", "void", "dynamic",
                "nint", "nuint"
            ], priority: 798)

            // Literals
            .AddKeywords(TokenType.Keyword, [
                "true", "false", "null", "default"
            ], priority: 797)

            // Numbers
            .AddPattern(TokenType.Number, @"0[xX][0-9a-fA-F_]+[uUlL]*", priority: 700)
            .AddPattern(TokenType.Number, @"0[bB][01_]+[uUlL]*", priority: 699)
            .AddPattern(TokenType.Number, @"\d[\d_]*\.[\d_]+([eE][+-]?[\d_]+)?[fFdDmM]?", priority: 698)
            .AddPattern(TokenType.Number, @"\.[\d_]+([eE][+-]?[\d_]+)?[fFdDmM]?", priority: 697)
            .AddPattern(TokenType.Number, @"\d[\d_]*([eE][+-]?[\d_]+)[fFdDmM]?", priority: 696)
            .AddPattern(TokenType.Number, @"\d[\d_]*[fFdDmM]", priority: 695)
            .AddPattern(TokenType.Number, @"\d[\d_]*[uUlL]*", requireWordBoundary: true, priority: 694)

            // Operator keywords (need word boundaries)
            .AddKeywords(TokenType.Operator, ["is", "as"], priority: 501)

            // Operators
            .AddOperators([
                "??=", "??", "?.","?[", "=>", "&&", "||", "++", "--",
                "<<", ">>", ">>>", "<=", ">=", "==", "!=",
                "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=",
                "<<=", ">>=", ">>>="
            ], priority: 500)
            .AddOperators("+-*/%&|^~!<>=?:", priority: 499)

            // Punctuation
            .AddPunctuation("{}[]();,.", priority: 400)

            .Build();
    }
}
