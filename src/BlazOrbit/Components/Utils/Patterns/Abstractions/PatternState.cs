using System.ComponentModel;

namespace BlazOrbit.Components.Utils.Patterns.Abstractions;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PatternState
{
    public bool IsComplete => Spans
        .Where(s => s.IsEditable)
        .All(s => s.IsComplete && !string.IsNullOrEmpty(s.Value));

    public bool IsDirty => Spans
        .Any(s => s.IsEditable && !s.IsToggle && !string.IsNullOrEmpty(s.Value));

    public List<SpanState> Spans { get; set; } = [];

    public string? GetActualText()
    {
        // Return null if not complete
        return !IsComplete
            ? null
            : string.Join("", Spans.Select(s =>
            s.IsEditable ? s.Value : s.Placeholder
        ));
    }

    public string GetFullText() => string.Join("", Spans.Select(s => s.DisplayValue));
}