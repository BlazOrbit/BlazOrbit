using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Funnel chart — visualizes a multi-stage drop-off pipeline. Each step's
/// row width auto-scales to the largest value in the funnel; the row
/// height is uniform. Useful for sales pipelines, ad attribution funnels
/// and onboarding completion analyses.
/// </summary>
/// <typeparam name="TY">Numeric value type for each step.</typeparam>
public sealed class BOBFunnelChart<TY> : BOBChartBase<int, TY>
    where TY : struct
{
    /// <summary>Funnel stages, in top-to-bottom order.</summary>
    [Parameter]
    public IEnumerable<BOBChartFunnelStep<TY>>? Steps { get; set; }

    /// <summary>
    /// When <c>true</c> (default) the rows render as trapezoids that taper
    /// down (classic funnel). Set to <c>false</c> for a horizontal-bar
    /// look with rectangular rows.
    /// </summary>
    [Parameter]
    public bool Tapered { get; set; } = true;

    /// <summary>
    /// Show the per-step value (and optional drop-off %) inside each row.
    /// Default <c>true</c>.
    /// </summary>
    [Parameter]
    public bool ShowValues { get; set; } = true;

    /// <summary>Pixel gap between consecutive rows. Default 2.</summary>
    [Parameter]
    public double RowGap { get; set; } = 2;

    /// <summary>Fired when the user clicks a funnel row.</summary>
    [Parameter]
    public EventCallback<BOBChartFunnelStep<TY>> OnStepClick { get; set; }

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int n = Steps?.Count() ?? 0;
        return n == 0 ? "Funnel chart with no data" : $"Funnel chart with {n} steps";
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Steps is null)
        {
            return;
        }

        BOBChartFunnelStep<TY>[] steps = Steps.ToArray();
        if (steps.Length == 0)
        {
            return;
        }

        ChartLayout layout = ChartLayout.Default(EffectiveWidth, EffectiveHeight);
        double[] vals = steps.Select(s => Convert.ToDouble(s.Value, CultureInfo.InvariantCulture)).ToArray();
        double maxVal = vals.Max();
        if (maxVal <= 0)
        {
            return;
        }

        double totalH = layout.PlotHeight - ((steps.Length - 1) * RowGap);
        double rowH = totalH / steps.Length;
        double midX = (layout.PlotLeft + layout.PlotRight) / 2;

        int seq = 100;
        for (int i = 0; i < steps.Length; i++)
        {
            double topW = vals[i] / maxVal * layout.PlotWidth;
            double botW = Tapered && i < steps.Length - 1
                ? vals[i + 1] / maxVal * layout.PlotWidth
                : topW;

            double y0 = layout.PlotTop + (i * (rowH + RowGap));
            double y1 = y0 + rowH;
            double x0L = midX - (topW / 2);
            double x0R = midX + (topW / 2);
            double x1L = midX - (botW / 2);
            double x1R = midX + (botW / 2);

            string color = steps[i].Color ?? Palette.ColorAt(i);
            BOBChartFunnelStep<TY> capturedStep = steps[i];

            string d = string.Format(CultureInfo.InvariantCulture,
                "M {0:F2} {1:F2} L {2:F2} {1:F2} L {3:F2} {4:F2} L {5:F2} {4:F2} Z",
                x0L, y0, x0R, x1R, y1, x1L);

            builder.OpenElement(seq++, "path");
            builder.AddAttribute(seq++, "class", "bob-funnel-chart__step");
            builder.AddAttribute(seq++, "d", d);
            builder.AddAttribute(seq++, "fill", color);
            if (OnStepClick.HasDelegate)
            {
                builder.AddAttribute(seq++, "onclick",
                    EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                        this, _ => OnStepClick.InvokeAsync(capturedStep)));
            }

            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++, $"{steps[i].Label}: {steps[i].Value}");
            builder.CloseElement();
            builder.CloseElement();

            // Centered label inside the row.
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "bob-funnel-chart__label");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(midX));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(y0 + (rowH / 2) + 5));
            builder.AddAttribute(seq++, "text-anchor", "middle");
            builder.AddAttribute(seq++, "fill", "white");
            builder.AddAttribute(seq++, "pointer-events", "none");
            builder.AddContent(seq++, ShowValues
                ? FormatRow(steps[i].Label, vals[i], i == 0 ? null : vals[0])
                : steps[i].Label);
            builder.CloseElement();
        }
    }

    private static string FormatRow(string label, double value, double? topValue)
    {
        if (topValue is null || topValue.Value <= 0)
        {
            return $"{label} — {value:N0}";
        }

        double pct = value / topValue.Value * 100;
        return $"{label} — {value:N0} ({pct:F1}%)";
    }
}