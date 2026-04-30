using BlazOrbit.SyntaxHighlight.Builder;
using BlazOrbit.SyntaxHighlight.Rendering;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests;

public class HighlighterTests
{
    [Fact]
    public void GetRegisteredLanguages_ReturnsAllLanguages()
    {
        Highlighter highlighter = new();

        IEnumerable<string> languages = highlighter.GetRegisteredLanguages();

        Assert.Contains("csharp", languages);
        Assert.Contains("razor", languages);
        Assert.Contains("typescript", languages);
        Assert.Contains("css", languages);
    }

    [Fact]
    public void HasLanguage_ReturnsFalseForUnknownLanguage()
    {
        Highlighter highlighter = new();

        Assert.False(highlighter.HasLanguage("unknown"));
    }

    [Fact]
    public void HasLanguage_ReturnsTrueForRegisteredLanguage()
    {
        Highlighter highlighter = new();

        Assert.True(highlighter.HasLanguage("csharp"));
        Assert.True(highlighter.HasLanguage("razor"));
        Assert.True(highlighter.HasLanguage("typescript"));
        Assert.True(highlighter.HasLanguage("css"));
    }

    [Fact]
    public void Highlight_ContainsPreAndCodeTags()
    {
        Highlighter highlighter = new();

        string result = highlighter.Highlight("csharp", "int x = 1;");

        Assert.Contains("<pre class=\"SH-container\">", result);
        Assert.Contains("<code class=\"SH-code\">", result);
        Assert.Contains("</code></pre>", result);
    }

    [Fact]
    public void Highlight_EscapesHtmlCharacters()
    {
        Highlighter highlighter = new();

        string result = highlighter.Highlight("csharp", "if (x < 5 && y > 3) { }");

        Assert.Contains("&lt;", result);
        Assert.Contains("&gt;", result);
        Assert.Contains("&amp;", result);
    }

    [Fact]
    public void Highlight_ExcludesStyleTag_WhenOptionDisabled()
    {
        Highlighter highlighter = new(new HtmlRenderOptions { IncludeStyles = false });

        string result = highlighter.Highlight("csharp", "class Foo { }");

        Assert.DoesNotContain("<style>", result);
    }

    [Fact]
    public void Highlight_IncludesStyleTag_WhenOptionEnabled()
    {
        Highlighter highlighter = new(new HtmlRenderOptions { IncludeStyles = true });

        string result = highlighter.Highlight("csharp", "class Foo { }");

        Assert.Contains("<style>", result);
        Assert.Contains("</style>", result);
    }

    [Theory]
    [InlineData("csharp")]
    [InlineData("cs")]
    [InlineData("c#")]
    [InlineData("CSharp")]
    [InlineData("CSHARP")]
    public void Highlight_WithCSharpAliases_Works(string alias)
    {
        Highlighter highlighter = new();

        string result = highlighter.Highlight(alias, "class Foo { }");

        Assert.Contains("SH-kw", result);
    }

    [Fact]
    public void Highlight_WithEmptyCode_ReturnsEmptyString()
    {
        Highlighter highlighter = new();

        string result = highlighter.Highlight("csharp", "");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Highlight_WithNullCode_ReturnsEmptyString()
    {
        Highlighter highlighter = new();

        string result = highlighter.Highlight("csharp", null!);

        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [InlineData("razor")]
    [InlineData("blazor")]
    [InlineData("cshtml")]
    public void Highlight_WithRazorAliases_Works(string alias)
    {
        Highlighter highlighter = new();

        string result = highlighter.Highlight(alias, "@page \"/home\"");

        Assert.Contains("SH-dir", result);
    }

    [Theory]
    [InlineData("typescript")]
    [InlineData("ts")]
    public void Highlight_WithTypeScriptAliases_Works(string alias)
    {
        Highlighter highlighter = new();

        string result = highlighter.Highlight(alias, "const x: number = 1;");

        Assert.Contains("SH-kw", result);
    }

    [Fact]
    public void Highlight_WithUnknownLanguage_ThrowsArgumentException()
    {
        Highlighter highlighter = new();

        Assert.Throws<ArgumentException>(() => highlighter.Highlight("unknown", "code"));
    }

    [Fact]
    public void Options_CanBeChanged()
    {
        Highlighter highlighter = new()
        {
            Options = HtmlRenderOptions.LightTheme
        };
        string result = highlighter.Highlight("csharp", "class Foo { }");

        Assert.Contains("#ffffff", result);
    }

    [Fact]
    public void RegisterLanguage_AllowsCustomLanguage()
    {
        Highlighter highlighter = new();
        LanguageDefinition customLang = LanguageDefinition.Create("custom")
            .AddKeywords(TokenType.Keyword, ["foo", "bar"])
            .Build();

        highlighter.RegisterLanguage("custom", customLang);
        string result = highlighter.Highlight("custom", "foo bar baz");

        Assert.Contains("SH-kw", result);
    }

    [Fact]
    public void RegisterLanguage_WithMultipleAliases_Works()
    {
        Highlighter highlighter = new();
        LanguageDefinition customLang = LanguageDefinition.Create("custom")
            .AddKeywords(TokenType.Keyword, ["test"])
            .Build();

        highlighter.RegisterLanguage(["custom", "cst", "cus"], customLang);

        Assert.True(highlighter.HasLanguage("custom"));
        Assert.True(highlighter.HasLanguage("cst"));
        Assert.True(highlighter.HasLanguage("cus"));
    }

    [Fact]
    public void Tokenize_ReturnsTokenList()
    {
        Highlighter highlighter = new();

        IReadOnlyList<Token> tokens = highlighter.Tokenize("csharp", "class Foo { }");

        Assert.NotEmpty(tokens);
        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "class");
    }

    [Fact]
    public void Tokenize_WithEmptyCode_ReturnsEmptyList()
    {
        Highlighter highlighter = new();

        IReadOnlyList<Token> tokens = highlighter.Tokenize("csharp", "");

        Assert.Empty(tokens);
    }
}