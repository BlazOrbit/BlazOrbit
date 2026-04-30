namespace BlazOrbit.Components.Forms;

/// <summary>
/// Represents a variant definition for the <see cref="BOBInputCheckbox{TValue}"/> component.
/// </summary>
public sealed class BOBInputCheckboxVariant : Variant
{
    /// <summary>
    /// The default checkbox variant.
    /// </summary>
    public static readonly BOBInputCheckboxVariant Default = new("Default");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBInputCheckboxVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBInputCheckboxVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom checkbox variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBInputCheckboxVariant"/> instance.</returns>
    public static BOBInputCheckboxVariant Custom(string name) => new(name);
}
