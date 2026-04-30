namespace BlazOrbit.Components;

/// <summary>
/// Represents a variant definition for the <see cref="BOBSelect{TValue}"/> component.
/// </summary>
public sealed class BOBSelectVariant : Variant
{
    /// <summary>
    /// The default select variant.
    /// </summary>
    public static readonly BOBSelectVariant Default = new("default");

    private BOBSelectVariant(string value) : base(value) { }
}
