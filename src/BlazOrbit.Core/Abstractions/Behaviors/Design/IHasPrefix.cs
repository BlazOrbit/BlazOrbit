namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports prefix content (text, icon, colors).
/// </summary>
public interface IHasPrefix
{
    /// <summary>Text displayed before the component value.</summary>
    string? PrefixText { get; set; }
    /// <summary>Closed-catalog icon key displayed before the component value.</summary>
    IconKey? PrefixIcon { get; set; }
    /// <summary>Color of the prefix content. Accepts any valid CSS color value.</summary>
    string? PrefixColor { get; set; }
    /// <summary>Background color of the prefix content. Accepts any valid CSS color value.</summary>
    string? PrefixBackgroundColor { get; set; }
}
