namespace BlazOrbit.Components.Layout;

public sealed class BOBThemeSelectorVariant : Variant
{
    public static readonly BOBThemeSelectorVariant Default = new("Default");

    public static readonly BOBThemeSelectorVariant SunMoon = new("SunMoon");

    public BOBThemeSelectorVariant(string name) : base(name)
    {
    }

    public static BOBThemeSelectorVariant Custom(string name) => new(name);
}