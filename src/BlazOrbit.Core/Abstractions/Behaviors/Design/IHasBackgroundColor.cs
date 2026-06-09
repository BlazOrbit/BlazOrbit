namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports a <see cref="BackgroundColor" /> parameter.
/// </summary>
public interface IHasBackgroundColor
{
    /// <summary>Background color. Accepts any valid CSS color value, <see cref="PaletteColor"/> or <see cref="BOBColor"/>.</summary>
    string? BackgroundColor { get; set; }
}