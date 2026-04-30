namespace BlazOrbit.Components;

public sealed class DataGridVariant : Variant
{
    public static readonly DataGridVariant Default = new("Default");

    public DataGridVariant(string name) : base(name) { }

    public static DataGridVariant Custom(string name) => new(name);
}