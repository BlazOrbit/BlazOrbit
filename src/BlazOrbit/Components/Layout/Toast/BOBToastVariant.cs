namespace BlazOrbit.Components.Layout;

public sealed class BOBToastVariant : Variant
{
    public static readonly BOBToastVariant Default = new("Default");

    private BOBToastVariant(string name) : base(name)
    {
    }

    public static BOBToastVariant Custom(string name) => new(name);
}