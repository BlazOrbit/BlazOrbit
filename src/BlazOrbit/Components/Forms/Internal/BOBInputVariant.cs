namespace BlazOrbit.Components.Forms;

/// <summary>
/// Represents a variant definition for input components.
/// </summary>
public sealed class BOBInputVariant : Variant
{
    /// <summary>
    /// The filled input variant.
    /// </summary>
    public static readonly BOBInputVariant Filled = new("Filled");

    /// <summary>
    /// The outlined input variant.
    /// </summary>
    public static readonly BOBInputVariant Outlined = new("Outlined");

    /// <summary>
    /// The standard input variant.
    /// </summary>
    public static readonly BOBInputVariant Standard = new("Standard");

    /// <summary>
    /// The flat input variant.
    /// </summary>
    public static readonly BOBInputVariant Flat = new("Flat");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBInputVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBInputVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom input variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBInputVariant"/> instance.</returns>
    public static BOBInputVariant Custom(string name) => new(name);
}
