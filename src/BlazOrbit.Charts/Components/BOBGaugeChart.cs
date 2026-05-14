using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Enums;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Gauge chart — single-value KPI display rendered as an arc fill against
/// a track. Supports semi (180°), three-quarter (270°) and full (360°)
/// shapes plus optional zone <see cref="Segments"/> (e.g. green-amber-red
/// thresholds). The current <see cref="Value"/> drives the fill arc and
/// the centered numeric readout.
/// </summary>
public sealed class BOBGaugeChart : BOBChartBase<int, double>
{
    /// <summary>Current value to display.</summary>
    [Parameter]
    public double Value { get; set; }

    /// <summary>Lower bound of the gauge domain. Default 0.</summary>
    [Parameter]
    public double Min { get; set; } = 0;

    /// <summary>Upper bound of the gauge domain. Default 100.</summary>
    [Parameter]
    public double Max { get; set; } = 100;

    /// <summary>Arc layout shape. Default <see cref="BOBGaugeShape.Semi"/>.</summary>
    [Parameter]
    public BOBGaugeShape Shape { get; set; } = BOBGaugeShape.Semi;

    /// <summary>Track + value-arc thickness (px). Default 16.</summary>
    [Parameter]
    public double Thickness { get; set; } = 16;

    /// <summary>Optional zone segments (color bands across the domain).</summary>
    [Parameter]
    public IEnumerable<BOBChartGaugeSegment>? Segments { get; set; }

    /// <summary>Format string for the centered value readout. Default <c>"0.#"</c>.</summary>
    [Parameter]
    public string? ValueFormat { get; set; }

    /// <summary>Optional unit suffix appended after the value (e.g. <c>"%"</c>, <c>"ms"</c>).</summary>
    [Parameter]
    public string? Unit { get; set; }

    /// <summary>Optional caption rendered below the value (e.g. metric name).</summary>
    [Parameter]
    public string? Caption { get; set; }

    /// <summary>Color of the value arc (when no <see cref="Segments"/>). Default palette[0].</summary>
    [Parameter]
    public string? ValueColor { get; set; }

    /// <summary>Color of the track behind the value arc. Default surface-variant.</summary>
    [Parameter]
    public string TrackColor { get; set; } = "var(--palette-border, #e5e7eb)";

    /// <inheritdoc />
    protected override string BuildAriaLabel() =>
        $"Gauge {Caption ?? string.Empty} {Value} of {Max}".Trim();

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        double w = EffectiveWidth;
        double h = EffectiveHeight;
        double cx = w / 2;
        // Center Y is offset for non-Full shapes so the arc fills nicely.
        double cy = Shape switch
        {
            BOBGaugeShape.Semi => h * 0.78,
            BOBGaugeShape.ThreeQuarter => h * 0.62,
            _ => h / 2
        };
        double radius = (Math.Min(w, h) / 2) - Thickness;

        (double startAngle, double sweep) = Shape switch
        {
            BOBGaugeShape.Semi => (-Math.PI, Math.PI), // 180° from left to right
            BOBGaugeShape.ThreeQuarter => (-Math.PI * 1.25, Math.PI * 1.5), // 270°
            _ => (-Math.PI / 2, Math.PI * 2) // 360° starting at 12 o'clock
        };

        double t = Math.Clamp((Value - Min) / (Max - Min == 0 ? 1 : Max - Min), 0, 1);
        double valueAngle = startAngle + (t * sweep);
        string valueColor = ValueColor ?? Palette.ColorAt(0);

        int seq = 100;

        // Track (full sweep).
        builder.OpenElement(seq++, "path");
        builder.AddAttribute(seq++, "class", "bob-gauge-chart__track");
        builder.AddAttribute(seq++, "d", ArcPath(cx, cy, radius, startAngle, startAngle + sweep));
        builder.AddAttribute(seq++, "fill", "none");
        builder.AddAttribute(seq++, "stroke", TrackColor);
        builder.AddAttribute(seq++, "stroke-width", ChartLayout.ToInvariant(Thickness));
        builder.AddAttribute(seq++, "stroke-linecap", "round");
        builder.CloseElement();

