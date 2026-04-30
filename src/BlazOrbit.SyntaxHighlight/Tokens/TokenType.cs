namespace BlazOrbit.SyntaxHighlight.Tokens;

/// <summary>
/// Defines the types of tokens that can be produced by the syntax highlighter.
/// </summary>
public enum TokenType
{
    /// <summary>Plain text without special meaning.</summary>
    Text,
    /// <summary>A language keyword.</summary>
    Keyword,
    /// <summary>A control-flow keyword.</summary>
    ControlKeyword,
    /// <summary>A built-in or user-defined type name.</summary>
    Type,
    /// <summary>A string literal.</summary>
    String,
    /// <summary>A verbatim string literal.</summary>
    VerbatimString,
    /// <summary>An interpolated string literal.</summary>
    InterpolatedString,
    /// <summary>A character literal.</summary>
    Char,
    /// <summary>A numeric literal.</summary>
    Number,
    /// <summary>A comment.</summary>
    Comment,
    /// <summary>An operator.</summary>
    Operator,
    /// <summary>Punctuation.</summary>
    Punctuation,
    /// <summary>A method name.</summary>
    Method,
    /// <summary>A property name.</summary>
    Property,
    /// <summary>A field name.</summary>
    Field,
    /// <summary>A parameter name.</summary>
    Parameter,
    /// <summary>A variable name.</summary>
    Variable,
    /// <summary>A namespace name.</summary>
    Namespace,
    /// <summary>A directive.</summary>
    Directive,
    /// <summary>A preprocessor directive.</summary>
    PreprocessorDirective,
    /// <summary>An attribute or annotation.</summary>
    Attribute,
    /// <summary>A markup tag and its contents.</summary>
    Tag,
    /// <summary>A markup tag name.</summary>
    TagName,
    /// <summary>A markup attribute name.</summary>
    AttributeName,
    /// <summary>A markup attribute value.</summary>
    AttributeValue,
    /// <summary>A CSS selector.</summary>
    CssSelector,
    /// <summary>A CSS property name.</summary>
    CssProperty,
    /// <summary>A CSS value.</summary>
    CssValue,
    /// <summary>A CSS unit.</summary>
    CssUnit,
    /// <summary>A CSS pseudo-element or pseudo-class.</summary>
    CssPseudo,
    /// <summary>A Razor delimiter.</summary>
    RazorDelimiter,
    /// <summary>A Razor expression.</summary>
    RazorExpression,
    /// <summary>A Razor code block.</summary>
    RazorCodeBlock,
}
