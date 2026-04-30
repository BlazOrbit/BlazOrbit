namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports suffix content (text, icon, colors).
/// </summary>
public interface IHasSuffix
{
    /// <summary>Text displayed after the component value.</summary>
    string? SuffixText { get; set; }
    /// <summary>Closed-catalog icon key displayed after the component value.</summary>
    IconKey? SuffixIcon { get; set; }
    /// <summary>Color of the suffix content. Accepts any valid CSS color value.</summary>
    string? SuffixColor { get; set; }
    /// <summary>Background color of the suffix content. Accepts any valid CSS color value.</summary>
    string? SuffixBackgroundColor { get; set; }
}
