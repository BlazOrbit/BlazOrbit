using BlazOrbit.Charts.Abstractions;
using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Scatter / bubble chart. Each data point renders as an independent marker
/// with no inter-point connection.
/// <list type="bullet">
/// <item><description><see cref="Series"/> - XY scatter with a constant
///     <see cref="MarkerRadius"/> per point.</description></item>
/// <item><description><see cref="BubbleSeries"/> - bubble mode where the
///     marker radius encodes a third dimension. Per-point sizes are auto-
///     scaled into <see cref="BubbleMinRadius"/>..<see cref="BubbleMaxRadius"/>
///     across all bubble series in the chart.</description></item>
/// </list>
/// Both parameter sets can coexist (mixed scatter + bubble overlays).
/// </summary>
/// <typeparam name="TX">X-axis domain type.</typeparam>
/// <typeparam name="TY">Numeric Y-axis domain type.</typeparam>
public class BOBScatterChart<TX, TY> :
    BOBChartBase<TX, TY>,
    IHasSeries<TX, TY>,
    IHasAxes
    where TX : notnull
{
    /// <inheritdoc />
    [Parameter]
    public IEnumerable<BOBChartSeries<TX, TY>>? Series { get; set; }

    /// <summary>
    /// Optional bubble series rendered alongside <see cref="Series"/>. Each
    /// point's <see cref="BOBChartBubblePoint{TX, TY}.Size"/> is auto-scaled
    /// across the union of all bubble points into pixel radii.
    /// </summary>
    [Parameter]
    public IEnumerable<BOBChartBubbleSeries<TX, TY>>? BubbleSeries { get; set; }

    /// <inheritdoc />
    [Parameter]
    public BOBChartAxis XAxis { get; set; } = new();

    /// <inheritdoc />
    [Parameter]
    public BOBChartAxis YAxis { get; set; } = new();

    /// <summary>Pixel radius of the per-point markers in <see cref="Series"/> mode.</summary>
    [Parameter]
    public double MarkerRadius { get; set; } = 5;

    /// <summary>Lower clamp for bubble radii (px) when mapping <see cref="BOBChartBubblePoint{TX, TY}.Size"/> across <see cref="BubbleSeries"/>.</summary>
    [Parameter]
    public double BubbleMinRadius { get; set; } = 4;

    /// <summary>Upper clamp for bubble radii (px) when mapping <see cref="BOBChartBubblePoint{TX, TY}.Size"/> across <see cref="BubbleSeries"/>.</summary>
    [Parameter]
    public double BubbleMaxRadius { get; set; } = 28;

    /// <summary>
    /// Marker fill opacity (0..1). Default 0.7 keeps overlapping points
    /// individually distinguishable without sacrificing color identity.
    /// </summary>
    [Parameter]
    public double MarkerOpacity { get; set; } = 0.7;

    /// <summary>Optional reference / threshold lines drawn across the plot.</summary>
    [Parameter]
    public IEnumerable<BOBChartReferenceLine>? ReferenceLines { get; set; }

    /// <summary>Optional annotations (text / band / shape) layered on top of the markers.</summary>
    [Parameter]
    public IEnumerable<BOBChartAnnotation>? Annotations { get; set; }

    /// <summary>Fired when the user clicks a marker.</summary>
    [Parameter]
    public EventCallback<BOBChartClickArgs<TX, TY>> OnPointClick { get; set; }

    /// <summary>Fired when the user hovers a marker (mouseenter).</summary>
    [Parameter]
    public EventCallback<BOBChartHoverArgs<TX, TY>> OnDataHover { get; set; }

    private List<int>? _scatterOriginalIndices;
    private List<int>? _bubbleOriginalIndices;

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int sCount = Series?.Count() ?? 0;
        int bCount = BubbleSeries?.Count() ?? 0;
        int total = sCount + bCount;
        return total switch
        {
            0 => "Scatter chart with no data",
            1 => "Scatter chart with one series",
            _ => $"Scatter chart with {total} series"
        };
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        bool hasScatter = Series is not null && Series.Any();
        bool hasBubble = BubbleSeries is not null && BubbleSeries.Any();
        if (!hasScatter && !hasBubble)
        {
            return;
        }

        // Filter visible series + capture original indices so legend toggles
        // don't shift palette colors.
        List<BOBChartSeries<TX, TY>> scatterList = [];
        List<int> scatterOrig = [];
        if (hasScatter)
        {
            List<BOBChartSeries<TX, TY>> all = Series!.ToList();
            for (int i = 0; i < all.Count; i++)
            {
                if (!IsSeriesHidden(all[i].Label))
                {
                    scatterList.Add(all[i]);
                    scatterOrig.Add(i);
                }
            }
        }

        List<BOBChartBubbleSeries<TX, TY>> bubbleList = [];
        List<int> bubbleOrig = [];
        if (hasBubble)
        {
            List<BOBChartBubbleSeries<TX, TY>> all = BubbleSeries!.ToList();
            // Bubble series share the palette index space with scatter so
            // mixing both keeps colors deterministic.
            int offset = Series?.Count() ?? 0;
            for (int i = 0; i < all.Count; i++)
            {
                if (!IsSeriesHidden(all[i].Label))
                {
                    bubbleList.Add(all[i]);
                    bubbleOrig.Add(offset + i);
                }
            }
        }

        _scatterOriginalIndices = scatterOrig;
        _bubbleOriginalIndices = bubbleOrig;

        if (scatterList.Count == 0 && bubbleList.Count == 0)
        {
            return;
        }

        double svgWidth = EffectiveWidth;
        double svgHeight = EffectiveHeight;
        ChartLayout layout = ChartLayout.Default(svgWidth, svgHeight);

        // Y domain - union of all visible point Ys across both series sets.
        List<double> allY = scatterList.SelectMany(s => s.Points.Select(p => Numeric.ToDouble(p.Y)))
            .Concat(bubbleList.SelectMany(b => b.Points.Select(p => Numeric.ToDouble(p.Y))))
            .ToList();
        if (allY.Count == 0)
        {
            return;
        }

        LinearScale yScale = new(allY, layout.PlotBottom, layout.PlotTop, YAxis.Min, YAxis.Max);

        // X scale - continuous (numeric / temporal) or categorical fallback.
        bool xContinuous = Numeric.IsContinuous<TX>();
        LinearScale? xLinear = null;
        CategoricalScale<TX>? xCat = null;
        if (xContinuous)
        {
            List<double> allX = scatterList.SelectMany(s => s.Points.Select(p => Numeric.ToDouble(p.X)))
                .Concat(bubbleList.SelectMany(b => b.Points.Select(p => Numeric.ToDouble(p.X))))
                .ToList();
            xLinear = new LinearScale(allX, layout.PlotLeft, layout.PlotRight, XAxis.Min, XAxis.Max);
        }
        else
        {
            IEnumerable<TX> categories = scatterList.SelectMany(s => s.Points.Select(p => p.X))
                .Concat(bubbleList.SelectMany(b => b.Points.Select(p => p.X)))
                .Distinct();
            xCat = new CategoricalScale<TX>(categories, layout.PlotLeft, layout.PlotRight);
        }

        // Bubble radius scale - auto from min/max of all bubble sizes.
        (double sMin, double sMax) = bubbleList.Count == 0
            ? (0, 1)
            : (bubbleList.SelectMany(b => b.Points).Min(p => p.Size),
                bubbleList.SelectMany(b => b.Points).Max(p => p.Size));
        if (Math.Abs(sMax - sMin) < double.Epsilon)
        {
            sMax = sMin + 1;
        }

        int seq = 100;
        RenderGrid(builder, ref seq, layout, yScale);
        RenderYAxisLabels(builder, ref seq, layout, yScale);
        RenderXAxisLabels(builder, ref seq, layout, xLinear, xCat);

        RenderScatterSeries(builder, ref seq, scatterList, scatterOrig, layout, yScale, xLinear, xCat);
        RenderBubbleSeries(builder, ref seq, bubbleList, bubbleOrig, layout, yScale, xLinear, xCat, sMin, sMax);

        // Reference lines on top of the data - thresholds need to stay legible.
        ReferenceLineRenderer.Render(builder, ref seq, layout, yScale, ReferenceLines);

        if (Annotations is not null)
        {
            RenderAnnotations(builder, ref seq, layout, yScale, xLinear, xCat);
        }
    }

    private void RenderGrid(RenderTreeBuilder builder, ref int seq, ChartLayout layout, LinearScale yScale)
    {
        if (!YAxis.ShowGrid)
        {
            return;
        }

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-scatter-chart__grid");
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
    }

    private void RenderYAxisLabels(RenderTreeBuilder builder, ref int seq, ChartLayout layout, LinearScale yScale)
    {
        if (!YAxis.ShowLabels)
        {
            return;
        }

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-scatter-chart__axis bob-scatter-chart__axis--y");
        string format = YAxis.Format ?? yScale.SuggestedFormat();
        foreach (double tick in yScale.Ticks())
        {
            double y = yScale.Project(tick);
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(layout.PlotLeft - 8));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(y + 4));
            builder.AddAttribute(seq++, "text-anchor", "end");
            builder.AddContent(seq++, tick.ToString(format, CultureInfo.InvariantCulture));
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    private void RenderXAxisLabels(
        RenderTreeBuilder builder, ref int seq, ChartLayout layout,
        LinearScale? xLinear, CategoricalScale<TX>? xCat)
    {
        if (!XAxis.ShowLabels)
        {
            return;
        }

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-scatter-chart__axis bob-scatter-chart__axis--x");
        if (xLinear is not null)
        {
            foreach (double tick in xLinear.Ticks())
            {
                double x = xLinear.Project(tick);
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(x));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotBottom + 18));
                builder.AddAttribute(seq++, "text-anchor", "middle");
                builder.AddContent(seq++, Numeric.FormatContinuous<TX>(tick, XAxis.Format, xLinear.SuggestedFormat()));
                builder.CloseElement();
            }
        }
        else if (xCat is not null)
        {
            foreach (TX category in xCat.Categories)
            {
                double x = xCat.Center(category);
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(x));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotBottom + 18));
                builder.AddAttribute(seq++, "text-anchor", "middle");
                builder.AddContent(seq++, category?.ToString() ?? string.Empty);
                builder.CloseElement();
            }
        }

        builder.CloseElement();
    }

    private void RenderScatterSeries(
        RenderTreeBuilder builder, ref int seq,
        List<BOBChartSeries<TX, TY>> seriesList, List<int> originalIndices,
        ChartLayout layout, LinearScale yScale,
        LinearScale? xLinear, CategoricalScale<TX>? xCat)
    {
        for (int s = 0; s < seriesList.Count; s++)
        {
            BOBChartSeries<TX, TY> series = seriesList[s];
            int paletteIdx = s < originalIndices.Count ? originalIndices[s] : s;
            string color = series.Color ?? Palette.ColorAt(paletteIdx);

            builder.OpenElement(seq++, "g");
            builder.AddAttribute(seq++, "class", "bob-scatter-chart__series");
            builder.AddAttribute(seq++, "data-bob-series", series.Label);

            int pointIndex = 0;
            foreach (BOBChartPoint<TX, TY> pt in series.Points)
            {
                double cx = xLinear is not null
                    ? xLinear.Project(Numeric.ToDouble(pt.X))
                    : xCat!.Center(pt.X);
                double cy = yScale.Project(Numeric.ToDouble(pt.Y));

                BOBChartSeries<TX, TY> capturedSeries = series;
                BOBChartPoint<TX, TY> capturedPt = pt;
                int capturedIdx = pointIndex;

                builder.OpenElement(seq++, "circle");
                builder.AddAttribute(seq++, "class", "bob-scatter-chart__marker");
                builder.AddAttribute(seq++, "cx", ChartLayout.ToInvariant(cx));
                builder.AddAttribute(seq++, "cy", ChartLayout.ToInvariant(cy));
                builder.AddAttribute(seq++, "r", ChartLayout.ToInvariant(MarkerRadius));
                builder.AddAttribute(seq++, "fill", color);
                builder.AddAttribute(seq++, "fill-opacity", ChartLayout.ToInvariant(MarkerOpacity));
                builder.AddAttribute(seq++, "stroke", color);
                if (OnPointClick.HasDelegate)
                {
                    builder.AddAttribute(seq++, "onclick",
                        EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                            this,
                            _ => OnPointClick.InvokeAsync(new BOBChartClickArgs<TX, TY>
                            {
                                SeriesLabel = capturedSeries.Label,
                                X = capturedPt.X,
                                Y = capturedPt.Y,
                                PointIndex = capturedIdx
                            })));
                }

                if (OnDataHover.HasDelegate)
                {
                    builder.AddAttribute(seq++, "onmouseenter",
                        EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                            this,
                            _ => OnDataHover.InvokeAsync(new BOBChartHoverArgs<TX, TY>
                            {
                                SeriesLabel = capturedSeries.Label,
                                X = capturedPt.X,
                                Y = capturedPt.Y,
                                PointIndex = capturedIdx
                            })));
                }

                builder.OpenElement(seq++, "title");
                builder.AddContent(seq++, $"{series.Label}: ({pt.X}, {pt.Y})");
                builder.CloseElement();
                builder.CloseElement(); // circle

                pointIndex++;
            }

            builder.CloseElement(); // g
        }
    }

    private void RenderBubbleSeries(
        RenderTreeBuilder builder, ref int seq,
        List<BOBChartBubbleSeries<TX, TY>> seriesList, List<int> originalIndices,
        ChartLayout layout, LinearScale yScale,
        LinearScale? xLinear, CategoricalScale<TX>? xCat,
        double sizeMin, double sizeMax)
    {
        for (int s = 0; s < seriesList.Count; s++)
        {
            BOBChartBubbleSeries<TX, TY> series = seriesList[s];
            int paletteIdx = s < originalIndices.Count ? originalIndices[s] : s;
            string color = series.Color ?? Palette.ColorAt(paletteIdx);

            builder.OpenElement(seq++, "g");
            builder.AddAttribute(seq++, "class", "bob-scatter-chart__series bob-scatter-chart__series--bubble");
            builder.AddAttribute(seq++, "data-bob-series", series.Label);

            foreach (BOBChartBubblePoint<TX, TY> pt in series.Points)
            {
                double cx = xLinear is not null
                    ? xLinear.Project(Numeric.ToDouble(pt.X))
                    : xCat!.Center(pt.X);
                double cy = yScale.Project(Numeric.ToDouble(pt.Y));
                double t = (pt.Size - sizeMin) / (sizeMax - sizeMin);
                double r = BubbleMinRadius + (t * (BubbleMaxRadius - BubbleMinRadius));

                builder.OpenElement(seq++, "circle");
                builder.AddAttribute(seq++, "class", "bob-scatter-chart__bubble");
                builder.AddAttribute(seq++, "cx", ChartLayout.ToInvariant(cx));
                builder.AddAttribute(seq++, "cy", ChartLayout.ToInvariant(cy));
                builder.AddAttribute(seq++, "r", ChartLayout.ToInvariant(r));
                builder.AddAttribute(seq++, "fill", color);
                builder.AddAttribute(seq++, "fill-opacity", ChartLayout.ToInvariant(MarkerOpacity * 0.6));
                builder.AddAttribute(seq++, "stroke", color);
                builder.OpenElement(seq++, "title");
                builder.AddContent(seq++, $"{series.Label}: ({pt.X}, {pt.Y}) size={pt.Size}");
                builder.CloseElement();
                builder.CloseElement();
            }

            builder.CloseElement();
        }
    }

    private void RenderAnnotations(
        RenderTreeBuilder builder, ref int seq, ChartLayout layout,
        LinearScale yScale, LinearScale? xLinear, CategoricalScale<TX>? xCat)
    {
        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "bob-chart__annotations");
        foreach (BOBChartAnnotation ann in Annotations!)
        {
            // Reuse the same lightweight rendering as BOBLineChart for
            // consistency. Only a subset is supported here (no text-anchor
            // tweaking) - extend if scatter-specific layouts are needed.
            switch (ann)
            {
                case BOBChartTextAnnotation<TX, TY> text:
                    {
                        double? cxn = xLinear is not null ? xLinear.Project(Numeric.ToDouble(text.X)) :
                            xCat is not null ? xCat.Center(text.X) : (double?)null;
                        if (cxn is null)
                        {
                            break;
                        }

                        double cyn = yScale.Project(Numeric.ToDouble(text.Y));
                        builder.OpenElement(seq++, "text");
                        builder.AddAttribute(seq++, "class", "bob-chart__annotation-text");
                        builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(cxn.Value + text.DxPx));
                        builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(cyn + text.DyPx));
                        builder.AddAttribute(seq++, "fill", text.Color ?? "var(--palette-surface-contrast, #1f2937)");
                        builder.AddContent(seq++, text.Text);
                        builder.CloseElement();
                        break;
                    }
            }
        }

        builder.CloseElement();
    }

    /// <inheritdoc />
    private protected override IEnumerable<LegendEntry> GetLegendEntries()
    {
        if (Series is not null)
        {
            int i = 0;
            foreach (BOBChartSeries<TX, TY> s in Series)
            {
                yield return new LegendEntry(s.Label, s.Color ?? Palette.ColorAt(i));
                i++;
            }
        }

        if (BubbleSeries is not null)
        {
            int offset = Series?.Count() ?? 0;
            int i = 0;
            foreach (BOBChartBubbleSeries<TX, TY> s in BubbleSeries)
            {
                yield return new LegendEntry(s.Label, s.Color ?? Palette.ColorAt(offset + i));
                i++;
            }
        }
    }
}