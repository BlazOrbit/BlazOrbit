namespace BlazOrbit.Components;

/// <summary>Variant definition for the <see cref="BOBDataCards{TItem}"/> component.</summary>
public sealed class DataCardsVariant : Variant
{
    /// <summary>The default cards variant.</summary>
    public static readonly DataCardsVariant Default = new("Default");

    /// <summary>Creates a new variant with the specified <paramref name="name"/>.</summary>
    public DataCardsVariant(string name) : base(name) { }

    /// <summary>Creates a custom variant identified by <paramref name="name"/>.</summary>
    public static DataCardsVariant Custom(string name) => new(name);
}