using BlazOrbit.SyntaxHighlight.Languages;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests;

public class RazorTokenizerTests
{
    [Fact]
    public void Tokenize_BindAttribute_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<input @bind=\"Value\" />");

        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "@bind");
    }

    [Fact]
    public void Tokenize_BlazorComponent_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<MyComponent Parameter=\"value\" />");

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "MyComponent");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "Parameter");
    }

    [Fact]
    public void Tokenize_ClosingTag_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("</div>");

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "div");
        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == "/");
    }

    [Fact]
    public void Tokenize_CodeBlock_IsRecognized()
    {
        string code = "@code { public int Value { get; set; } }";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.RazorCodeBlock);
    }

    [Fact]
    public void Tokenize_ComplexRazorFile_ProducesCorrectTokens()
    {
        string code = @"@page ""/counter""
@using System

<h1>Counter: @currentCount</h1>

<button @onclick=""IncrementCount"">Click me</button>

@code {
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}";

        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.Directive && t.Value == "@page");
        Assert.Contains(tokens, t => t.Type == TokenType.Directive && t.Value == "@using");
        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "h1");
        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "button");
        Assert.Contains(tokens, t => t.Type == TokenType.RazorExpression && t.Value == "@currentCount");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "@onclick");
        Assert.Contains(tokens, t => t.Type == TokenType.RazorCodeBlock);
    }

    [Fact]
    public void Tokenize_ComponentWithGenericParameter_IsRecognized()
    {
        string code = """<Grid TItem="Person" Items="@people" />""";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "Grid");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "TItem");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "Items");
    }

    [Fact]
    public void Tokenize_EscapedAtSign_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("@@");

        Assert.Contains(tokens, t => t.Type == TokenType.RazorDelimiter && t.Value == "@@");
    }

    [Fact]
    public void Tokenize_EventHandler_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<button @onclick=\"HandleClick\">");

        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "@onclick");
    }

    [Fact]
    public void Tokenize_HtmlComment_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<!-- html comment -->");

        Assert.Single(tokens);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_HtmlTag_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<div>");

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "div");
        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == "<");
        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == ">");
    }

    [Fact]
    public void Tokenize_HtmlTagWithAttributes_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<div class=\"container\" id=\"main\">");

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "div");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "class");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeValue && t.Value == "\"container\"");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "id");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeValue && t.Value == "\"main\"");
    }

    [Fact]
    public void Tokenize_InlineCodeBlock_IsRecognized()
    {
        string code = "@{ var x = 5; }";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.RazorCodeBlock);
    }

    [Fact]
    public void Tokenize_MixedContent_MaintainsCorrectOrder()
    {
        string code = "<p>Hello @name!</p>";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        int pTagIndex = tokens.ToList().FindIndex(t => t.Type == TokenType.TagName && t.Value == "p");
        int expressionIndex = tokens.ToList().FindIndex(t => t.Type == TokenType.RazorExpression);
        int closingTagIndex = tokens.ToList().FindLastIndex(t => t.Type == TokenType.TagName && t.Value == "p");

        Assert.True(pTagIndex < expressionIndex);
        Assert.True(expressionIndex < closingTagIndex);
    }

    [Fact]
    public void Tokenize_MultipleComponentsWithEnumValues_AllRecognized()
    {
        string code = """
            <Dropdown Placement="Placement.Auto" />
            <Dropdown Placement="Placement.Top" />
            <Dropdown Placement="Placement.Bottom" />
            """;
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Equal(3, tokens.Count(t => t.Type == TokenType.TagName && t.Value == "Dropdown"));
        Assert.Equal(3, tokens.Count(t => t.Type == TokenType.AttributeName && t.Value == "Placement"));
    }

    [Fact]
    public void Tokenize_NestedComponents_AllRecognized()
    {
        string code = """
            <Card>
                <CardHeader>Title</CardHeader>
                <CardBody>
                    <Button>Click</Button>
                </CardBody>
            </Card>
            """;
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "Card");
        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "CardHeader");
        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "CardBody");
        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "Button");
    }

    [Fact]
    public void Tokenize_PageDirectiveWithPath_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("@page \"/counter\"");

        Assert.Contains(tokens, t => t.Type == TokenType.Directive && t.Value == "@page");
        Assert.Contains(tokens, t => t.Type == TokenType.String && t.Value == "\"/counter\"");
    }

    [Fact]
    public void Tokenize_RazorComment_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("@* this is a comment *@");

        Assert.Single(tokens);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
    }

    [Theory]
    [InlineData("@if")]
    [InlineData("@else")]
    [InlineData("@foreach")]
    [InlineData("@for")]
    [InlineData("@while")]
    [InlineData("@switch")]
    public void Tokenize_RazorControlFlow_AreRecognized(string keyword)
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize($"{keyword} (true) {{ }}");

        Assert.Contains(tokens, t => t.Type == TokenType.ControlKeyword && t.Value == keyword);
    }

    [Theory]
    [InlineData("@page")]
    [InlineData("@using")]
    [InlineData("@inject")]
    [InlineData("@inherits")]
    [InlineData("@implements")]
    [InlineData("@namespace")]
    [InlineData("@layout")]
    [InlineData("@typeparam")]
    [InlineData("@attribute")]
    [InlineData("@rendermode")]
    public void Tokenize_RazorDirectives_AreRecognized(string directive)
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(directive);

        Assert.Contains(tokens, t => t.Type == TokenType.Directive && t.Value == directive);
    }

    [Fact]
    public void Tokenize_RazorExplicitExpression_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("@(expression + value)");

        Assert.Contains(tokens, t => t.Type == TokenType.RazorExpression);
    }

    [Fact]
    public void Tokenize_RazorImplicitExpression_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("@variable");

        Assert.Contains(tokens, t => t.Type == TokenType.RazorExpression && t.Value == "@variable");
    }

    [Fact]
    public void Tokenize_RazorImplicitExpressionWithDot_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("@Model.Property");

        Assert.Contains(tokens, t => t.Type == TokenType.RazorExpression && t.Value == "@Model.Property");
    }

    [Fact]
    public void Tokenize_RazorWithLambdaAndNestedQuotes_DoesNotBreakTagParsing()
    {
        string code = """<BOBButton Text="Click" OnClick="@(() => Log("msg"))" />""";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "BOBButton");

        Assert.DoesNotContain(tokens, t => t.Type == TokenType.Operator && t.Value == "<");
    }

    [Fact]
    public void Tokenize_SelfClosingTag_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<br />");

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "br");
        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == "/");
    }
}