using BlazOrbit.Components;

namespace BlazOrbit.Localization.Shared;

public sealed class BOBCultureSelectorVariant : Variant
{
    public static readonly BOBCultureSelectorVariant Dropdown = new("Dropdown");
    public static readonly BOBCultureSelectorVariant Flags = new("Flags");

    private BOBCultureSelectorVariant(string name) : base(name) { }
}
