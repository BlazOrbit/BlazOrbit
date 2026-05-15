using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Polar-area / Coxcomb / Rose chart - variant of pie where every slice
/// has the same angular width but the radius encodes the magnitude.
/// Useful for cyclic data (months, hours-of-day) where you want to
/// preserve uniform angular slots and let the radii do the talking.
/// </summary>
/// <typeparam name="TY">Numeric value type.</typeparam>
public sealed class BOBPolarAreaChart<TY> : BOBChartBase<int, TY>
    where TY : struct
{
    /// <summary>Slice values (one wedge per item, equal angular width).</summary>
    [Parameter]
    public IEnumerable<BOBChartSlice<TY>>? Slices { get; set; }

    /// <summary>Pixel padding from SVG edge to the outer radius. Default 32.</summary>
    [Parameter]
    public double Padding { get; set; } = 32;

    /// <summary>Number of concentric grid rings drawn for reference.</summary>
    [Parameter]
    public int RingCount { get; set; } = 4;

    /// <summary>Slice fill opacity (0..1). Default 0.7.</summary>
    [Parameter]
    public double FillOpacity { get; set; } = 0.7;

    /// <summary>Render the slice value at its centroid. Default <c>true</c>.</summary>
    [Parameter]
    public bool ShowValues { get; set; } = true;

    /// <inheritdoc />
    protected override string BuildAriaLabel() => $"Polar area chart with {Slices?.Count() ?? 0} slices";

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Slices is null)
        {
            return;
        }

        BOBChartSlice<TY>[] slices = Slices.ToArray();
        if (slices.Length == 0)
        {
            return;
        }

        double[] vals = slices.Select(s => Convert.ToDouble(s.Value, CultureInfo.InvariantCulture)).ToArray();
        double maxV = vals.Max();
        if (maxV <= 0)
        {
            return;
        }

        double w = EffectiveWidth;
        double h = EffectiveHeight;
        double cx = w / 2;
        double cy = h / 2;
        double radius = (Math.Min(w, h) / 2) - Padding;
        double slot = 2 * Math.PI / slices.Length;

        int seq = 100;

        // Concentric reference rings.
        for (int r = 1; r <= RingCount; r++)
        {
            double rPx = radius * r / RingCount;
            builder.OpenElement(seq++, "circle");
            builder.AddAttribute(seq++, "class", "bob-polar-area-chart__ring");
            builder.AddAttribute(seq++, "cx", ChartLayout.ToInvariant(cx));
            builder.AddAttribute(seq++, "cy", ChartLayout.ToInvariant(cy));
            builder.AddAttribute(seq++, "r", ChartLayout.ToInvariant(rPx));
            builder.AddAttribute(seq++, "fill", "none");
            builder.AddAttribute(seq++, "stroke", "var(--palette-border, #d1d5db)");
            builder.AddAttribute(seq++, "stroke-dasharray", "2 3");
            builder.CloseElement();
        }

        // Wedges.
        for (int i = 0; i < slices.Length; i++)
        {
            double rPx = radius * vals[i] / maxV;
            double a0 = (-Math.PI / 2) + (i * slot);
            double a1 = a0 + slot;
            double x0 = cx + (rPx * Math.Cos(a0));
            double y0 = cy + (rPx * Math.Sin(a0));
            double x1 = cx + (rPx * Math.Cos(a1));
            double y1 = cy + (rPx * Math.Sin(a1));
            int large = slot > Math.PI ? 1 : 0;
            string color = slices[i].Color ?? Palette.ColorAt(i);

            string d = string.Format(CultureInfo.InvariantCulture,
                "M {0:F2} {1:F2} L {2:F3} {3:F3} A {4:F3} {4:F3} 0 {5} 1 {6:F3} {7:F3} Z",
                cx, cy, x0, y0, rPx, large, x1, y1);

            builder.OpenElement(seq++, "path");
            builder.AddAttribute(seq++, "class", "bob-polar-area-chart__wedge");
            builder.AddAttribute(seq++, "d", d);
            builder.AddAttribute(seq++, "fill", color);
            builder.AddAttribute(seq++, "fill-opacity", ChartLayout.ToInvariant(FillOpacity));
            builder.AddAttribute(seq++, "stroke", color);
            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++, $"{slices[i].Label}: {slices[i].Value}");
            builder.CloseElement();
            builder.CloseElement();

            if (ShowValues)
            {
                double aMid = a0 + (slot / 2);
                double rLabel = rPx * 0.65;
                double xL = cx + (rLabel * Math.Cos(aMid));
                double yL = cy + (rLabel * Math.Sin(aMid));
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "class", "bob-polar-area-chart__value");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(xL));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(yL + 4));
                builder.AddAttribute(seq++, "text-anchor", "middle");
                builder.AddAttribute(seq++, "fill", "white");
                builder.AddAttribute(seq++, "pointer-events", "none");
                builder.AddContent(seq++, slices[i].Label);
                builder.CloseElement();
            }
        }
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