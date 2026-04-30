using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Rules;

/// <summary>
/// Defines a rule for matching tokens during lexical analysis.
/// </summary>
public interface ITokenRule
{
    /// <summary>
    /// Gets the priority of this rule. Higher values are evaluated first.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Attempts to match a token at the specified position in the input.
    /// </summary>
    /// <param name="input">The source text.</param>
    /// <param name="position">The current position in the input.</param>
    /// <param name="context">The tokenizer context.</param>
    /// <returns>A <see cref="TokenMatch"/> if successful; otherwise, <see langword="null"/>.</returns>
    TokenMatch? TryMatch(string input, int position, TokenizerContext context);
}

/// <summary>
/// Provides contextual state for the tokenizer.
/// </summary>
public class TokenizerContext
{
    /// <summary>
    /// Gets a value indicating whether token matching is case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; init; } = true;

    /// <summary>
    /// Gets the current state from the top of the state stack, or <see langword="null"/> if empty.
    /// </summary>
    public string? CurrentState => StateStack.TryPeek(out string? state) ? state : null;

    /// <summary>
    /// Gets the name of the language being tokenized.
    /// </summary>
    public string LanguageName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the stack of tokenizer states.
    /// </summary>
    public Stack<string> StateStack { get; } = new();

    /// <summary>
    /// Pops the top state from the state stack.
    /// </summary>
    public void PopState() => StateStack.TryPop(out _);

    /// <summary>
    /// Pushes a state onto the state stack.
    /// </summary>
    /// <param name="state">The state to push.</param>
    public void PushState(string state) => StateStack.Push(state);
}
