using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Shared rendering pipeline for circular distribution charts. Subclassed by
/// <see cref="BOBPieChart{TY}"/> (full disc, <see cref="InnerRadius"/> = 0)
/// and <see cref="BOBDonutChart{TY}"/> (annulus, <see cref="InnerRadius"/> ≈ 0.6).
/// <para>
/// Pie / donut do not have X / Y axes, so this base derives from
/// <c>BOBChartBase&lt;object, TY&gt;</c> with TX bound to the dummy
/// <see cref="object"/> type.
/// </para>
/// </summary>
/// <typeparam name="TY">Numeric type of the slice values.</typeparam>
public abstract class BOBPieChartBase<TY> : BOBChartBase<object, TY>
{
    /// <summary>The slices rendered by the chart, in declaration order (clockwise from 12 o'clock).</summary>
    [Parameter]
    public IEnumerable<BOBChartSlice<TY>>? Slices { get; set; }

    /// <summary>
    /// Inner-radius / outer-radius ratio in [0, 1).
    /// 0 → full pie. ~0.5–0.7 → donut. ≥1 invalid (clamped to 0.99).
    /// </summary>
    [Parameter]
    public double InnerRadius { get; set; }

    /// <summary>
    /// When <c>true</c> (default), the slice's percentage is rendered at the
    /// arc's mid-radius. Skipped for slices smaller than 5% to avoid label
    /// collisions.
    /// </summary>
    [Parameter]
    public bool ShowPercentages { get; set; } = true;

    /// <summary>
    /// Pixel padding inside the SVG viewport so labels don't bump against
    /// the edge. 16px default.
    /// </summary>
    [Parameter]
    public double Padding { get; set; } = 16;

    /// <summary>
    /// Fired when the user clicks a slice. Carries the slice label, raw
    /// value, declaration index and the computed percentage.
    /// </summary>
    [Parameter]
    public EventCallback<BOBChartSliceClickArgs<TY>> OnSliceClick { get; set; }

    /// <summary>
    /// Fired when the user hovers a slice (mouseenter). Raised in addition
    /// to the native SVG <c>&lt;title&gt;</c> tooltip.
    /// </summary>
    [Parameter]
    public EventCallback<BOBChartSliceHoverArgs<TY>> OnSliceHover { get; set; }

