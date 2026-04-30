using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Rendering;

/// <summary>
/// Specifies options for rendering highlighted HTML output, including colors and fonts.
/// </summary>
public sealed class HtmlRenderOptions
{
    /// <summary>
    /// Gets a dark theme preset.
    /// </summary>
    public static HtmlRenderOptions DarkTheme => new()
    {
        BackgroundColor = "#1e1e1e",
        DefaultColor = "#d4d4d4",
        TokenColors = new Dictionary<TokenType, string>
        {
            [TokenType.Keyword] = "#569cd6",
            [TokenType.ControlKeyword] = "#c586c0",
            [TokenType.Type] = "#4ec9b0",
            [TokenType.String] = "#ce9178",
            [TokenType.VerbatimString] = "#ce9178",
            [TokenType.InterpolatedString] = "#ce9178",
            [TokenType.Char] = "#ce9178",
            [TokenType.Number] = "#b5cea8",
            [TokenType.Comment] = "#6a9955",
            [TokenType.Operator] = "#d4d4d4",
            [TokenType.Punctuation] = "#d4d4d4",
            [TokenType.Method] = "#dcdcaa",
            [TokenType.Property] = "#9cdcfe",
            [TokenType.Field] = "#9cdcfe",
            [TokenType.Parameter] = "#9cdcfe",
            [TokenType.Variable] = "#9cdcfe",
            [TokenType.Namespace] = "#4ec9b0",
            [TokenType.Directive] = "#c586c0",
            [TokenType.PreprocessorDirective] = "#808080",
            [TokenType.Attribute] = "#4ec9b0",
            [TokenType.TagName] = "#569cd6",
            [TokenType.AttributeName] = "#9cdcfe",
            [TokenType.AttributeValue] = "#ce9178",
            [TokenType.CssSelector] = "#d7ba7d",
            [TokenType.CssProperty] = "#9cdcfe",
            [TokenType.CssValue] = "#ce9178",
            [TokenType.CssUnit] = "#b5cea8",
            [TokenType.CssPseudo] = "#d7ba7d",
            [TokenType.RazorDelimiter] = "#c586c0",
            [TokenType.RazorExpression] = "#c586c0",
            [TokenType.RazorCodeBlock] = "#c586c0",
        }
    };

    /// <summary>
    /// Gets the default render options (same as <see cref="DarkTheme"/>).
    /// </summary>
    public static HtmlRenderOptions Default => DarkTheme;

    /// <summary>
    /// Gets a light theme preset.
    /// </summary>
    public static HtmlRenderOptions LightTheme => new()
    {
        BackgroundColor = "#ffffff",
        DefaultColor = "#000000",
        TokenColors = new Dictionary<TokenType, string>
        {
            [TokenType.Keyword] = "#0000ff",
            [TokenType.ControlKeyword] = "#af00db",
            [TokenType.Type] = "#267f99",
            [TokenType.String] = "#a31515",
            [TokenType.VerbatimString] = "#a31515",
            [TokenType.InterpolatedString] = "#a31515",
            [TokenType.Char] = "#a31515",
            [TokenType.Number] = "#098658",
            [TokenType.Comment] = "#008000",
            [TokenType.Operator] = "#000000",
            [TokenType.Punctuation] = "#000000",
            [TokenType.Method] = "#795e26",
            [TokenType.Property] = "#001080",
            [TokenType.Field] = "#001080",
            [TokenType.Parameter] = "#001080",
            [TokenType.Variable] = "#001080",
            [TokenType.Namespace] = "#267f99",
            [TokenType.Directive] = "#af00db",
            [TokenType.PreprocessorDirective] = "#808080",
            [TokenType.Attribute] = "#267f99",
            [TokenType.TagName] = "#800000",
            [TokenType.AttributeName] = "#ff0000",
            [TokenType.AttributeValue] = "#0000ff",
            [TokenType.CssSelector] = "#800000",
            [TokenType.CssProperty] = "#ff0000",
            [TokenType.CssValue] = "#0000ff",
            [TokenType.CssUnit] = "#098658",
            [TokenType.CssPseudo] = "#800000",
            [TokenType.RazorDelimiter] = "#af00db",
            [TokenType.RazorExpression] = "#af00db",
            [TokenType.RazorCodeBlock] = "#af00db",
        }
    };

    /// <summary>
    /// Gets the background color.
    /// </summary>
    public string BackgroundColor { get; init; } = "#1e1e1e";

    /// <summary>
    /// Gets the default text color.
    /// </summary>
    public string DefaultColor { get; init; } = "#d4d4d4";

    /// <summary>
    /// Gets the font family.
    /// </summary>
    public string FontFamily { get; init; } = "'Cascadia Code', 'Fira Code', Consolas, 'Courier New', monospace";

    /// <summary>
    /// Gets the font size.
    /// </summary>
    public string FontSize { get; init; } = "14px";

    /// <summary>
    /// Gets a value indicating whether to include inline CSS styles.
    /// </summary>
    public bool IncludeStyles { get; init; } = true;

    /// <summary>
    /// Gets the dictionary mapping token types to colors.
    /// </summary>
    public Dictionary<TokenType, string> TokenColors { get; init; } = [];
}
