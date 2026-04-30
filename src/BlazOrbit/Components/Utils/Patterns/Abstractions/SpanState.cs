using System.ComponentModel;

namespace BlazOrbit.Components.Utils.Patterns.Abstractions;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class SpanState
{
    public string AllowedChars { get; set; } = string.Empty;
    public string DisplayValue => string.IsNullOrEmpty(Value) ? Placeholder : Value;
    public int Index { get; set; }
    public bool IsComplete => !IsEditable || Value.Length == MaxLength;
    public bool IsEditable { get; set; }
    public bool IsToggle { get; set; } = false;
    public int MaxLength { get; set; }
    public string Placeholder { get; set; } = string.Empty;

    // "d" = digits, "w" = letters, "" = any Validator for complete value
    public Func<string, bool>? Validator { get; set; }

    public string Value { get; set; } = string.Empty;
}