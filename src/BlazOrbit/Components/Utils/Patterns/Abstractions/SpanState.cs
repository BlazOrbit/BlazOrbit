using System.ComponentModel;

namespace BlazOrbit.Components.Utils.Patterns.Abstractions;

/// <summary>Pattern-input plumbing — describes a single span in a pattern.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class SpanState
{
    /// <summary>Character class allowed in the span ("d" = digits, "w" = letters, "" = any).</summary>
    public string AllowedChars { get; set; } = string.Empty;
    /// <summary>Value when set, placeholder otherwise.</summary>
    public string DisplayValue => string.IsNullOrEmpty(Value) ? Placeholder : Value;
    /// <summary>Position of the span within the pattern.</summary>
    public int Index { get; set; }
    /// <summary><see langword="true"/> when the span is full (or non-editable).</summary>
    public bool IsComplete => !IsEditable || Value.Length == MaxLength;
    /// <summary><see langword="true"/> for spans accepting user input.</summary>
    public bool IsEditable { get; set; }
    /// <summary><see langword="true"/> when the span toggles between fixed values rather than free input.</summary>
    public bool IsToggle { get; set; } = false;
    /// <summary>Maximum number of characters accepted by the span.</summary>
    public int MaxLength { get; set; }
    /// <summary>Placeholder rendered when <see cref="Value"/> is empty.</summary>
    public string Placeholder { get; set; } = string.Empty;

    /// <summary>Optional predicate validating the completed value.</summary>
    public Func<string, bool>? Validator { get; set; }

    /// <summary>Current value typed into the span.</summary>
    public string Value { get; set; } = string.Empty;
}