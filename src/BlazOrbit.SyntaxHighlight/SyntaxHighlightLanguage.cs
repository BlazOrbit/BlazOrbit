namespace BlazOrbit.SyntaxHighlight;

/// <summary>
/// Defines the languages supported by the syntax highlighter.
/// </summary>
public enum SyntaxHighlightLanguage
{
    /// <summary>C# programming language.</summary>
    CSharp,
    /// <summary>Razor markup syntax.</summary>
    Razor,
    /// <summary>TypeScript programming language.</summary>
    TypeScript,
    /// <summary>CSS stylesheet language.</summary>
    Css,
    /// <summary>JSON data format.</summary>
    Json
}

/// <summary>
/// Provides extension methods for <see cref="SyntaxHighlightLanguage"/>.
/// </summary>
public static class SyntaxHighlightLanguageExtensions
{
    /// <summary>
    /// Converts the <see cref="SyntaxHighlightLanguage"/> to a lowercase language identifier string.
    /// </summary>
    /// <param name="language">The language to convert.</param>
    /// <returns>The language identifier.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="language"/> is not supported.</exception>
    public static string ToLanguageIdentifier(this SyntaxHighlightLanguage language)
    {
        return language switch
        {
            SyntaxHighlightLanguage.CSharp => "csharp",
            SyntaxHighlightLanguage.Razor => "razor",
            SyntaxHighlightLanguage.TypeScript => "typescript",
            SyntaxHighlightLanguage.Css => "css",
            SyntaxHighlightLanguage.Json => "json",

            _ => throw new ArgumentOutOfRangeException(nameof(language))
        };
    }
}
