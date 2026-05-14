namespace BlazOrbit.Components;

/// <summary>
/// Represents a variant definition for the <see cref="BOBProgressIcon"/> component.
/// </summary>
public sealed class BOBProgressIconVariant : Variant
{
    /// <summary>
    /// A spinner loading indicator.
    /// </summary>
    public static readonly BOBProgressIconVariant Spinner = new("Spinner");

    /// <summary>
    /// A ring loading indicator.
    /// </summary>
    public static readonly BOBProgressIconVariant Ring = new("Ring");

    /// <summary>
    /// A dots loading indicator.
    /// </summary>
    public static readonly BOBProgressIconVariant Dots = new("Dots");

    /// <summary>
    /// A bars loading indicator.
    /// </summary>
    public static readonly BOBProgressIconVariant Bars = new("Bars");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBProgressIconVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBProgressIconVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom loading indicator variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBProgressIconVariant"/> instance.</returns>
    public static BOBProgressIconVariant Custom(string name) => new(name);
}