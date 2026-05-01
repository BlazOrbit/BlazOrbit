using System.ComponentModel;

namespace BlazOrbit.Components.Utils.Patterns.Abstractions;

/// <summary>Pattern-input plumbing — internal state shared by pattern-driven inputs.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PatternState
{
    /// <summary><see langword="true"/> when every editable span has a complete, non-empty value.</summary>
    public bool IsComplete => Spans
        .Where(s => s.IsEditable)
        .All(s => s.IsComplete && !string.IsNullOrEmpty(s.Value));

    /// <summary><see langword="true"/> when at least one editable, non-toggle span has a value.</summary>
    public bool IsDirty => Spans
        .Any(s => s.IsEditable && !s.IsToggle && !string.IsNullOrEmpty(s.Value));

    /// <summary>Underlying span sequence describing the parsed pattern.</summary>
    public List<SpanState> Spans { get; set; } = [];

    /// <summary>Returns the fully-typed value when complete, or <see langword="null"/> otherwise.</summary>
    public string? GetActualText()
    {
        return !IsComplete
            ? null
            : string.Join("", Spans.Select(s =>
            s.IsEditable ? s.Value : s.Placeholder
        ));
    }

    /// <summary>Returns the visible text (value when present, placeholder otherwise) across every span.</summary>
    public string GetFullText() => string.Join("", Spans.Select(s => s.DisplayValue));
}