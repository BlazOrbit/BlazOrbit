namespace BlazOrbit.Components;

/// <summary>
/// Defines a styling pattern for rows (DataGrid) or cards (DataCards).
/// CSS-expressible patterns set container-level variables once; custom patterns compute per-item.
/// </summary>
public abstract class RowStylePattern
{
    internal abstract bool IsCssExpressible { get; }
    internal abstract string? GetPatternDataAttribute();
    internal abstract Dictionary<string, string> GetContainerCssVariables();
    internal abstract string? GetItemInlineStyle(int index);

    /// <summary>
    /// Creates an alternating row style pattern.
    /// </summary>
    /// <param name="evenBackground">The background color for even rows.</param>
    /// <param name="oddBackground">The background color for odd rows.</param>
    /// <returns>A <see cref="RowStylePattern"/> that alternates row backgrounds.</returns>
    public static RowStylePattern Alternating(
        string? evenBackground = null,
        string? oddBackground = null)
        => new AlternatingRowStylePattern(evenBackground, oddBackground);

    /// <summary>
    /// Creates a pattern that highlights every Nth row.
    /// </summary>
    /// <param name="n">The interval at which rows are highlighted.</param>
    /// <param name="backgroundColor">The background color for highlighted rows.</param>
    /// <returns>A <see cref="RowStylePattern"/> that highlights every Nth row.</returns>
    public static RowStylePattern EveryNth(int n, string backgroundColor)
        => new EveryNthRowStylePattern(n, backgroundColor);

    /// <summary>
    /// Creates a pattern that applies a background color to all rows.
    /// </summary>
    /// <param name="backgroundColor">The background color for all rows.</param>
    /// <returns>A <see cref="RowStylePattern"/> that applies to all rows.</returns>
    public static RowStylePattern All(string backgroundColor)
        => new AllRowStylePattern(backgroundColor);

    /// <summary>
    /// Creates a custom row style pattern using the specified selector function.
    /// </summary>
    /// <param name="selector">A function that returns a <see cref="RowStyle"/> for a given row index.</param>
    /// <returns>A <see cref="RowStylePattern"/> that uses custom logic.</returns>
    public static RowStylePattern Custom(Func<int, RowStyle?> selector)
        => new CustomRowStylePattern(selector);
}

internal sealed class AlternatingRowStylePattern : RowStylePattern
{
    private readonly string? _evenBackground;
    private readonly string? _oddBackground;

    internal AlternatingRowStylePattern(string? evenBackground, string? oddBackground)
    {
        _evenBackground = evenBackground;
        _oddBackground = oddBackground;
    }

    internal override bool IsCssExpressible => true;

    internal override string? GetPatternDataAttribute() => "alternating";

    internal override Dictionary<string, string> GetContainerCssVariables()
    {
        Dictionary<string, string> vars = [];
        if (_evenBackground != null)
        {
            vars[FeatureDefinitions.InlineVariables.RowPatternEvenBackground] = _evenBackground;
        }

        if (_oddBackground != null)
        {
            vars[FeatureDefinitions.InlineVariables.RowPatternOddBackground] = _oddBackground;
        }

        return vars;
    }

    internal override string? GetItemInlineStyle(int index)
    {
        if (index % 2 == 0 && _evenBackground != null) { return $"{FeatureDefinitions.InlineVariables.RowPatternBackground}: {_evenBackground}"; }

        if (index % 2 != 0 && _oddBackground != null) { return $"{FeatureDefinitions.InlineVariables.RowPatternBackground}: {_oddBackground}"; }

        return null;
    }
}

internal sealed class EveryNthRowStylePattern : RowStylePattern
{
    private static readonly HashSet<int> SupportedCssValues = [3, 4, 5];
    private readonly string _backgroundColor;
    private readonly int _n;

    internal EveryNthRowStylePattern(int n, string backgroundColor)
    {
        _n = n;
        _backgroundColor = backgroundColor;
    }

    internal override bool IsCssExpressible => SupportedCssValues.Contains(_n);

    internal override string? GetPatternDataAttribute()
        => IsCssExpressible ? $"every-{_n}" : null;

    internal override Dictionary<string, string> GetContainerCssVariables()
    {
        return !IsCssExpressible
            ? []
            : new Dictionary<string, string>
            {
                [FeatureDefinitions.InlineVariables.RowPatternNthBackground] = _backgroundColor
            };
    }

