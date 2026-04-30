using BlazOrbit.SyntaxHighlight.Rules;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Builder;

/// <summary>
/// Defines the tokenization rules and behavior for a specific programming language.
/// </summary>
public sealed class LanguageDefinition
{
    internal LanguageDefinition(string name, bool caseSensitive, IReadOnlyList<ITokenRule> rules)
    {
        Name = name;
        CaseSensitive = caseSensitive;
        Rules = rules;
    }

    /// <summary>
    /// Gets a value indicating whether the language is case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; }

    /// <summary>
    /// Gets the name of the language.
    /// </summary>
    public string Name { get; }

    internal IReadOnlyList<ITokenRule> Rules { get; }

    /// <summary>
    /// Creates a new <see cref="LanguageDefinitionBuilder"/> for the specified language name.
    /// </summary>
    /// <param name="name">The language name.</param>
    /// <returns>A new language definition builder.</returns>
    public static LanguageDefinitionBuilder Create(string name) => new(name);

    /// <summary>
    /// Tokenizes the specified input string into a list of tokens.
    /// </summary>
    /// <param name="input">The source code to tokenize.</param>
    /// <returns>A read-only list of tokens.</returns>
    public IReadOnlyList<Token> Tokenize(string input)
    {
        TokenizerContext context = new()
        {
            LanguageName = Name,
            CaseSensitive = CaseSensitive
        };
        Tokenizer.Tokenizer tokenizer = new(Rules, context);
        return tokenizer.Tokenize(input);
    }
}
