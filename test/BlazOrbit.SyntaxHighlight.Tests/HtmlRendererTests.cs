using BlazOrbit.SyntaxHighlight.Rendering;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests;

public class HtmlRendererTests
{
    private readonly HtmlRenderer _renderer = new();

    [Fact]
    public void Render_DarkTheme_HasCorrectColors()
    {
        List<Token> tokens = [new Token(TokenType.Keyword, "test", 0, 4)];

        string result = _renderer.Render(tokens, HtmlRenderOptions.DarkTheme);

        Assert.Contains("background-color: #1e1e1e", result);
        Assert.Contains("color: #d4d4d4", result);
    }

    [Fact]
    public void Render_EmptyTokenList_ReturnsEmptyContainer()
    {
        string result = _renderer.Render([], new HtmlRenderOptions { IncludeStyles = false });

        Assert.Contains("<pre class=\"SH-container\">", result);
        Assert.Contains("<code class=\"SH-code\">", result);
        Assert.Contains("</code></pre>", result);
    }

    [Fact]
    public void Render_EscapesAmpersand()
    {
        List<Token> tokens = [new Token(TokenType.Operator, "&&", 0, 2)];

        string result = _renderer.Render(tokens, new HtmlRenderOptions { IncludeStyles = false });

        Assert.Contains("&amp;&amp;", result);
    }

    [Fact]
    public void Render_EscapesHtmlCharacters()
    {
        List<Token> tokens = [new Token(TokenType.Operator, "<", 0, 1)];

        string result = _renderer.Render(tokens, new HtmlRenderOptions { IncludeStyles = false });

        Assert.Contains("&lt;", result);
        Assert.DoesNotContain(">SH-op\"><", result);
    }

    [Fact]
    public void Render_EscapesQuotes()
    {
        List<Token> tokens = [new Token(TokenType.String, "\"hello\"", 0, 7)];

        string result = _renderer.Render(tokens, new HtmlRenderOptions { IncludeStyles = false });

        Assert.Contains("&quot;hello&quot;", result);
    }

    [Fact]
    public void Render_ExcludesStyles_WhenDisabled()
    {
        List<Token> tokens = [new Token(TokenType.Keyword, "test", 0, 4)];

        string result = _renderer.Render(tokens, new HtmlRenderOptions { IncludeStyles = false });

        Assert.DoesNotContain("<style>", result);
    }

    [Fact]
    public void Render_IncludesStyles_WhenEnabled()
    {
        List<Token> tokens = [new Token(TokenType.Keyword, "test", 0, 4)];

        string result = _renderer.Render(tokens, new HtmlRenderOptions { IncludeStyles = true });

        Assert.Contains("<style>", result);
        Assert.Contains("</style>", result);
    }

    [Fact]
    public void Render_LightTheme_HasCorrectColors()
    {
        List<Token> tokens = [new Token(TokenType.Keyword, "test", 0, 4)];

        string result = _renderer.Render(tokens, HtmlRenderOptions.LightTheme);

        Assert.Contains("background-color: #ffffff", result);
        Assert.Contains("color: #000000", result);
    }

    [Fact]
    public void Render_MultipleTokens_CorrectlyOrdered()
    {
        List<Token> tokens =
        [
            new Token(TokenType.Keyword, "public", 0, 6),
        new Token(TokenType.Text, " ", 6, 1),
        new Token(TokenType.Keyword, "class", 7, 5),
        new Token(TokenType.Text, " ", 12, 1),
        new Token(TokenType.Text, "Foo", 13, 3),
    ];

        string result = _renderer.Render(tokens, new HtmlRenderOptions { IncludeStyles = false });

        int publicIndex = result.IndexOf(">public<");
        int classIndex = result.IndexOf(">class<");
        int fooIndex = result.IndexOf("Foo");

        Assert.True(publicIndex < classIndex);
        Assert.True(classIndex < fooIndex);
    }

    [Fact]
    public void Render_PreservesNewlines()
    {
        List<Token> tokens = [new Token(TokenType.Text, "line1\nline2\nline3", 0, 17)];

        string result = _renderer.Render(tokens, new HtmlRenderOptions { IncludeStyles = false });

        Assert.Contains("line1\nline2\nline3", result);
    }

