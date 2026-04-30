namespace BlazOrbit.Components;

/// <summary>
/// Strongly-typed identifier for an SVG icon in the closed catalog.
/// Consumers obtain instances exclusively through <see cref="BOBIconKeys"/>;
/// the framework guarantees that <see cref="SvgContent"/> corresponds to a
/// vetted glyph from the built-in icon sets.
/// </summary>
public readonly record struct IconKey(string Name)
{
    /// <summary>
    /// The raw SVG markup (inner content of the &lt;svg&gt; element).
    /// This property is <c>init-only</c> and <c>internal</c> so that only
    /// the framework can pair a name with verified SVG content.
    /// </summary>
    internal string SvgContent { get; init; } = string.Empty;
}
