namespace BlazOrbit.Components;

/// <summary>
/// Indicates the component supports a text or foreground <see cref="Color" /> parameter.
/// </summary>
public interface IHasColor
{
    /// <summary>Text or foreground color. Accepts any valid CSS color value, <see cref="PaletteColor"/> or <see cref="BOBColor"/>.</summary>
    string? Color { get; set; }
}