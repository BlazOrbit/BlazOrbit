using BlazOrbit.Components;

namespace BlazOrbit.Localization.Shared;

/// <summary>Variants available for the culture-selector component.</summary>
public sealed class BOBCultureSelectorVariant : Variant
{
    /// <summary>Dropdown variant — culture list inside a <c>BOBInputDropdown</c>.</summary>
    public static readonly BOBCultureSelectorVariant Dropdown = new("Dropdown");

    /// <summary>Flags variant — inline strip of clickable country flags.</summary>
    public static readonly BOBCultureSelectorVariant Flags = new("Flags");

    private BOBCultureSelectorVariant(string name) : base(name) { }
}
