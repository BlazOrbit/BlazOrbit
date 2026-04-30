namespace BlazOrbit.Components;

/// <summary>
/// Represents a variant definition for the <see cref="BOBLoadingIndicator"/> component.
/// </summary>
public sealed class BOBLoadingIndicatorVariant : Variant
{
    /// <summary>
    /// A spinner loading indicator.
    /// </summary>
    public static readonly BOBLoadingIndicatorVariant Spinner = new("Spinner");

    /// <summary>
    /// A circular progress loading indicator.
    /// </summary>
    public static readonly BOBLoadingIndicatorVariant CircularProgress = new("CircularProgress");

    /// <summary>
    /// A ring loading indicator.
    /// </summary>
    public static readonly BOBLoadingIndicatorVariant Ring = new("Ring");

    /// <summary>
    /// A dots loading indicator.
    /// </summary>
    public static readonly BOBLoadingIndicatorVariant Dots = new("Dots");

    /// <summary>
    /// A bars loading indicator.
    /// </summary>
    public static readonly BOBLoadingIndicatorVariant Bars = new("Bars");

    /// <summary>
    /// A linear indeterminate loading indicator.
    /// </summary>
    public static readonly BOBLoadingIndicatorVariant LinearIndeterminate = new("LinearIndeterminate");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBLoadingIndicatorVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBLoadingIndicatorVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom loading indicator variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBLoadingIndicatorVariant"/> instance.</returns>
    public static BOBLoadingIndicatorVariant Custom(string name) => new(name);
}
