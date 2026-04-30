using BlazOrbit.SyntaxHighlight.Languages;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests;

public class TypeScriptTokenizerTests
{
    [Fact]
    public void Tokenize_ArrowFunction_ProducesCorrectTokens()
    {
        string code = "const add = (a: number, b: number): number => a + b;";
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "const");
        Assert.Contains(tokens, t => t.Type == TokenType.Type && t.Value == "number");
        Assert.Contains(tokens, t => t.Type == TokenType.Operator && t.Value == "=>");
    }

    [Fact]
    public void Tokenize_AsyncAwait_AreRecognized()
    {
        string code = "async function fetchData() { await fetch(url); }";
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "async");
        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "function");
        Assert.Contains(tokens, t => t.Type == TokenType.ControlKeyword && t.Value == "await");
    }

    [Theory]
    [InlineData("/* block */")]
    [InlineData("/* multi\nline */")]
    public void Tokenize_BlockComment_IsRecognized(string code)
    {
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.Comment);
    }

    [Theory]
    [InlineData("number")]
    [InlineData("string")]
    [InlineData("boolean")]
    [InlineData("any")]
    [InlineData("void")]
    [InlineData("never")]
    [InlineData("unknown")]
    [InlineData("null")]
    [InlineData("undefined")]
    public void Tokenize_BuiltInTypes_AreRecognized(string type)
    {
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(type);

        Assert.Single(tokens);
        Assert.Equal(TokenType.Type, tokens[0].Type);
    }

    [Theory]
    [InlineData("if")]
    [InlineData("else")]
    [InlineData("for")]
    [InlineData("while")]
    [InlineData("switch")]
    [InlineData("return")]
    [InlineData("throw")]
    [InlineData("try")]
    [InlineData("catch")]
    [InlineData("await")]
    public void Tokenize_ControlKeywords_AreRecognized(string keyword)
    {
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(keyword);

        Assert.Single(tokens);
        Assert.Equal(TokenType.ControlKeyword, tokens[0].Type);
    }

    [Theory]
    [InlineData("@Component")]
    [InlineData("@Injectable")]
    [InlineData("@Input")]
    public void Tokenize_Decorators_AreRecognized(string code)
    {
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.Attribute);
    }

    [Fact]
    public void Tokenize_GenericType_ContainsAngleBrackets()
    {
        string code = "Array<string>";
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.Type && t.Value == "Array");
        Assert.Contains(tokens, t => t.Type == TokenType.Operator && t.Value == "<");
        Assert.Contains(tokens, t => t.Type == TokenType.Operator && t.Value == ">");
    }

    [Fact]
    public void Tokenize_Interface_ProducesCorrectTokens()
    {
        string code = @"interface User {
    name: string;
    age: number;
}";
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "interface");
        Assert.Contains(tokens, t => t.Type == TokenType.Type && t.Value == "string");
        Assert.Contains(tokens, t => t.Type == TokenType.Type && t.Value == "number");
    }

    [Theory]
    [InlineData("const")]
    [InlineData("let")]
    [InlineData("var")]
    [InlineData("function")]
    [InlineData("class")]
    [InlineData("interface")]
    [InlineData("type")]
    [InlineData("enum")]
    [InlineData("async")]
    [InlineData("export")]
    [InlineData("import")]
    public void Tokenize_Keywords_AreRecognized(string keyword)
    {
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(keyword);

        Assert.Single(tokens);
        Assert.Equal(TokenType.Keyword, tokens[0].Type);
        Assert.Equal(keyword, tokens[0].Value);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123.456")]
    [InlineData("0xFF")]
    [InlineData("0b1010")]
    [InlineData("0o777")]
    [InlineData("123n")]
    public void Tokenize_Numbers_AreRecognized(string code)
    {
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.Equal(TokenType.Number, tokens[0].Type);
    }

    [Theory]
    [InlineData("=>")]
    [InlineData("===")]
    [InlineData("!==")]
    [InlineData("??")]
    [InlineData("?.")]
    [InlineData("...")]
    public void Tokenize_Operators_AreRecognized(string code)
    {
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.Equal(TokenType.Operator, tokens[0].Type);
    }

    [Theory]
    [InlineData("// single line")]
    public void Tokenize_SingleLineComment_IsRecognized(string code)
    {
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
    }

    [Theory]
    [InlineData("\"hello\"")]
    [InlineData("'hello'")]
    [InlineData("`template`")]
    public void Tokenize_Strings_AreRecognized(string code)
    {
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.True(tokens[0].Type is TokenType.String or TokenType.InterpolatedString);
    }

    [Fact]
    public void Tokenize_TemplateLiteral_IsRecognizedAsInterpolated()
    {
        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize("`hello ${name}`");

        Assert.Single(tokens);
        Assert.Equal(TokenType.InterpolatedString, tokens[0].Type);
    }
}