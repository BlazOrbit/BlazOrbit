namespace BlazOrbit.Components;

/// <summary>Variant definition for the <see cref="BOBDataGrid{TItem}"/> component.</summary>
public sealed class DataGridVariant : Variant
{
    /// <summary>The default grid variant.</summary>
    public static readonly DataGridVariant Default = new("Default");

    /// <summary>Creates a new variant with the specified <paramref name="name"/>.</summary>
    public DataGridVariant(string name) : base(name) { }

    /// <summary>Creates a custom variant identified by <paramref name="name"/>.</summary>
    public static DataGridVariant Custom(string name) => new(name);
}