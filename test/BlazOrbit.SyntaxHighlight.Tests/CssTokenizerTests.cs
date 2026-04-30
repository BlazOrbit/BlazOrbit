using BlazOrbit.SyntaxHighlight.Languages;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests;

public class CssTokenizerTests
{
    [Theory]
    [InlineData("@import")]
    [InlineData("@media")]
    [InlineData("@keyframes")]
    [InlineData("@font-face")]
    [InlineData("@supports")]
    [InlineData("@container")]
    public void Tokenize_AtRules_AreRecognized(string directive)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(directive);

        Assert.Contains(tokens, t => t.Type == TokenType.Directive);
    }

    [Theory]
    [InlineData("/* comment */")]
    [InlineData("/* multi\nline\ncomment */")]
    public void Tokenize_BlockComment_IsRecognized(string code)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.Comment);
    }

    [Fact]
    public void Tokenize_CaseInsensitive_PropertiesMatchRegardlessOfCase()
    {
        IReadOnlyList<Token> tokens1 = CssLanguage.Instance.Tokenize("DISPLAY");
        IReadOnlyList<Token> tokens2 = CssLanguage.Instance.Tokenize("display");
        IReadOnlyList<Token> tokens3 = CssLanguage.Instance.Tokenize("Display");

        Assert.Equal(tokens1[0].Type, tokens2[0].Type);
        Assert.Equal(tokens2[0].Type, tokens3[0].Type);
    }

    [Theory]
    [InlineData(".container")]
    [InlineData(".btn-primary")]
    [InlineData(".my-class")]
    public void Tokenize_ClassSelectors_AreRecognized(string selector)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(selector);

        Assert.Single(tokens);
        Assert.Equal(TokenType.CssSelector, tokens[0].Type);
    }

    [Theory]
    [InlineData("rgb(255, 0, 0)")]
    [InlineData("rgba(255, 0, 0, 0.5)")]
    [InlineData("hsl(120, 100%, 50%)")]
    public void Tokenize_ColorFunctions_AreRecognized(string color)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(color);

        Assert.Contains(tokens, t => (t.Type == TokenType.CssValue && t.Value.StartsWith("rgb")) || t.Value.StartsWith("hsl"));
    }

    [Fact]
    public void Tokenize_CompleteRule_ProducesCorrectTokens()
    {
        string code = ".container { display: flex; margin: 10px; }";
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.CssSelector && t.Value == ".container");
        Assert.Contains(tokens, t => t.Type == TokenType.CssProperty && t.Value == "display");
        Assert.Contains(tokens, t => t.Type == TokenType.CssValue && t.Value == "flex");
        Assert.Contains(tokens, t => t.Type == TokenType.CssProperty && t.Value == "margin");
        Assert.Contains(tokens, t => t.Type == TokenType.CssUnit && t.Value == "10px");
    }

    [Theory]
    [InlineData("display:")]
    [InlineData("color:")]
    [InlineData("background:")]
    [InlineData("margin:")]
    [InlineData("padding:")]
    [InlineData("font-size:")]
    [InlineData("flex:")]
    public void Tokenize_CssProperties_AreRecognized(string property)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(property);

        Assert.Contains(tokens, t => t.Type == TokenType.CssProperty);
    }

    [Theory]
    [InlineData("10px")]
    [InlineData("1.5em")]
    [InlineData("100%")]
    [InlineData("50vh")]
    [InlineData("2rem")]
    [InlineData("45deg")]
    [InlineData("500ms")]
    public void Tokenize_CssUnits_AreRecognized(string value)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(value);

        Assert.Single(tokens);
        Assert.Equal(TokenType.CssUnit, tokens[0].Type);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("block")]
    [InlineData("flex")]
    [InlineData("grid")]
    [InlineData("hidden")]
    [InlineData("center")]
    [InlineData("bold")]
    public void Tokenize_CssValues_AreRecognized(string value)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(value);

        Assert.Contains(tokens, t => t.Type is TokenType.CssValue or TokenType.CssProperty);
    }

    [Theory]
    [InlineData("--my-color")]
    [InlineData("--spacing-small")]
    [InlineData("--font-size-base")]
    public void Tokenize_CssVariables_AreRecognized(string variable)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(variable);

        Assert.Single(tokens);
        Assert.Equal(TokenType.Variable, tokens[0].Type);
    }

    [Theory]
    [InlineData("div")]
    [InlineData("span")]
    [InlineData("body")]
    [InlineData("header")]
    public void Tokenize_ElementSelectors_AreRecognized(string selector)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(selector);

        Assert.Single(tokens);
        Assert.Equal(TokenType.TagName, tokens[0].Type);
    }

    [Theory]
    [InlineData("#fff")]
    [InlineData("#ffffff")]
    [InlineData("#ff0000")]
    [InlineData("#00ff00ff")]
    public void Tokenize_HexColors_AreRecognized(string color)
    {
        string code = $"color: {color}";
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.CssValue && t.Value == color);
    }

    [Theory]
    [InlineData("#header")]
    [InlineData("#main-content")]
    [InlineData("#app")]
    public void Tokenize_IdSelectors_AreRecognized(string selector)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(selector);

        Assert.Single(tokens);
        Assert.Equal(TokenType.CssSelector, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_ImportantKeyword_IsRecognized()
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize("!important");

        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "!important");
    }

    [Fact]
    public void Tokenize_MediaQuery_ProducesCorrectTokens()
    {
        string code = "@media (max-width: 768px) { .container { display: block; } }";
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.Directive && t.Value == "@media");
        Assert.Contains(tokens, t => t.Type == TokenType.CssSelector && t.Value == ".container");
    }

    [Theory]
    [InlineData(":hover")]
    [InlineData(":focus")]
    [InlineData(":active")]
    [InlineData(":first-child")]
    [InlineData(":nth-child(2)")]
    public void Tokenize_PseudoClasses_AreRecognized(string pseudo)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(pseudo);

        Assert.Contains(tokens, t => t.Type == TokenType.CssPseudo);
    }

    [Theory]
    [InlineData("::before")]
    [InlineData("::after")]
    [InlineData("::first-line")]
    [InlineData("::placeholder")]
    public void Tokenize_PseudoElements_AreRecognized(string pseudo)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(pseudo);

        Assert.Contains(tokens, t => t.Type == TokenType.CssPseudo);
    }

    [Theory]
    [InlineData("\"Helvetica Neue\"")]
    [InlineData("'Arial'")]
    public void Tokenize_Strings_AreRecognized(string code)
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(code);

        Assert.Single(tokens);
        Assert.Equal(TokenType.String, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_UrlFunction_IsRecognized()
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize("url(image.png)");

        Assert.Single(tokens);
        Assert.Equal(TokenType.String, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_VarFunction_IsRecognized()
    {
        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize("var(--my-color)");

        Assert.Contains(tokens, t => t.Type == TokenType.Variable);
    }
}