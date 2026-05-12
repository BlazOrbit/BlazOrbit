using System.Globalization;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazOrbit.Charts.Components.Internal;

/// <summary>
/// Renders horizontal reference / threshold lines across the plot area.
/// Shared helper used by every cartesian chart (<see cref="BOBBarChart{TX, TY}"/>,
/// <see cref="BOBLineChart{TX, TY}"/>, <see cref="BOBAreaChart{TX, TY}"/>);
/// pie / donut don't have a Y axis so they don't consume this.
/// </summary>
internal static class ReferenceLineRenderer
{
    /// <summary>Emit each reference line as <c>&lt;line&gt;</c> + optional label.</summary>
    public static void Render(
        RenderTreeBuilder builder,
        ref int seq,
        ChartLayout layout,
        LinearScale yScale,
        IEnumerable<BOBChartReferenceLine>? lines)
    {
        if (lines is null)
        {
            return;
        }

        List<BOBChartReferenceLine> list = lines.ToList();
        if (list.Count == 0)
        {
            return;
        }

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-chart__reference-lines");
        // pointer-events:none so the line never steals hover from data points.
        builder.AddAttribute(seq++, "pointer-events", "none");

        foreach (BOBChartReferenceLine line in list)
        {
            double y = yScale.Project(line.Value);

            string color = line.Color ?? "var(--palette-warning, #f59e0b)";
            string dashArray = line.Style switch
            {
                BOBChartReferenceLineStyle.Dashed => "6,4",
                BOBChartReferenceLineStyle.Dotted => "2,3",
                _ => "0"
            };

            builder.OpenElement(seq++, "line");
            builder.AddAttribute(seq++, "class", "bob-chart__reference-line");
            builder.AddAttribute(seq++, "x1", ChartLayout.ToInvariant(layout.PlotLeft));
            builder.AddAttribute(seq++, "x2", ChartLayout.ToInvariant(layout.PlotRight));
            builder.AddAttribute(seq++, "y1", ChartLayout.ToInvariant(y));
            builder.AddAttribute(seq++, "y2", ChartLayout.ToInvariant(y));
            builder.AddAttribute(seq++, "stroke", color);
            builder.AddAttribute(seq++, "stroke-width",
                line.StrokeWidth.ToString(CultureInfo.InvariantCulture));
            if (line.Style != BOBChartReferenceLineStyle.Solid)
            {
                builder.AddAttribute(seq++, "stroke-dasharray", dashArray);
            }

            builder.CloseElement(); // line

            if (!string.IsNullOrEmpty(line.Label))
            {
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "class", "bob-chart__reference-label");
                // Right-align label inside the plot area, sitting just above the line.
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(layout.PlotRight - 6));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(y - 4));
                builder.AddAttribute(seq++, "text-anchor", "end");
                builder.AddAttribute(seq++, "fill", color);
                builder.AddContent(seq++, line.Label);
                builder.CloseElement();
            }
        }

        builder.CloseElement(); // g
    }
}