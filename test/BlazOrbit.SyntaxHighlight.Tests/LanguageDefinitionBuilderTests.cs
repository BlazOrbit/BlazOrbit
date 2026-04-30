using BlazOrbit.SyntaxHighlight.Builder;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests;

public class LanguageDefinitionBuilderTests
{
    [Fact]
    public void AddBalanced_HandlesNestedBrackets()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddBalanced(TokenType.RazorExpression, "@", '(', ')')
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("@(Method(inner))");

        Token token = tokens.First(t => t.Type == TokenType.RazorExpression);
        Assert.Equal("@(Method(inner))", token.Value);
    }

    [Fact]
    public void AddBalanced_TokenizesBalancedContent()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddBalanced(TokenType.RazorExpression, "@", '(', ')')
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("@(expression)");

        Assert.Contains(tokens, t => t.Type == TokenType.RazorExpression);
    }

    [Fact]
    public void AddBlockComment_SpansMultipleLines()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddBlockComment("/*", "*/")
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("/* line1\nline2\nline3 */");

        Assert.Single(tokens);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
        Assert.Contains("line1", tokens[0].Value);
        Assert.Contains("line2", tokens[0].Value);
        Assert.Contains("line3", tokens[0].Value);
    }

    [Fact]
    public void AddDelimited_HandlesEscapeSequences()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddDelimited(TokenType.String, "\"", "\"", escape: "\\")
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("\"hello \\\"escaped\\\" world\"");

        Assert.Single(tokens);
        Assert.Equal("\"hello \\\"escaped\\\" world\"", tokens[0].Value);
    }

    [Fact]
    public void AddDelimited_TokenizesDelimitedContent()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddDelimited(TokenType.String, "\"", "\"", escape: "\\")
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("\"hello world\"");

        Assert.Single(tokens);
        Assert.Equal(TokenType.String, tokens[0].Type);
    }

    [Fact]
    public void AddKeywords_DoesNotMatchPartialWords()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddKeywords(TokenType.Keyword, ["if"])
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("iffy");

        Assert.DoesNotContain(tokens, t => t.Type == TokenType.Keyword);
    }

    [Fact]
    public void AddKeywords_TokenizesKeywords()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddKeywords(TokenType.Keyword, ["foo", "bar", "baz"])
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("foo bar baz");

        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "foo");
        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "bar");
        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "baz");
    }

    [Fact]
    public void AddLineComment_StopsAtNewline()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddLineComment("//")
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("// comment\nnot comment");

        Token comment = tokens.First(t => t.Type == TokenType.Comment);
        Assert.DoesNotContain("not comment", comment.Value);
    }

    [Fact]
    public void AddMarkup_TokenizesHtmlTags()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddMarkup()
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("<div class=\"container\">");

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "div");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "class");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeValue);
    }

    [Fact]
    public void AddOperators_LongerOperatorsMatchFirst()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddOperators(["==", "="])
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("==");

        Assert.Single(tokens);
        Assert.Equal("==", tokens[0].Value);
    }

    [Fact]
    public void AddOperators_TokenizesMultiCharOperators()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddOperators(["=>", "==", "!=", "<=", ">="])
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("=> == != <= >=");

        Assert.Equal(5, tokens.Count(t => t.Type == TokenType.Operator));
    }

    [Fact]
    public void AddPattern_MatchesRegexPattern()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddPattern(TokenType.Number, @"\d+")
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("123 456 789");

        Assert.Equal(3, tokens.Count(t => t.Type == TokenType.Number));
    }

    [Fact]
    public void AddPattern_WithWordBoundary_DoesNotMatchPartial()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddPattern(TokenType.Number, @"\d+", requireWordBoundary: true)
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("abc123def");

        Assert.DoesNotContain(tokens, t => t.Type == TokenType.Number);
    }

    [Fact]
    public void AddPunctuation_TokenizesSingleCharacters()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddPunctuation("{}[]();,")
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("{ } [ ] ( ) ; ,");

        Assert.Equal(8, tokens.Count(t => t.Type == TokenType.Punctuation));
    }

    [Fact]
    public void AddSequence_TokenizesExactSequence()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddSequence(TokenType.Directive, "#include")
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("#include <stdio.h>");

        Assert.Contains(tokens, t => t.Type == TokenType.Directive && t.Value == "#include");
    }

    [Fact]
    public void Build_ReturnsImmutableDefinition()
    {
        LanguageDefinitionBuilder builder = LanguageDefinition.Create("test")
            .AddKeywords(TokenType.Keyword, ["foo"]);

        LanguageDefinition definition1 = builder.Build();
        builder.AddKeywords(TokenType.Type, ["bar"]);
        LanguageDefinition definition2 = builder.Build();

        IReadOnlyList<Token> tokens1 = definition1.Tokenize("foo bar");
        IReadOnlyList<Token> tokens2 = definition2.Tokenize("foo bar");

        Assert.DoesNotContain(tokens1, t => t.Type == TokenType.Type);
        Assert.Contains(tokens2, t => t.Type == TokenType.Type);
    }

    [Fact]
    public void CaseSensitive_CanBeSetToFalse()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .CaseSensitive(false)
            .Build();

        Assert.False(definition.CaseSensitive);
    }

    [Fact]
    public void CaseSensitive_DefaultsToTrue()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test").Build();

        Assert.True(definition.CaseSensitive);
    }

    [Fact]
    public void CaseSensitiveFalse_MatchesKeywordsRegardlessOfCase()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .CaseSensitive(false)
            .AddKeywords(TokenType.Keyword, ["select", "from", "where"])
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("SELECT FROM WHERE");

        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "SELECT");
        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "FROM");
        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "WHERE");
    }

    [Fact]
    public void Create_WithName_SetsName()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test").Build();

        Assert.Equal("test", definition.Name);
    }

    [Fact]
    public void Embed_IncludesRulesFromOtherDefinition()
    {
        LanguageDefinition baseLanguage = LanguageDefinition.Create("base")
            .AddKeywords(TokenType.Keyword, ["base"])
            .Build();

        LanguageDefinition extendedLanguage = LanguageDefinition.Create("extended")
            .AddKeywords(TokenType.Type, ["extended"])
            .Embed(baseLanguage)
            .Build();

        IReadOnlyList<Token> tokens = extendedLanguage.Tokenize("base extended");

        Assert.Contains(tokens, t => t.Type == TokenType.Keyword && t.Value == "base");
        Assert.Contains(tokens, t => t.Type == TokenType.Type && t.Value == "extended");
    }

    [Fact]
    public void Priority_HigherPriorityMatchesFirst()
    {
        LanguageDefinition definition = LanguageDefinition.Create("test")
            .AddKeywords(TokenType.Type, ["string"], priority: 100)
            .AddKeywords(TokenType.Keyword, ["string"], priority: 50)
            .Build();

        IReadOnlyList<Token> tokens = definition.Tokenize("string");

        Assert.Single(tokens);
        Assert.Equal(TokenType.Type, tokens[0].Type);
    }
}