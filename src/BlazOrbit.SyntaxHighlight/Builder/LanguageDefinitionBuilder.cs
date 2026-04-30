using BlazOrbit.SyntaxHighlight.Rules;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Builder;

/// <summary>
/// Provides a fluent API for constructing <see cref="LanguageDefinition"/> instances.
/// </summary>
public sealed class LanguageDefinitionBuilder
{
    private readonly string _name;
    private readonly List<ITokenRule> _rules = [];
    private bool _caseSensitive = true;
    private int _nextPriority = 1000;

    internal LanguageDefinitionBuilder(string name) => _name = name;

    /// <summary>
    /// Adds a balanced rule that matches delimited blocks with optional nested tokenization.
    /// </summary>
    public LanguageDefinitionBuilder AddBalanced(
        TokenType tokenType,
        string prefix,
        char open,
        char close,
        Func<string, IReadOnlyList<Token>>? innerTokenizer = null,
        int? priority = null,
        int maxDepth = 100)
    {
        _rules.Add(new BalancedRule(tokenType, prefix, open, close, innerTokenizer, priority ?? _nextPriority--, maxDepth));
        return this;
    }

    /// <summary>
    /// Adds a block comment rule.
    /// </summary>
    public LanguageDefinitionBuilder AddBlockComment(string start, string end, int? priority = null) => AddDelimited(TokenType.Comment, start, end, multiline: true, priority: priority);

    /// <summary>
    /// Adds contextual keywords that match only when the specified predicate is satisfied.
    /// </summary>
    public LanguageDefinitionBuilder AddContextualKeywords(
    TokenType tokenType,
    IEnumerable<string> keywords,
    Func<string, int, bool> contextPredicate,
    int? priority = null)
    {
        _rules.Add(new ContextualKeywordRule(tokenType, keywords, contextPredicate, priority ?? _nextPriority--));
        return this;
    }

    /// <summary>
    /// Adds a contextual regex pattern that matches only when the specified predicate is satisfied.
    /// </summary>
    public LanguageDefinitionBuilder AddContextualPattern(
    TokenType tokenType,
    string pattern,
    Func<string, int, bool> contextPredicate,
    bool requireWordBoundary = false,
    int? priority = null)
    {
        _rules.Add(new ContextualRegexRule(tokenType, pattern, contextPredicate, requireWordBoundary, priority ?? _nextPriority--));
        return this;
    }

    /// <summary>
    /// Adds a delimited token rule.
    /// </summary>
    public LanguageDefinitionBuilder AddDelimited(
        TokenType tokenType,
        string start,
        string end,
        string? escape = null,
        bool multiline = true,
        int? priority = null)
    {
        _rules.Add(new DelimitedRule(tokenType, start, end, escape, multiline, priority ?? _nextPriority--));
        return this;
    }

    /// <summary>
    /// Adds keyword tokens.
    /// </summary>
    public LanguageDefinitionBuilder AddKeywords(TokenType tokenType, IEnumerable<string> keywords, int? priority = null)
    {
        _rules.Add(new KeywordRule(tokenType, keywords, priority ?? _nextPriority--));
        return this;
    }

    /// <summary>
    /// Adds a line comment rule.
    /// </summary>
    public LanguageDefinitionBuilder AddLineComment(string start, int? priority = null) => AddDelimited(TokenType.Comment, start, "\n", multiline: false, priority: priority);

    /// <summary>
    /// Adds a markup rule for HTML-like tags.
    /// </summary>
    public LanguageDefinitionBuilder AddMarkup(int? priority = null)
    {
        _rules.Add(new MarkupRule(priority ?? _nextPriority--));
        return this;
    }

    /// <summary>
    /// Adds single-character operators.
    /// </summary>
    public LanguageDefinitionBuilder AddOperators(string operators) => AddOperators(operators, null);

