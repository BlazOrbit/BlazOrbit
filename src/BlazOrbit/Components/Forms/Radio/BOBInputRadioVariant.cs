namespace BlazOrbit.Components.Forms;

/// <summary>
/// Represents a variant definition for the <see cref="BOBInputRadio{TValue}"/> component.
/// </summary>
public sealed class BOBInputRadioVariant : Variant
{
    /// <summary>
    /// The default radio variant.
    /// </summary>
    public static readonly BOBInputRadioVariant Default = new("Default");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBInputRadioVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBInputRadioVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom radio variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBInputRadioVariant"/> instance.</returns>
    public static BOBInputRadioVariant Custom(string name) => new(name);
}
