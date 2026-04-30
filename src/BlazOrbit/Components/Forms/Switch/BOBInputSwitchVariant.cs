namespace BlazOrbit.Components.Forms;

/// <summary>
/// Represents a variant definition for the <see cref="BOBInputSwitch"/> component.
/// </summary>
public sealed class BOBInputSwitchVariant : Variant
{
    /// <summary>
    /// The default switch variant.
    /// </summary>
    public static readonly BOBInputSwitchVariant Default = new("Default");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBInputSwitchVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBInputSwitchVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom switch variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBInputSwitchVariant"/> instance.</returns>
    public static BOBInputSwitchVariant Custom(string name) => new(name);
}
