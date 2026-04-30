using BlazOrbit.SyntaxHighlight.Languages;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests;

public class CSharpTokenizerTests
{
    [Theory]
    [InlineData("/* block comment */")]
    [InlineData("/* multi\nline\ncomment */")]
    [InlineData("/* nested /* not really */ end */")]
    public void Tokenize_BlockComments_AreRecognized(string code)
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        Token? commentToken = tokens.FirstOrDefault(t => t.Type == TokenType.Comment);
        Assert.NotNull(commentToken);
    }

    [Fact]
    public void Tokenize_BuiltInTypes_AreRecognized()
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize("int string bool void object");

        Assert.Contains(tokens, t => t.Type == TokenType.Type && t.Value == "int");
        Assert.Contains(tokens, t => t.Type == TokenType.Type && t.Value == "string");
        Assert.Contains(tokens, t => t.Type == TokenType.Type && t.Value == "bool");
        Assert.Contains(tokens, t => t.Type == TokenType.Type && t.Value == "void");
        Assert.Contains(tokens, t => t.Type == TokenType.Type && t.Value == "object");
    }

    [Fact]
    public void Tokenize_CaseSensitive_KeywordsNotMatchedWithWrongCase()
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize("PUBLIC CLASS IF");

        Assert.DoesNotContain(tokens, t => t.Type == TokenType.Keyword);
        Assert.DoesNotContain(tokens, t => t.Type == TokenType.ControlKeyword);
    }

    [Theory]
    [InlineData("'a'")]
    [InlineData("'\\n'")]
    [InlineData("'\\''")]
    public void Tokenize_CharLiterals_AreRecognized(string code)
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.Equal(TokenType.Char, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_ComplexCode_AllTokensHaveCorrectPositions()
    {
        string code = "public class Foo { }";
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        foreach (Token token in tokens)
        {
            string extracted = code.Substring(token.StartIndex, token.Length);
            Assert.Equal(token.Value, extracted);
        }
    }

    [Fact]
    public void Tokenize_ControlKeywords_AreRecognized()
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize("if else for foreach while");

        Assert.Contains(tokens, t => t.Type == TokenType.ControlKeyword && t.Value == "if");
        Assert.Contains(tokens, t => t.Type == TokenType.ControlKeyword && t.Value == "else");
        Assert.Contains(tokens, t => t.Type == TokenType.ControlKeyword && t.Value == "for");
        Assert.Contains(tokens, t => t.Type == TokenType.ControlKeyword && t.Value == "foreach");
        Assert.Contains(tokens, t => t.Type == TokenType.ControlKeyword && t.Value == "while");
    }

    [Fact]
    public void Tokenize_FullClassDeclaration_ProducesCorrectTokens()
    {
        string code = @"public class MyClass
{
    private int _value;

    public int Value => _value;
}";

        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "public");
        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "private");
        Assert.Contains(tokens, t => t.Type == TokenType.Type && t.Value == "int");
        Assert.Contains(tokens, t => t.Type == TokenType.Operator && t.Value == "=>");
    }

    [Theory]
    [InlineData("$\"interpolated {value}\"", TokenType.InterpolatedString)]
    [InlineData("$@\"verbatim interpolated\"", TokenType.InterpolatedString)]
    [InlineData("@$\"verbatim interpolated\"", TokenType.InterpolatedString)]
    public void Tokenize_InterpolatedStrings_AreRecognized(string code, TokenType expectedType)
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.Equal(expectedType, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_Keywords_AreRecognized()
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize("public class interface struct");

        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "public");
        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "interface");
        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "struct");
    }

    [Fact]
    public void Tokenize_KeywordsInsideIdentifiers_NotMatched()
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize("className interfaceType publicMethod");

        Assert.DoesNotContain(tokens, t => t.Type == TokenType.Keyword);
    }

    [Theory]
    [InlineData("=>")]
    [InlineData("??")]
    [InlineData("?.")]
    [InlineData("++")]
    [InlineData("--")]
    [InlineData("&&")]
    [InlineData("||")]
    [InlineData("==")]
    [InlineData("!=")]
    [InlineData("<=")]
    [InlineData(">=")]
    public void Tokenize_MultiCharOperators_AreRecognized(string code)
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.Equal(TokenType.Operator, tokens[0].Type);
        Assert.Equal(code, tokens[0].Value);
    }

    [Theory]
    [InlineData("123", "123")]
    [InlineData("123.456", "123.456")]
    [InlineData("0xFF", "0xFF")]
    [InlineData("0b1010", "0b1010")]
    [InlineData("1_000_000", "1_000_000")]
    [InlineData("3.14f", "3.14f")]
    [InlineData("3.14d", "3.14d")]
    [InlineData("3.14m", "3.14m")]
    [InlineData("123L", "123L")]
    [InlineData("123UL", "123UL")]
    public void Tokenize_Numbers_AreRecognized(string code, string expectedValue)
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("#if")]
    [InlineData("#else")]
    [InlineData("#endif")]
    [InlineData("#region")]
    [InlineData("#endregion")]
    [InlineData("#nullable")]
    public void Tokenize_PreprocessorDirectives_AreRecognized(string code)
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.PreprocessorDirective);
    }

    [Fact]
    public void Tokenize_Punctuation_AreRecognized()
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize("{ } [ ] ( ) ; , .");

        Assert.Equal(9, tokens.Count(t => t.Type == TokenType.Punctuation));
    }

    [Theory]
    [InlineData("\"hello world\"", TokenType.String)]
    [InlineData("\"hello \\\"escaped\\\" world\"", TokenType.String)]
    [InlineData("\"\"", TokenType.String)]
    public void Tokenize_RegularStrings_AreRecognized(string code, TokenType expectedType)
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(code, tokens[0].Value);
    }

    [Theory]
    [InlineData("// single line comment")]
    [InlineData("// comment with special chars: <>&\"'")]
    public void Tokenize_SingleLineComments_AreRecognized(string code)
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
    }

    [Theory]
    [InlineData("@\"verbatim string\"", TokenType.VerbatimString)]
    [InlineData("@\"contains \"\"quotes\"\"\"", TokenType.VerbatimString)]
    public void Tokenize_VerbatimStrings_AreRecognized(string code, TokenType expectedType)
    {
        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.Equal(expectedType, tokens[0].Type);
    }
}