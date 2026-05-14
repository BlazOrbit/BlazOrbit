namespace BlazOrbit.Charts.Models;

/// <summary>
/// Symbol shape for <see cref="BOBChartShapeAnnotation{TX, TY}"/>.
/// </summary>
public enum BOBChartAnnotationShape
{
    /// <summary>Filled circle.</summary>
    Circle,

    /// <summary>Filled square.</summary>
    Square,

    /// <summary>Equilateral triangle pointing up.</summary>
    Triangle,

    /// <summary>5-point star.</summary>
    Star,

    /// <summary>4-point diamond.</summary>
    Diamond
}

/// <summary>
/// Base class for chart annotations. Closed hierarchy — derive only the
/// specialized types provided by the framework. All concrete subclasses
/// share the optional <see cref="Color"/> override (when null, falls back
/// to the chart theme's accent color).
/// </summary>
public abstract class BOBChartAnnotation
{
    /// <summary>
    /// Optional color override (CSS color value). When null, the
    /// annotation uses the chart's default accent token.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>Optional CSS class appended to the annotation element.</summary>
    public string? CssClass { get; init; }
}

/// <summary>
/// Text label anchored at a data coordinate, optionally offset by a
/// pixel delta (so the label can sit beside its anchor without
/// overlapping the data point).
/// </summary>
/// <typeparam name="TX">X-axis domain type.</typeparam>
/// <typeparam name="TY">Numeric Y-axis domain type.</typeparam>
public sealed class BOBChartTextAnnotation<TX, TY> : BOBChartAnnotation
    where TX : notnull
{
    /// <summary>Anchor X in domain space.</summary>
    public TX X { get; init; } = default!;

    /// <summary>Anchor Y in domain space.</summary>
    public TY Y { get; init; } = default!;

    /// <summary>The text to render.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Pixel offset on X applied after projection (e.g. +8 to push label right of the point).</summary>
    public double DxPx { get; init; }

    /// <summary>Pixel offset on Y applied after projection.</summary>
    public double DyPx { get; init; }
}

/// <summary>
/// Vertical band spanning the full plot height between two X domain
/// values (highlights an interval — outage window, market hours,
/// experiment phase). Set <see cref="ToX"/> equal to <see cref="FromX"/>
/// for a single vertical guide line.
/// </summary>
/// <typeparam name="TX">X-axis domain type.</typeparam>
public sealed class BOBChartBandAnnotation<TX> : BOBChartAnnotation
    where TX : notnull
{
    /// <summary>Left edge in domain space.</summary>
    public TX FromX { get; init; } = default!;

    /// <summary>Right edge in domain space.</summary>
    public TX ToX { get; init; } = default!;

    /// <summary>Fill opacity 0..1. Default 0.15.</summary>
    public double FillOpacity { get; init; } = 0.15;

    /// <summary>Optional label rendered horizontally-centered at the top of the band.</summary>
    public string? Label { get; init; }
}

/// <summary>
/// Symbol marker placed at a data coordinate. Useful for flagging
/// individual events (release marker, anomaly, peak) without writing a
/// text label.
/// </summary>
/// <typeparam name="TX">X-axis domain type.</typeparam>
/// <typeparam name="TY">Numeric Y-axis domain type.</typeparam>
public sealed class BOBChartShapeAnnotation<TX, TY> : BOBChartAnnotation
    where TX : notnull
{
    /// <summary>Anchor X in domain space.</summary>
    public TX X { get; init; } = default!;

    /// <summary>Anchor Y in domain space.</summary>
    public TY Y { get; init; } = default!;

    /// <summary>Symbol to draw.</summary>
    public BOBChartAnnotationShape Shape { get; init; } = BOBChartAnnotationShape.Circle;

    /// <summary>Symbol pixel size (bounding-box edge). Default 12.</summary>
    public double SizePx { get; init; } = 12;
}