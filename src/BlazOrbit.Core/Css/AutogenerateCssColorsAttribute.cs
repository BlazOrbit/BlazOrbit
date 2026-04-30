namespace BlazOrbit.Components;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AutogenerateCssColorsAttribute : Attribute
{
    public AutogenerateCssColorsAttribute(int variantLevels = 5)
    {
        if (variantLevels <= 0)
        {
            throw new ArgumentException("Variant levels must be greater than 0", nameof(variantLevels));
        }

        VariantLevels = variantLevels;
    }

    public int VariantLevels { get; }
}