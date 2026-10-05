namespace BlazOrbit.Components;

/// <summary>
/// Represents a variant definition for the <see cref="BOBLink"/> component.
/// </summary>
public sealed class BOBLinkVariant : Variant
{
    /// <summary>
    /// Inline text link. Default variant.
    /// </summary>
    public static readonly BOBLinkVariant Text = new("Text");

    /// <summary>
    /// Link styled like a button, for navigational calls to action. Keeps anchor semantics.
    /// </summary>
    public static readonly BOBLinkVariant Button = new("Button");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBLinkVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBLinkVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom link variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBLinkVariant"/> instance.</returns>
    public static BOBLinkVariant Custom(string name) => new(name);
}
