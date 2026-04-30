namespace BlazOrbit.Components;

/// <summary>
/// Represents a variant definition for the <see cref="BOBBadge"/> component.
/// </summary>
public sealed class BOBBadgeVariant : Variant
{
    /// <summary>
    /// The default badge variant.
    /// </summary>
    public static readonly BOBBadgeVariant Default = new("Default");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBBadgeVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBBadgeVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom badge variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBBadgeVariant"/> instance.</returns>
    public static BOBBadgeVariant Custom(string name) => new(name);
}
