using BlazOrbit.SyntaxHighlight.Languages;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests;

public class MarkupRuleTests
{
    [Fact]
    public void Tokenize_ClosingTag_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("</div>");

        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == "<");
        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == "/");
        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "div");
        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == ">");
    }

    [Fact]
    public void Tokenize_ComponentWithBindValue_IsRecognized()
    {
        string code = """<InputText @bind-Value="model.Name" />""";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "InputText");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "@bind-Value");
    }

    [Fact]
    public void Tokenize_HtmlComment_NotParsedAsTag()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<!-- comment -->");

        Assert.Single(tokens);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_LessThanNotFollowedByLetter_NotRecognizedAsTag()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("x < 5");

        Assert.DoesNotContain(tokens, t => t.Type == TokenType.TagName);
        Assert.Contains(tokens, t => t.Type == TokenType.Operator && t.Value == "<");
    }

    [Fact]
    public void Tokenize_MultipleTagsOnSeparateLines_AreRecognized()
    {
        string code = """
            <BOBInputDropdown Placement="DropdownPlacement.Auto" />
            <BOBInputDropdown Placement="DropdownPlacement.Top" />
            <BOBInputDropdown Placement="DropdownPlacement.Bottom" />
            """;
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Equal(3, tokens.Count(t => t.Type == TokenType.TagName && t.Value == "BOBInputDropdown"));
        Assert.Equal(3, tokens.Count(t => t.Type == TokenType.AttributeName && t.Value == "Placement"));
    }

    [Fact]
    public void Tokenize_SelfClosingTag_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<br />");

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "br");
        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == "/");
    }

    [Fact]
    public void Tokenize_SimpleTag_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<div>");

        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == "<");
        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "div");
        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == ">");
    }

    [Fact]
    public void Tokenize_TagWithComplexRazorExpression_IsRecognized()
    {
        string code = """<div class="@(isActive ? "active" : "inactive")">""";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "div");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "class");
    }

    [Fact]
    public void Tokenize_TagWithEllipsis_IsRecognized()
    {
        string code = """<BOBInputDropdown Placement="DropdownPlacement.Auto" ... />""";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "BOBInputDropdown");
        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == ">");
    }

    [Fact]
    public void Tokenize_TagWithEnumValue_IsRecognized()
    {
        string code = """<BOBInputDropdown Placement="DropdownPlacement.Auto" />""";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "BOBInputDropdown");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "Placement");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeValue && t.Value == "\"DropdownPlacement.Auto\"");
    }

    [Fact]
    public void Tokenize_TagWithGenericType_IsRecognized()
    {
        string code = """<BOBInputDropdown TValue="string" Placeholder="Select" />""";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "BOBInputDropdown");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "TValue");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "Placeholder");
    }

    [Fact]
    public void Tokenize_TagWithGreaterThanInAttribute_IsRecognized()
    {
        string code = """<span title="a > b">text</span>""";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        int spanCount = tokens.Count(t => t.Type == TokenType.TagName && t.Value == "span");
        Assert.Equal(2, spanCount);
    }

    [Fact]
    public void Tokenize_TagWithLambdaExpression_IsRecognized()
    {
        string code = """<BOBButton OnClick="@(() => DoSomething())" />""";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "BOBButton");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "OnClick");
        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == "<");
        Assert.Contains(tokens, t => t.Type == TokenType.Punctuation && t.Value == ">");
    }

    [Fact]
    public void Tokenize_TagWithMultipleAttributes_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<input type=\"text\" id=\"name\" />");

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "input");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "type");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "id");
        Assert.Equal(2, tokens.Count(t => t.Type == TokenType.AttributeValue));
    }

    [Fact]
    public void Tokenize_TagWithNestedQuotesInLambda_IsRecognized()
    {
        string code = """<BOBButton Text="Click Me" OnClick="@(() => Console.WriteLine("Clicked!"))" />""";
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "BOBButton");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "Text");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "OnClick");

        Assert.DoesNotContain(tokens, t => t.Type == TokenType.Operator && t.Value == "<");
        Assert.DoesNotContain(tokens, t => t.Type == TokenType.Operator && t.Value == ">");
    }

    [Fact]
    public void Tokenize_TagWithRazorEventHandler_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<button @onclick=\"HandleClick\">");

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "button");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "@onclick");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeValue && t.Value == "\"HandleClick\"");
    }

    [Fact]
    public void Tokenize_TagWithSimpleAttribute_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<div class=\"container\">");

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "div");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "class");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeValue && t.Value == "\"container\"");
    }

    [Fact]
    public void Tokenize_TagWithSingleQuoteAttribute_IsRecognized()
    {
        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize("<div class='container'>");

        Assert.Contains(tokens, t => t.Type == TokenType.TagName && t.Value == "div");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeName && t.Value == "class");
        Assert.Contains(tokens, t => t.Type == TokenType.AttributeValue && t.Value == "'container'");
    }
}