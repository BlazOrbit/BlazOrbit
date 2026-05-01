namespace BlazOrbit.Components.Layout;

/// <summary>Variant definition for the <c>BOBThemeSelector</c> component.</summary>
public sealed class BOBThemeSelectorVariant : Variant
{
    /// <summary>The default theme-selector variant.</summary>
    public static readonly BOBThemeSelectorVariant Default = new("Default");

    /// <summary>Sun/moon toggle variant.</summary>
    public static readonly BOBThemeSelectorVariant SunMoon = new("SunMoon");

    /// <summary>Creates a new variant with the specified <paramref name="name"/>.</summary>
    public BOBThemeSelectorVariant(string name) : base(name)
    {
    }

    /// <summary>Creates a custom variant identified by <paramref name="name"/>.</summary>
    public static BOBThemeSelectorVariant Custom(string name) => new(name);
}