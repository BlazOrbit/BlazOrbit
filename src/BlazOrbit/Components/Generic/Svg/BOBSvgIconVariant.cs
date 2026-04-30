namespace BlazOrbit.Components;

/// <summary>
/// Represents a variant definition for the <see cref="BOBSvgIcon"/> component.
/// </summary>
public sealed class BOBSvgIconVariant : Variant
{
    /// <summary>
    /// The default SVG icon variant.
    /// </summary>
    public static readonly BOBSvgIconVariant Default = new("Default");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBSvgIconVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBSvgIconVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom SVG icon variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBSvgIconVariant"/> instance.</returns>
    public static BOBSvgIconVariant Custom(string name) => new(name);
}
