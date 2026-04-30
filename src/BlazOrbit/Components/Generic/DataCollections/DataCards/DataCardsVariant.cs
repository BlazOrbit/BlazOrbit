namespace BlazOrbit.Components;

public sealed class DataCardsVariant : Variant
{
    public static readonly DataCardsVariant Default = new("Default");

    public DataCardsVariant(string name) : base(name) { }

    public static DataCardsVariant Custom(string name) => new(name);
}