    /// <summary>
    /// Subclass-specific noun used in <see cref="BuildAriaLabel"/>
    /// (e.g. "Pie", "Donut") so the screen-reader announcement is accurate.
    /// </summary>
    protected abstract string ChartTypeNoun { get; }

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int sliceCount = Slices?.Count() ?? 0;
        return sliceCount switch
        {
            0 => $"{ChartTypeNoun} chart with no data",
            1 => $"{ChartTypeNoun} chart with one slice",
            _ => $"{ChartTypeNoun} chart with {sliceCount} slices"
        };
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Slices is null)
        {
            return;
        }

        // Filter out slices the user toggled hidden via the legend.
        List<BOBChartSlice<TY>> slices = Slices
            .Where(s => !IsSeriesHidden(s.Label))
            .ToList();
        if (slices.Count == 0)
        {
            return;
        }

        // Total magnitude. Skip the chart entirely if zero or negative —
        // a "100% of nothing" pie is more confusing than no chart.
        double total = slices.Sum(s => Math.Max(0, Numeric.ToDouble(s.Value)));
        if (total <= 0)
        {
            return;
        }

        double svgWidth = EffectiveWidth;
        double svgHeight = EffectiveHeight;

        // Center the disc inside the viewport, leaving Padding around it.
        double cx = svgWidth / 2;
        double cy = svgHeight / 2;
        double outerR = Math.Max(0, (Math.Min(svgWidth, svgHeight) / 2) - Padding);
        double inner = Math.Clamp(InnerRadius, 0, 0.99);
        double innerR = outerR * inner;

        int seq = 100;

        // Single-slice case: a 360° arc is degenerate in SVG (M=A=Z form
        // collapses). Render a full circle (or annulus) instead.
        if (slices.Count == 1)
        {
            RenderFullDisc(builder, ref seq, slices[0], 0, cx, cy, outerR, innerR, total);
            return;
        }

        double accumulated = 0;
        for (int i = 0; i < slices.Count; i++)
        {
            BOBChartSlice<TY> slice = slices[i];
            double value = Math.Max(0, Numeric.ToDouble(slice.Value));
            if (value <= 0)
            {
                continue;
            }

            double startAngle = accumulated / total * Math.Tau;
            accumulated += value;
            double endAngle = accumulated / total * Math.Tau;

            string color = slice.Color ?? Palette.ColorAt(i);

            RenderArc(builder, ref seq, slice, i, cx, cy, outerR, innerR,
                startAngle, endAngle, color, value, total);
        }
    }

    private void RenderFullDisc(
        RenderTreeBuilder builder, ref int seq,
        BOBChartSlice<TY> slice, int index,
        double cx, double cy, double outerR, double innerR,
        double total)
    {
        string color = slice.Color ?? Palette.ColorAt(index);

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-pie-chart__slice");
        builder.AddAttribute(seq++, "data-bob-slice", slice.Label);

        if (innerR <= 0)
        {
            // Full circle.
            builder.OpenElement(seq++, "circle");
            builder.AddAttribute(seq++, "cx", ChartLayout.ToInvariant(cx));
            builder.AddAttribute(seq++, "cy", ChartLayout.ToInvariant(cy));
            builder.AddAttribute(seq++, "r", ChartLayout.ToInvariant(outerR));
            builder.AddAttribute(seq++, "fill", color);

            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++, $"{slice.Label}: 100%");
            builder.CloseElement();

            builder.CloseElement();
        }
        else
        {
            // Annulus: outer circle minus inner via even-odd fill rule on a
            // composite path (two M+a subpaths).
            string d = $"M {ChartLayout.ToInvariant(cx - outerR)},{ChartLayout.ToInvariant(cy)} "
                       + $"a {ChartLayout.ToInvariant(outerR)},{ChartLayout.ToInvariant(outerR)} 0 1,0 {ChartLayout.ToInvariant(outerR * 2)},0 "
                       + $"a {ChartLayout.ToInvariant(outerR)},{ChartLayout.ToInvariant(outerR)} 0 1,0 {ChartLayout.ToInvariant(-outerR * 2)},0 Z "
                       + $"M {ChartLayout.ToInvariant(cx - innerR)},{ChartLayout.ToInvariant(cy)} "
                       + $"a {ChartLayout.ToInvariant(innerR)},{ChartLayout.ToInvariant(innerR)} 0 1,0 {ChartLayout.ToInvariant(innerR * 2)},0 "
                       + $"a {ChartLayout.ToInvariant(innerR)},{ChartLayout.ToInvariant(innerR)} 0 1,0 {ChartLayout.ToInvariant(-innerR * 2)},0 Z";

            builder.OpenElement(seq++, "path");
            builder.AddAttribute(seq++, "d", d);
            builder.AddAttribute(seq++, "fill", color);
            builder.AddAttribute(seq++, "fill-rule", "evenodd");

            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++, $"{slice.Label}: 100%");
            builder.CloseElement();

            builder.CloseElement();
        }

        builder.CloseElement(); // g
    }

    private void RenderArc(
        RenderTreeBuilder builder, ref int seq,
        BOBChartSlice<TY> slice, int index,
        double cx, double cy, double outerR, double innerR,
        double startAngle, double endAngle,
        string color, double value, double total)
    {
        // Convert polar (centred at cx,cy, angle from -π/2 = 12 o'clock,
        // clockwise) to cartesian. SVG Y grows down so we negate sin.
        double a0 = startAngle - (Math.PI / 2);
        double a1 = endAngle - (Math.PI / 2);

        double outerStartX = cx + (outerR * Math.Cos(a0));
        double outerStartY = cy + (outerR * Math.Sin(a0));
        double outerEndX = cx + (outerR * Math.Cos(a1));
        double outerEndY = cy + (outerR * Math.Sin(a1));
        int largeArc = endAngle - startAngle > Math.PI ? 1 : 0;

        string d;
        if (innerR <= 0)
        {
            // Pie wedge: M(centre) L(outerStart) A(outerEnd) Z.
            d = $"M {ChartLayout.ToInvariant(cx)},{ChartLayout.ToInvariant(cy)} "
                + $"L {ChartLayout.ToInvariant(outerStartX)},{ChartLayout.ToInvariant(outerStartY)} "
                + $"A {ChartLayout.ToInvariant(outerR)},{ChartLayout.ToInvariant(outerR)} 0 {largeArc},1 "
                + $"{ChartLayout.ToInvariant(outerEndX)},{ChartLayout.ToInvariant(outerEndY)} Z";
        }
        else
        {
            // Donut wedge: outer arc forward, inner arc reverse, close.
            double innerStartX = cx + (innerR * Math.Cos(a0));
            double innerStartY = cy + (innerR * Math.Sin(a0));
            double innerEndX = cx + (innerR * Math.Cos(a1));
            double innerEndY = cy + (innerR * Math.Sin(a1));

            d = $"M {ChartLayout.ToInvariant(outerStartX)},{ChartLayout.ToInvariant(outerStartY)} "
                + $"A {ChartLayout.ToInvariant(outerR)},{ChartLayout.ToInvariant(outerR)} 0 {largeArc},1 "
                + $"{ChartLayout.ToInvariant(outerEndX)},{ChartLayout.ToInvariant(outerEndY)} "
                + $"L {ChartLayout.ToInvariant(innerEndX)},{ChartLayout.ToInvariant(innerEndY)} "
                + $"A {ChartLayout.ToInvariant(innerR)},{ChartLayout.ToInvariant(innerR)} 0 {largeArc},0 "
                + $"{ChartLayout.ToInvariant(innerStartX)},{ChartLayout.ToInvariant(innerStartY)} Z";
        }

        double percent = value / total * 100;

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-pie-chart__slice");
        builder.AddAttribute(seq++, "data-bob-slice", slice.Label);

        // Capture for closures (defensive — keeps the event handlers
        // independent of any future loop-variable hoisting).
        BOBChartSlice<TY> capturedSlice = slice;
        int capturedIndex = index;
        double capturedPercent = percent;

        builder.OpenElement(seq++, "path");
        builder.AddAttribute(seq++, "class", "bob-pie-chart__arc");
        builder.AddAttribute(seq++, "d", d);
        builder.AddAttribute(seq++, "fill", color);

        if (OnSliceClick.HasDelegate)
        {
            builder.AddAttribute(seq++, "onclick",
                EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                    this,
                    _ => OnSliceClick.InvokeAsync(new BOBChartSliceClickArgs<TY>
                    {
                        SliceLabel = capturedSlice.Label,
                        Value = capturedSlice.Value,
                        SliceIndex = capturedIndex,
                        Percentage = capturedPercent
                    })));
            builder.AddAttribute(seq++, "cursor", "pointer");
        }

        if (OnSliceHover.HasDelegate)
        {
            builder.AddAttribute(seq++, "onmouseenter",
                EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                    this,
                    _ => OnSliceHover.InvokeAsync(new BOBChartSliceHoverArgs<TY>
                    {
                        SliceLabel = capturedSlice.Label,
                        Value = capturedSlice.Value,
                        SliceIndex = capturedIndex,
                        Percentage = capturedPercent
                    })));
        }

        // Native <title> tooltip.
        builder.OpenElement(seq++, "title");
        builder.AddContent(seq++, string.Format(CultureInfo.InvariantCulture,
            "{0}: {1} ({2:F1}%)", slice.Label, slice.Value, percent));
        builder.CloseElement(); // title

        builder.CloseElement(); // path

        // Optional percentage label at the slice's mid-radius.
        if (ShowPercentages && percent >= 5)
        {
            double midAngle = ((startAngle + endAngle) / 2) - (Math.PI / 2);
            double midRadius = innerR <= 0
                ? outerR * 0.6 // Pie: 60% of outer.
                : (outerR + innerR) / 2; // Donut: midway between rings.
            double labelX = cx + (midRadius * Math.Cos(midAngle));
            double labelY = cy + (midRadius * Math.Sin(midAngle));

            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "bob-pie-chart__percentage");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(labelX));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(labelY + 4));
            builder.AddAttribute(seq++, "text-anchor", "middle");
            builder.AddContent(seq++, string.Format(CultureInfo.InvariantCulture, "{0:F0}%", percent));
            builder.CloseElement();
        }

        builder.CloseElement(); // g
    }

    /// <inheritdoc />
    private protected override IEnumerable<LegendEntry> GetLegendEntries()
    {
        if (Slices is null)
        {
            yield break;
        }

        int i = 0;
        foreach (BOBChartSlice<TY> s in Slices)
        {
            yield return new LegendEntry(s.Label, s.Color ?? Palette.ColorAt(i));
            i++;
        }
    }
}