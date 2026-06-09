namespace BlazOrbit.Components;

/// <summary>
/// Marks a partial static class so the <c>ColorClassGenerator</c> expands it into per-color
/// nested partial classes (Default + lighten/darken variants) at compile time.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AutogenerateCssColorsAttribute : Attribute
{
    /// <summary>Creates the attribute with the desired lighten/darken variant level count.</summary>
    public AutogenerateCssColorsAttribute(int variantLevels = 5)
    {
        if (variantLevels <= 0)
        {
            throw new ArgumentException("Variant levels must be greater than 0", nameof(variantLevels));
        }

        VariantLevels = variantLevels;
    }

    /// <summary>Number of lighten/darken steps emitted per color.</summary>
    public int VariantLevels { get; }
}
