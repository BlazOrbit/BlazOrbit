namespace BlazOrbit.Components;

/// <summary>
/// Represents a variant definition for the <see cref="BOBTabs"/> component.
/// </summary>
public sealed class BOBTabsVariant : Variant
{
    /// <summary>
    /// An underline tab variant.
    /// </summary>
    public static readonly BOBTabsVariant Underline = new("Underline");

    /// <summary>
    /// A pills tab variant.
    /// </summary>
    public static readonly BOBTabsVariant Pills = new("Pills");

    /// <summary>
    /// An enclosed tab variant.
    /// </summary>
    public static readonly BOBTabsVariant Enclosed = new("Enclosed");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBTabsVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBTabsVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom tabs variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBTabsVariant"/> instance.</returns>
    public static BOBTabsVariant Custom(string name) => new(name);
}