    /// <summary>
    /// Adds single-character operators with the specified priority.
    /// </summary>
    public LanguageDefinitionBuilder AddOperators(string operators, int? priority)
    {
        foreach (char op in operators)
        {
            _rules.Add(new SequenceRule(TokenType.Operator, op.ToString(), priority ?? _nextPriority--));
        }

        return this;
    }

    /// <summary>
    /// Adds multi-character operators.
    /// </summary>
    public LanguageDefinitionBuilder AddOperators(IEnumerable<string> operators) => AddOperators(operators, null);

    /// <summary>
    /// Adds multi-character operators with the specified priority.
    /// </summary>
    public LanguageDefinitionBuilder AddOperators(IEnumerable<string> operators, int? priority)
    {
        foreach (string op in operators.OrderByDescending(o => o.Length))
        {
            _rules.Add(new SequenceRule(TokenType.Operator, op, priority ?? _nextPriority--));
        }

        return this;
    }

    /// <summary>
    /// Adds a regex pattern rule.
    /// </summary>
    public LanguageDefinitionBuilder AddPattern(
        TokenType tokenType,
        string pattern,
        bool requireWordBoundary = false,
        int? priority = null)
    {
        _rules.Add(new RegexRule(tokenType, pattern, requireWordBoundary, priority ?? _nextPriority--));
        return this;
    }

    /// <summary>
    /// Adds punctuation characters.
    /// </summary>
    public LanguageDefinitionBuilder AddPunctuation(string punctuation, int? priority = null)
    {
        foreach (char p in punctuation)
        {
            _rules.Add(new SequenceRule(TokenType.Punctuation, p.ToString(), priority ?? _nextPriority--));
        }

        return this;
    }

    /// <summary>
    /// Adds a custom token rule.
    /// </summary>
    public LanguageDefinitionBuilder AddRule(ITokenRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Adds a sequence token rule.
    /// </summary>
    public LanguageDefinitionBuilder AddSequence(TokenType tokenType, string sequence, int? priority = null)
    {
        _rules.Add(new SequenceRule(tokenType, sequence, priority ?? _nextPriority--));
        return this;
    }

    /// <summary>
    /// Adds multiple sequence token rules.
    /// </summary>
    public LanguageDefinitionBuilder AddSequences(TokenType tokenType, IEnumerable<string> sequences, int? priority = null)
    {
        foreach (string seq in sequences.OrderByDescending(s => s.Length))
        {
            _rules.Add(new SequenceRule(tokenType, seq, priority ?? _nextPriority--));
        }

        return this;
    }

    /// <summary>
    /// Adds a string delimited rule with the specified start and end markers.
    /// </summary>
    public LanguageDefinitionBuilder AddString(
        string start,
        string end,
        string? escape = "\\",
        TokenType tokenType = TokenType.String,
        int? priority = null) => AddDelimited(tokenType, start, end, escape, multiline: true, priority: priority);

    /// <summary>
    /// Builds the <see cref="LanguageDefinition"/>.
    /// </summary>
    /// <returns>The configured language definition.</returns>
    public LanguageDefinition Build() => new(_name, _caseSensitive, [.. _rules]);

    /// <summary>
    /// Sets whether the language is case-sensitive.
    /// </summary>
    public LanguageDefinitionBuilder CaseSensitive(bool value = true)
    {
        _caseSensitive = value;
        return this;
    }

    /// <summary>
    /// Embeds rules from another language definition with an optional priority offset.
    /// </summary>
    public LanguageDefinitionBuilder Embed(LanguageDefinition other, int priorityOffset = 0)
    {
        foreach (ITokenRule rule in other.Rules)
        {
            _rules.Add(new PriorityOffsetRule(rule, priorityOffset));
        }

        return this;
    }

    private sealed class PriorityOffsetRule : ITokenRule
    {
        private readonly ITokenRule _inner;

        public PriorityOffsetRule(ITokenRule inner, int offset)
        {
            _inner = inner;
            Priority = offset;
        }

        public int Priority => _inner.Priority + field;

        public TokenMatch? TryMatch(string input, int position, TokenizerContext context)
            => _inner.TryMatch(input, position, context);
    }
}
