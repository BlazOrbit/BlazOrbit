namespace BlazOrbit.Components.Layout;

/// <summary>
/// Represents a variant definition for the <see cref="BOBCard"/> component.
/// </summary>
public sealed class BOBCardVariant : Variant
{
    /// <summary>
    /// The default card variant.
    /// </summary>
    public static readonly BOBCardVariant Default = new("Default");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBCardVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBCardVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom card variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBCardVariant"/> instance.</returns>
    public static BOBCardVariant Custom(string name) => new(name);
}
