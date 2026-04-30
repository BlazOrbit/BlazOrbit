namespace BlazOrbit.Components;

/// <summary>
/// Public, stable subset of the BlazOrbit CSS/DOM contract that consumers can reference
/// when writing custom CSS or tests against BOB components.
/// </summary>
/// <remarks>
/// The internal contract lives in <c>BlazOrbit.Components.FeatureDefinitions</c>
/// and may change between minor versions; the keys exposed here are the subset that
/// the library commits to keep stable across 1.x.
/// </remarks>
public static class BOBStylingKeys
{
    /// <summary>
    /// <c>data-bob-component</c> — identifies the component type on the root
    /// <c>&lt;bob-component&gt;</c> element (e.g. <c>button</c>, <c>input-text</c>).
    /// </summary>
    public const string Component = "data-bob-component";

    /// <summary>
    /// <c>data-bob-size</c> — small | medium | large. Drives
    /// <see cref="InlineSizeMultiplier"/>.
    /// </summary>
    public const string Size = "data-bob-size";

    /// <summary>
    /// <c>data-bob-density</c> — compact | standard | comfortable. Drives
    /// <see cref="InlineDensityMultiplier"/>.
    /// </summary>
    public const string Density = "data-bob-density";

    /// <summary>
    /// <c>data-bob-variant</c> — current variant name (e.g. <c>outlined</c>,
    /// <c>filled</c>, a custom variant registered via <c>AddBlazOrbitVariants</c>).
    /// </summary>
    public const string Variant = "data-bob-variant";

    /// <summary>
    /// <c>--bob-inline-color</c> — inline foreground-color override emitted from
    /// <c>IHasColor</c>.
    /// </summary>
    public const string InlineColor = "--bob-inline-color";

    /// <summary>
    /// <c>--bob-inline-background</c> — inline background-color override emitted
    /// from <c>IHasBackgroundColor</c>.
    /// </summary>
    public const string InlineBackground = "--bob-inline-background";

    /// <summary>
    /// <c>--bob-size-multiplier</c> — scalar multiplier applied to component
    /// dimensions based on <see cref="Size"/>.
    /// </summary>
    public const string InlineSizeMultiplier = "--bob-size-multiplier";

    /// <summary>
    /// <c>--bob-density-multiplier</c> — scalar multiplier applied to inter-element
    /// spacing based on <see cref="Density"/>.
    /// </summary>
    public const string InlineDensityMultiplier = "--bob-density-multiplier";
}
