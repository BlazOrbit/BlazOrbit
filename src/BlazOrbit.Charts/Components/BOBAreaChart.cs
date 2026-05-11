using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Area chart — line chart with the region between the line and the X axis
/// baseline filled. Each series renders as its own filled region (overlapping
/// when series intersect); a stacked variant is on the v2 roadmap.
/// <para>
/// Inherits the full Line-chart pipeline (axes, smooth interpolation, X-axis
/// auto-detect for numeric / temporal / categorical) and overrides the
/// per-series shape to add a filled <c>&lt;path&gt;</c> beneath the stroke.
/// Markers are off by default — area charts emphasise the filled volume,
/// not the individual data points.
/// </para>
/// </summary>
/// <typeparam name="TX">X-axis domain type.</typeparam>
/// <typeparam name="TY">Numeric Y-axis domain type.</typeparam>
public sealed class BOBAreaChart<TX, TY> : BOBLineChart<TX, TY>
    where TX : notnull
{
    /// <summary>
    /// Initializes the chart with area-typical defaults: <see cref="BOBLineChart{TX, TY}.ShowMarkers"/>
    /// off (the filled region carries the visual weight; markers add clutter).
    /// </summary>
    public BOBAreaChart() => ShowMarkers = false;

    /// <summary>
    /// Opacity applied to the area fill (0..1). The line stroke on top stays
    /// fully opaque so the contour reads cleanly. Default 0.25 — bright
    /// enough to see the volume, transparent enough that overlapping series
    /// remain distinguishable.
    /// </summary>
    [Parameter] public double FillOpacity { get; set; } = 0.25;

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int seriesCount = Series?.Count() ?? 0;
        return seriesCount switch
        {
            0 => "Area chart with no data",
            1 => "Area chart with one series",
            _ => $"Area chart with {seriesCount} series",
        };
    }

    /// <inheritdoc />
    private protected override void RenderSeriesShape(
        RenderTreeBuilder builder,
        ref int seq,
        BOBChartSeries<TX, TY> series,
        List<BOBChartPoint<TX, TY>> points,
        List<(double X, double Y)> projected,
        string color,
        int seriesIndex,
        ChartLayout layout,
        LinearScale yScale,
        List<(double X, double Y)>? baselineProjected = null)
    {
        // Build the area-fill path. Two cases:
        //   - Stacked (baselineProjected supplied): trace the top contour
        //     forward, then the previous-series contour backward, close.
        //     Each layer fills the band between two stacked levels.
        //   - Non-stacked: drop straight to the projected zero line (clamped
        //     to the plot rect when 0 is outside the data domain).
        string topSegment = Smooth
            ? BuildSmoothPath(projected)
            : BuildPolylinePath(projected);

        string areaPath;
        if (baselineProjected is not null && baselineProjected.Count == projected.Count)
        {
            // Reverse the baseline so the path closes cleanly: top forward,
            // baseline backward. Both rendered with the same curve engine so
            // smooth fills hug both contours consistently.
            var reversedBaseline = new List<(double X, double Y)>(baselineProjected);
            reversedBaseline.Reverse();
            string baseSegment = Smooth
                ? BuildSmoothPath(reversedBaseline)
                : BuildPolylinePath(reversedBaseline);
            // BuildSmoothPath / BuildPolylinePath both emit a leading "M" —
            // we replace the second segment's "M" with "L" to glue paths.
            string baseGlued = "L " + baseSegment.Substring(2);
            areaPath = $"{topSegment} {baseGlued} Z";
        }
        else
        {
            double zero = yScale.Project(0);
            double baseline = Math.Min(layout.PlotBottom, Math.Max(layout.PlotTop, zero));
            areaPath = $"{topSegment} "
                + $"L {ChartLayout.ToInvariant(projected[^1].X)},{ChartLayout.ToInvariant(baseline)} "
                + $"L {ChartLayout.ToInvariant(projected[0].X)},{ChartLayout.ToInvariant(baseline)} Z";
        }

        builder.OpenElement(seq++, "path");
        builder.AddAttribute(seq++, "class", "bob-area-chart__fill");
        builder.AddAttribute(seq++, "d", areaPath);
        builder.AddAttribute(seq++, "fill", color);
        builder.AddAttribute(seq++, "fill-opacity",
            FillOpacity.ToString(CultureInfo.InvariantCulture));
        builder.AddAttribute(seq++, "stroke", "none");
        builder.CloseElement();

        // Then call base for the line stroke + (optional) markers, layered on top.
        base.RenderSeriesShape(builder, ref seq, series, points, projected, color, seriesIndex, layout, yScale, baselineProjected);
    }
}
