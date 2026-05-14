using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Box-and-whisker plot — distribution comparison across categorical
/// groups. Each box renders the IQR (Q1..Q3), median line, whiskers
/// (typically <c>±1.5·IQR</c>) and individual outlier dots.
/// </summary>
/// <typeparam name="TX">Categorical X key (group / segment).</typeparam>
public sealed class BOBBoxplotChart<TX> : BOBChartBase<TX, double>
    where TX : notnull
{
    /// <summary>Pre-computed box statistics, one per group.</summary>
    [Parameter]
    public IEnumerable<BOBChartBoxStat<TX>>? Boxes { get; set; }

    /// <summary>Box width as a fraction of the per-group band. Default 0.55.</summary>
    [Parameter]
    public double BoxRatio { get; set; } = 0.55;

    /// <summary>Y-axis explicit min override.</summary>
    [Parameter]
    public double? YMin { get; set; }

    /// <summary>Y-axis explicit max override.</summary>
    [Parameter]
    public double? YMax { get; set; }

    /// <inheritdoc />
    protected override string BuildAriaLabel() => $"Boxplot with {Boxes?.Count() ?? 0} groups";

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Boxes is null)
        {
            return;
        }

        BOBChartBoxStat<TX>[] boxes = Boxes.ToArray();
        if (boxes.Length == 0)
        {
            return;
        }

        ChartLayout layout = ChartLayout.Default(EffectiveWidth, EffectiveHeight);
        IEnumerable<double> allValues = boxes.SelectMany(b =>
            new[] { b.Min, b.Max }.Concat(b.Outliers));
        LinearScale yScale = new(allValues.ToList(),
            layout.PlotBottom, layout.PlotTop, YMin, YMax);

        CategoricalScale<TX> xScale = new(boxes.Select(b => b.X), layout.PlotLeft, layout.PlotRight);
        double bandWidth = xScale.BandWidth;
        double boxWidth = bandWidth * BoxRatio;

        int seq = 100;
        // Y grid + labels.
        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-boxplot-chart__grid");
        foreach (double tick in yScale.Ticks())
        {
            double y = yScale.Project(tick);
            builder.OpenElement(seq++, "line");
            builder.AddAttribute(seq++, "x1", ChartLayout.ToInvariant(layout.PlotLeft));
            builder.AddAttribute(seq++, "x2", ChartLayout.ToInvariant(layout.PlotRight));
            builder.AddAttribute(seq++, "y1", ChartLayout.ToInvariant(y));
            builder.AddAttribute(seq++, "y2", ChartLayout.ToInvariant(y));
            builder.CloseElement();
        }

        builder.CloseElement();

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-boxplot-chart__axis bob-boxplot-chart__axis--y");
        string yFormat = yScale.SuggestedFormat();
        foreach (double tick in yScale.Ticks())
        {
            double y = yScale.Project(tick);
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(layout.PlotLeft - 8));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(y + 4));
            builder.AddAttribute(seq++, "text-anchor", "end");
            builder.AddContent(seq++, tick.ToString(yFormat, CultureInfo.InvariantCulture));
            builder.CloseElement();
        }

        builder.CloseElement();

        // Boxes.
        for (int i = 0; i < boxes.Length; i++)
        {
            BOBChartBoxStat<TX> box = boxes[i];
            string color = box.Color ?? Palette.ColorAt(i);
            double cx = xScale.Center(box.X);
            double left = cx - (boxWidth / 2);

            double yMin = yScale.Project(box.Min);
            double yQ1 = yScale.Project(box.Q1);
            double yMed = yScale.Project(box.Median);
            double yQ3 = yScale.Project(box.Q3);
            double yMax = yScale.Project(box.Max);

            // Whisker line.
            builder.OpenElement(seq++, "line");
            builder.AddAttribute(seq++, "class", "bob-boxplot-chart__whisker");
            builder.AddAttribute(seq++, "x1", ChartLayout.ToInvariant(cx));
            builder.AddAttribute(seq++, "x2", ChartLayout.ToInvariant(cx));
            builder.AddAttribute(seq++, "y1", ChartLayout.ToInvariant(yMin));
            builder.AddAttribute(seq++, "y2", ChartLayout.ToInvariant(yMax));
            builder.AddAttribute(seq++, "stroke", color);
            builder.CloseElement();

            // Min / max caps.
            foreach (double y in new[] { yMin, yMax })
            {
                builder.OpenElement(seq++, "line");
                builder.AddAttribute(seq++, "class", "bob-boxplot-chart__cap");
                builder.AddAttribute(seq++, "x1", ChartLayout.ToInvariant(cx - (boxWidth / 4)));
                builder.AddAttribute(seq++, "x2", ChartLayout.ToInvariant(cx + (boxWidth / 4)));
                builder.AddAttribute(seq++, "y1", ChartLayout.ToInvariant(y));
                builder.AddAttribute(seq++, "y2", ChartLayout.ToInvariant(y));
                builder.AddAttribute(seq++, "stroke", color);
                builder.CloseElement();
            }

            // IQR box.
            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "class", "bob-boxplot-chart__box");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(left));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(yQ3));
            builder.AddAttribute(seq++, "width", ChartLayout.ToInvariant(boxWidth));
            builder.AddAttribute(seq++, "height", ChartLayout.ToInvariant(yQ1 - yQ3));
            builder.AddAttribute(seq++, "fill", color);
            builder.AddAttribute(seq++, "fill-opacity", "0.45");
            builder.AddAttribute(seq++, "stroke", color);
            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++,
                $"{box.X}: min {box.Min:G6} | Q1 {box.Q1:G6} | med {box.Median:G6} | Q3 {box.Q3:G6} | max {box.Max:G6}");
            builder.CloseElement();
            builder.CloseElement();

            // Median line.
            builder.OpenElement(seq++, "line");
            builder.AddAttribute(seq++, "class", "bob-boxplot-chart__median");
            builder.AddAttribute(seq++, "x1", ChartLayout.ToInvariant(left));
            builder.AddAttribute(seq++, "x2", ChartLayout.ToInvariant(left + boxWidth));
            builder.AddAttribute(seq++, "y1", ChartLayout.ToInvariant(yMed));
            builder.AddAttribute(seq++, "y2", ChartLayout.ToInvariant(yMed));
            builder.AddAttribute(seq++, "stroke", color);
            builder.AddAttribute(seq++, "stroke-width", "2");
            builder.CloseElement();

            // Outliers.
            foreach (double o in box.Outliers)
            {
                builder.OpenElement(seq++, "circle");
                builder.AddAttribute(seq++, "class", "bob-boxplot-chart__outlier");
                builder.AddAttribute(seq++, "cx", ChartLayout.ToInvariant(cx));
                builder.AddAttribute(seq++, "cy", ChartLayout.ToInvariant(yScale.Project(o)));
                builder.AddAttribute(seq++, "r", "2.5");
                builder.AddAttribute(seq++, "fill", color);
                builder.AddAttribute(seq++, "fill-opacity", "0.7");
                builder.OpenElement(seq++, "title");
                builder.AddContent(seq++, $"{box.X}: outlier {o:G6}");
                builder.CloseElement();
                builder.CloseElement();
            }
        }

        // X labels.
        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-boxplot-chart__axis bob-boxplot-chart__axis--x");
        foreach (TX cat in xScale.Categories)
        {
            double cxLabel = xScale.Center(cat);
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(cxLabel));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotBottom + 18));
            builder.AddAttribute(seq++, "text-anchor", "middle");
            builder.AddContent(seq++, cat?.ToString() ?? string.Empty);
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}