        // Zone segments (overlay on track when provided).
        if (Segments is not null)
        {
            foreach (BOBChartGaugeSegment seg in Segments)
            {
                double t0 = Math.Clamp((seg.From - Min) / (Max - Min == 0 ? 1 : Max - Min), 0, 1);
                double t1 = Math.Clamp((seg.To - Min) / (Max - Min == 0 ? 1 : Max - Min), 0, 1);
                if (t1 <= t0)
                {
                    continue;
                }

                double a0 = startAngle + (t0 * sweep);
                double a1 = startAngle + (t1 * sweep);
                builder.OpenElement(seq++, "path");
                builder.AddAttribute(seq++, "class", "bob-gauge-chart__segment");
                builder.AddAttribute(seq++, "d", ArcPath(cx, cy, radius, a0, a1));
                builder.AddAttribute(seq++, "fill", "none");
                builder.AddAttribute(seq++, "stroke", seg.Color);
                builder.AddAttribute(seq++, "stroke-width", ChartLayout.ToInvariant(Thickness));
                builder.AddAttribute(seq++, "stroke-linecap", "butt");
                if (!string.IsNullOrEmpty(seg.Label))
                {
                    builder.OpenElement(seq++, "title");
                    builder.AddContent(seq++, seg.Label);
                    builder.CloseElement();
                }

                builder.CloseElement();
            }
        }

        // Value arc.
        if (t > 0)
        {
            builder.OpenElement(seq++, "path");
            builder.AddAttribute(seq++, "class", "bob-gauge-chart__value-arc");
            builder.AddAttribute(seq++, "d", ArcPath(cx, cy, radius, startAngle, valueAngle));
            builder.AddAttribute(seq++, "fill", "none");
            builder.AddAttribute(seq++, "stroke", valueColor);
            builder.AddAttribute(seq++, "stroke-width", ChartLayout.ToInvariant(Thickness));
            builder.AddAttribute(seq++, "stroke-linecap", "round");
            builder.CloseElement();
        }

        // Centered value text + caption.
        string format = ValueFormat ?? "0.#";
        string valueStr = Value.ToString(format, CultureInfo.InvariantCulture)
                          + (string.IsNullOrEmpty(Unit) ? string.Empty : Unit);
        builder.OpenElement(seq++, "text");
        builder.AddAttribute(seq++, "class", "bob-gauge-chart__value");
        builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(cx));
        builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(cy + 4));
        builder.AddAttribute(seq++, "text-anchor", "middle");
        builder.AddAttribute(seq++, "fill", valueColor);
        builder.AddAttribute(seq++, "font-size", "28");
        builder.AddAttribute(seq++, "font-weight", "700");
        builder.AddContent(seq++, valueStr);
        builder.CloseElement();

        if (!string.IsNullOrEmpty(Caption))
        {
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "bob-gauge-chart__caption");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(cx));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(cy + 28));
            builder.AddAttribute(seq++, "text-anchor", "middle");
            builder.AddAttribute(seq++, "fill", "var(--palette-surface-contrast, #6b7280)");
            builder.AddContent(seq++, Caption);
            builder.CloseElement();
        }
    }

    /// <summary>SVG arc-path builder. Handles arcs &gt; 180° via the large-arc flag.</summary>
    private static string ArcPath(double cx, double cy, double r, double start, double end)
    {
        double x0 = cx + (r * Math.Cos(start));
        double y0 = cy + (r * Math.Sin(start));
        double x1 = cx + (r * Math.Cos(end));
        double y1 = cy + (r * Math.Sin(end));
        int largeArc = Math.Abs(end - start) > Math.PI ? 1 : 0;
        int sweep = end > start ? 1 : 0;
        return string.Format(CultureInfo.InvariantCulture,
            "M {0:F3} {1:F3} A {2:F3} {2:F3} 0 {3} {4} {5:F3} {6:F3}",
            x0, y0, r, largeArc, sweep, x1, y1);
    }
}