    internal override string? GetItemInlineStyle(int index) => index % _n == 0 ? $"{FeatureDefinitions.InlineVariables.RowPatternBackground}: {_backgroundColor}" : null;
}

internal sealed class AllRowStylePattern : RowStylePattern
{
    private readonly string _backgroundColor;

    internal AllRowStylePattern(string backgroundColor) => _backgroundColor = backgroundColor;

    internal override bool IsCssExpressible => true;

    internal override string? GetPatternDataAttribute() => "all";

    internal override Dictionary<string, string> GetContainerCssVariables()
    {
        return new Dictionary<string, string>
        {
            [FeatureDefinitions.InlineVariables.RowPatternAllBackground] = _backgroundColor
        };
    }

    internal override string? GetItemInlineStyle(int index)
        => $"{FeatureDefinitions.InlineVariables.RowPatternBackground}: {_backgroundColor}";
}

internal sealed class CustomRowStylePattern : RowStylePattern
{
    private readonly Func<int, RowStyle?> _selector;

    internal CustomRowStylePattern(Func<int, RowStyle?> selector) => _selector = selector;

    internal override bool IsCssExpressible => false;

    internal override string? GetPatternDataAttribute() => null;

    internal override Dictionary<string, string> GetContainerCssVariables() => [];

    internal override string? GetItemInlineStyle(int index)
    {
        RowStyle? style = _selector(index);
        return style?.ToInlineVariables();
    }
}

/// <summary>
/// Defines the style for a single row.
/// </summary>
public sealed class RowStyle
{
    /// <summary>
    /// The background color of the row.
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// The border style of the row.
    /// </summary>
    public BorderStyle? Border { get; set; }

    internal string? ToInlineVariables()
    {
        List<string> vars = [];

        if (BackgroundColor != null)
        {
            vars.Add($"{FeatureDefinitions.InlineVariables.RowPatternBackground}: {BackgroundColor}");
        }

        if (Border != null)
        {
            BorderCssValues values = Border.GetCssValues();
            if (values.All != null)
            {
                vars.Add($"{FeatureDefinitions.InlineVariables.RowPatternBorder}: {values.All}");
            }

            if (values.Top != null)
            {
                vars.Add($"{FeatureDefinitions.InlineVariables.RowPatternBorderTop}: {values.Top}");
            }

            if (values.Right != null)
            {
                vars.Add($"{FeatureDefinitions.InlineVariables.RowPatternBorderRight}: {values.Right}");
            }

            if (values.Bottom != null)
            {
                vars.Add($"{FeatureDefinitions.InlineVariables.RowPatternBorderBottom}: {values.Bottom}");
            }

            if (values.Left != null)
            {
                vars.Add($"{FeatureDefinitions.InlineVariables.RowPatternBorderLeft}: {values.Left}");
            }
        }

        return vars.Count > 0 ? string.Join("; ", vars) : null;
    }
}

/// <summary>
/// Provides preset <see cref="RowStylePattern"/> configurations.
/// </summary>
public static class BOBRowPatternPresets
{
    /// <summary>
    /// A preset that stripes rows using the header background color for even rows.
    /// </summary>
    public static RowStylePattern Striped
        => RowStylePattern.Alternating(
            evenBackground: "var(--_dc-header-bg)");

    /// <summary>
    /// A preset that stripes rows using the header background color for odd rows.
    /// </summary>
    public static RowStylePattern StripedReversed
        => RowStylePattern.Alternating(
            oddBackground: "var(--_dc-header-bg)");

    /// <summary>
    /// A preset that highlights every 3rd row using the header background color.
    /// </summary>
    public static RowStylePattern Every3rd
        => RowStylePattern.EveryNth(3, "var(--_dc-header-bg)");

    /// <summary>
    /// A preset that highlights every 4th row using the header background color.
    /// </summary>
    public static RowStylePattern Every4th
        => RowStylePattern.EveryNth(4, "var(--_dc-header-bg)");

    /// <summary>
    /// A preset that highlights every 5th row using the header background color.
    /// </summary>
    public static RowStylePattern Every5th
        => RowStylePattern.EveryNth(5, "var(--_dc-header-bg)");
}
