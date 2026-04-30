namespace BlazOrbit.Components;

/// <summary>
/// Represents a variant definition for the <see cref="BOBButton"/> component.
/// </summary>
public sealed class BOBButtonVariant : Variant
{
    /// <summary>
    /// The default button variant.
    /// </summary>
    public static readonly BOBButtonVariant Default = new("Default");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBButtonVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBButtonVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom button variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBButtonVariant"/> instance.</returns>
    public static BOBButtonVariant Custom(string name) => new(name);
}