    [Fact]
    public void Render_PreservesWhitespace()
    {
        List<Token> tokens = [new Token(TokenType.Text, "    indented", 0, 12)];

        string result = _renderer.Render(tokens, new HtmlRenderOptions { IncludeStyles = false });

        Assert.Contains("    indented", result);
    }

    [Fact]
    public void Render_SingleToken_WrapsInSpan()
    {
        List<Token> tokens = [new Token(TokenType.Keyword, "class", 0, 5)];

        string result = _renderer.Render(tokens, new HtmlRenderOptions { IncludeStyles = false });

        Assert.Contains("<span class=\"SH-kw\">class</span>", result);
    }

    [Fact]
    public void Render_StylesContainBackgroundColor()
    {
        List<Token> tokens = [];
        HtmlRenderOptions options = new()
        {
            IncludeStyles = true,
            BackgroundColor = "#123456"
        };

        string result = _renderer.Render(tokens, options);

        Assert.Contains("background-color: #123456", result);
    }

    [Fact]
    public void Render_StylesContainDefaultColor()
    {
        List<Token> tokens = [];
        HtmlRenderOptions options = new()
        {
            IncludeStyles = true,
            DefaultColor = "#abcdef"
        };

        string result = _renderer.Render(tokens, options);

        Assert.Contains("color: #abcdef", result);
    }

    [Fact]
    public void Render_StylesContainFontFamily()
    {
        List<Token> tokens = [];
        HtmlRenderOptions options = new()
        {
            IncludeStyles = true,
            FontFamily = "monospace"
        };

        string result = _renderer.Render(tokens, options);

        Assert.Contains("font-family: monospace", result);
    }

    [Fact]
    public void Render_StylesContainFontSize()
    {
        List<Token> tokens = [];
        HtmlRenderOptions options = new()
        {
            IncludeStyles = true,
            FontSize = "16px"
        };

        string result = _renderer.Render(tokens, options);

        Assert.Contains("font-size: 16px", result);
    }

    [Fact]
    public void Render_StylesContainTokenColors()
    {
        List<Token> tokens = [new Token(TokenType.Keyword, "test", 0, 4)];
        HtmlRenderOptions options = new()
        {
            IncludeStyles = true,
            TokenColors = new Dictionary<TokenType, string>
            {
                [TokenType.Keyword] = "#ff0000"
            }
        };

        string result = _renderer.Render(tokens, options);

        Assert.Contains(".SH-kw { color: #ff0000; }", result);
    }

    [Fact]
    public void Render_TextToken_NotWrappedInSpan()
    {
        List<Token> tokens = [new Token(TokenType.Text, " ", 0, 1)];

        string result = _renderer.Render(tokens, new HtmlRenderOptions { IncludeStyles = false });

        Assert.DoesNotContain("<span class=\"SH-txt\">", result);
        Assert.Contains(" ", result);
    }

    [Theory]
    [InlineData(TokenType.Keyword, "SH-kw")]
    [InlineData(TokenType.ControlKeyword, "SH-ckw")]
    [InlineData(TokenType.Type, "SH-type")]
    [InlineData(TokenType.String, "SH-str")]
    [InlineData(TokenType.Number, "SH-num")]
    [InlineData(TokenType.Comment, "SH-cmt")]
    [InlineData(TokenType.Operator, "SH-op")]
    [InlineData(TokenType.Punctuation, "SH-punc")]
    [InlineData(TokenType.TagName, "SH-tname")]
    [InlineData(TokenType.AttributeName, "SH-aname")]
    [InlineData(TokenType.AttributeValue, "SH-aval")]
    [InlineData(TokenType.Directive, "SH-dir")]
    [InlineData(TokenType.CssSelector, "SH-csel")]
    [InlineData(TokenType.CssProperty, "SH-cprop")]
    [InlineData(TokenType.RazorExpression, "SH-rexp")]
    public void Render_TokenTypes_HaveCorrectClassNames(TokenType tokenType, string expectedClass)
    {
        List<Token> tokens = [new Token(tokenType, "x", 0, 1)];

        string result = _renderer.Render(tokens, new HtmlRenderOptions { IncludeStyles = false });

        Assert.Contains($"class=\"{expectedClass}\"", result);
    }
}