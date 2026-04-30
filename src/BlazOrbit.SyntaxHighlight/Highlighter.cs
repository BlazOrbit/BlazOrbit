using BlazOrbit.SyntaxHighlight.Builder;
using BlazOrbit.SyntaxHighlight.Languages;
using BlazOrbit.SyntaxHighlight.Rendering;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight;

/// <summary>
/// Provides syntax highlighting for multiple programming languages by tokenizing code and rendering it as HTML.
/// </summary>
public sealed class Highlighter
{
    private readonly Dictionary<string, LanguageDefinition> _languages;
    private readonly HtmlRenderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Highlighter"/> class with default render options.
    /// </summary>
    public Highlighter() : this(HtmlRenderOptions.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Highlighter"/> class with the specified render options.
    /// </summary>
    /// <param name="options">The HTML render options.</param>
    public Highlighter(HtmlRenderOptions options)
    {
        _renderer = new HtmlRenderer();
        Options = options;
        _languages = new Dictionary<string, LanguageDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["csharp"] = CSharpLanguage.Instance,
            ["cs"] = CSharpLanguage.Instance,
            ["c#"] = CSharpLanguage.Instance,
            ["razor"] = RazorLanguage.Instance,
            ["blazor"] = RazorLanguage.Instance,
            ["cshtml"] = RazorLanguage.Instance,
            ["typescript"] = TypeScriptLanguage.Instance,
            ["ts"] = TypeScriptLanguage.Instance,
            ["css"] = CssLanguage.Instance,
        };
    }

    /// <summary>
    /// Gets or sets the HTML render options used for highlighting.
    /// </summary>
    public HtmlRenderOptions Options { get; set; }

    /// <summary>
    /// Returns the registered language identifiers.
    /// </summary>
    /// <returns>A collection of registered language names.</returns>
    public IEnumerable<string> GetRegisteredLanguages() => _languages.Keys.Distinct();

    /// <summary>
    /// Determines whether the specified language is registered.
    /// </summary>
    /// <param name="name">The language name to check.</param>
    /// <returns><see langword="true"/> if the language is registered; otherwise, <see langword="false"/>.</returns>
    public bool HasLanguage(string name) => _languages.ContainsKey(name);

    /// <summary>
    /// Highlights the specified code using the given language and returns HTML.
    /// </summary>
    /// <param name="language">The language identifier.</param>
    /// <param name="code">The code to highlight.</param>
    /// <returns>The highlighted HTML output.</returns>
    public string Highlight(string language, string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return string.Empty;
        }

        LanguageDefinition definition = GetLanguage(language);
        IReadOnlyList<Token> tokens = definition.Tokenize(code);
        return _renderer.Render(tokens, Options);
    }

    /// <summary>
    /// Registers a language definition with the specified name.
    /// </summary>
    /// <param name="name">The language name.</param>
    /// <param name="definition">The language definition.</param>
    public void RegisterLanguage(string name, LanguageDefinition definition) => _languages[name] = definition;

    /// <summary>
    /// Registers a language definition with multiple aliases.
    /// </summary>
    /// <param name="aliases">The language aliases.</param>
    /// <param name="definition">The language definition.</param>
    public void RegisterLanguage(string[] aliases, LanguageDefinition definition)
    {
        foreach (string alias in aliases)
        {
            _languages[alias] = definition;
        }
    }

    /// <summary>
    /// Tokenizes the specified code using the given language.
    /// </summary>
    /// <param name="language">The language identifier.</param>
    /// <param name="code">The code to tokenize.</param>
    /// <returns>A read-only list of tokens.</returns>
    public IReadOnlyList<Token> Tokenize(string language, string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return [];
        }

        LanguageDefinition definition = GetLanguage(language);
        return definition.Tokenize(code);
    }

    private LanguageDefinition GetLanguage(string name)
    {
        return _languages.TryGetValue(name, out LanguageDefinition? definition)
            ? definition
            : throw new ArgumentException($"Language '{name}' is not registered. Available: {string.Join(", ", GetRegisteredLanguages())}", nameof(name));
    }
}
