using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Radial variant of <see cref="BOBTreemapChart{TY}"/>. Hierarchical nodes render as
/// concentric arcs - innermost ring for root nodes, outer rings for their descendants.
/// Shares <see cref="BOBChartTreemapNode{TY}"/> with the treemap so consumers can swap
/// representations without rebuilding their dataset.
/// </summary>
/// <typeparam name="TY">Numeric type of the node values.</typeparam>
public sealed class BOBSunburstChart<TY> : BOBChartBase<object, TY>
{
    /// <summary>Hierarchical dataset rendered as concentric arcs.</summary>
    [Parameter]
    public IEnumerable<BOBChartTreemapNode<TY>>? Nodes { get; set; }

    /// <summary>
    /// Inner-radius / outer-radius ratio in [0, 0.9]. <c>0</c> leaves a solid center,
    /// <c>0.2</c> hollows a small donut hole. Default 0.
    /// </summary>
    [Parameter]
    public double InnerRadius { get; set; }

    /// <summary>
    /// Pixel padding around the chart so labels and outer rings don't bump against the
    /// viewport edge. Default 12.
    /// </summary>
    [Parameter]
    public double Padding { get; set; } = 12;

    /// <summary>
    /// Minimum arc angle (degrees) below which the label is skipped. Tiny wedges hide
    /// their labels to avoid clutter. Default 12°.
    /// </summary>
    [Parameter]
    public double MinLabelArc { get; set; } = 12;

    /// <summary>
    /// Fired when the user clicks any arc (leaf or branch). Carries the node, its
    /// root-to-self path and its depth in the tree.
    /// </summary>
    [Parameter]
    public EventCallback<BOBChartTreemapClickArgs<TY>> OnNodeClick { get; set; }

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int count = Nodes?.Count() ?? 0;
        return count == 0
            ? "Sunburst chart with no data"
            : $"Sunburst chart with {count} root nodes";
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Nodes is null)
        {
            return;
        }

        List<BOBChartTreemapNode<TY>> roots = Nodes
            .Where(n => !IsSeriesHidden(n.Label))
            .ToList();
        if (roots.Count == 0)
        {
            return;
        }

        double svgW = EffectiveWidth;
        double svgH = EffectiveHeight;
        double cx = svgW / 2;
        double cy = svgH / 2;
        double outerR = Math.Max(0, (Math.Min(svgW, svgH) / 2) - Padding);
        if (outerR <= 0)
        {
            return;
        }

        double inner = Math.Clamp(InnerRadius, 0, 0.9);
        double r0 = outerR * inner;
        int maxDepth = MeasureDepth(roots);
        if (maxDepth <= 0)
        {
            return;
        }

        double ringWidth = (outerR - r0) / maxDepth;
        double totalWeight = SumWeights(roots);
        if (totalWeight <= 0)
        {
            return;
        }

        int seq = 100;
        RenderRing(builder, ref seq, roots, cx, cy, r0, ringWidth,
            startAngle: 0, endAngle: 360, depth: 0, parentPath: []);
    }

    private void RenderRing(
        RenderTreeBuilder builder,
        ref int seq,
        IReadOnlyList<BOBChartTreemapNode<TY>> nodes,
        double cx, double cy, double baseRadius, double ringWidth,
        double startAngle, double endAngle, int depth,
        IReadOnlyList<string> parentPath)
    {
        double total = SumWeights(nodes);
        if (total <= 0)
        {
            return;
        }

        double range = endAngle - startAngle;
        double cursor = startAngle;
        double innerR = baseRadius + (depth * ringWidth);
        double outerR = innerR + ringWidth;

        for (int i = 0; i < nodes.Count; i++)
        {
            BOBChartTreemapNode<TY> node = nodes[i];
            double weight = AggregateValue(node);
            if (weight <= 0)
            {
                continue;
            }

            double sweep = range * weight / total;
            double a0 = cursor;
            double a1 = cursor + sweep;
            cursor = a1;

            string color = node.Color ?? Palette.ColorAt(depth == 0 ? i : depth);
            string path = ArcPath(cx, cy, innerR, outerR, a0, a1);
            string[] nodePath = [.. parentPath, node.Label];
            BOBChartTreemapNode<TY> capturedNode = node;
            string[] capturedPath = nodePath;
            int capturedDepth = depth;

            builder.OpenElement(seq++, "path");
            builder.AddAttribute(seq++, "class", "bob-sunburst-chart__arc");
            builder.AddAttribute(seq++, "d", path);
            builder.AddAttribute(seq++, "fill", color);
            builder.AddAttribute(seq++, "stroke", "var(--palette-surface, white)");
            builder.AddAttribute(seq++, "stroke-width", "1");
            if (OnNodeClick.HasDelegate)
            {
                builder.AddAttribute(seq++, "onclick",
                    EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                        this,
                        _ => OnNodeClick.InvokeAsync(new BOBChartTreemapClickArgs<TY>(
                            capturedNode, capturedPath, capturedDepth))));
                builder.AddAttribute(seq++, "style", "cursor: pointer;");
            }

            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++, string.Format(CultureInfo.InvariantCulture, "{0}: {1:N0}", node.Label, weight));
            builder.CloseElement();
            builder.CloseElement();

            // Label rendered at the arc's centroid when the wedge is wide enough.
            if (sweep >= MinLabelArc)
            {
                double midAngle = (a0 + a1) / 2;
                double midRadius = (innerR + outerR) / 2;
                (double labelX, double labelY) = PolarToCartesian(cx, cy, midRadius, midAngle);
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "class", "bob-sunburst-chart__label");
                builder.AddAttribute(seq++, "x", Squarified.F(labelX));
                builder.AddAttribute(seq++, "y", Squarified.F(labelY + 4));
                builder.AddAttribute(seq++, "text-anchor", "middle");
                builder.AddAttribute(seq++, "fill", "white");
                builder.AddAttribute(seq++, "pointer-events", "none");
                builder.AddAttribute(seq++, "font-size", "11");
                builder.AddContent(seq++, node.Label);
                builder.CloseElement();
            }

            // Recurse into children - they occupy the same angle range on the next ring.
            if (node.Children is { Count: > 0 } children)
            {
                RenderRing(builder, ref seq, children, cx, cy, baseRadius, ringWidth,
                    a0, a1, depth + 1, nodePath);
            }
        }
    }

    private static double AggregateValue(BOBChartTreemapNode<TY> node)
    {
        // Branch nodes derive their magnitude from descendants - sum children first.
        // See BOBTreemapChart.AggregateValue for the full rationale: TY is unconstrained
        // so an unset value-typed Value defaults to 0 and `is not null` can't tell
        // "no value" from "explicit 0".
        if (node.Children is { Count: > 0 } kids)
        {
            double sum = 0;
            foreach (BOBChartTreemapNode<TY> child in kids)
            {
                sum += AggregateValue(child);
            }

            return sum;
        }

        if (node.Value is not null)
        {
            return Math.Max(0, Numeric.ToDouble(node.Value));
        }

        return 0;
    }

    private static double SumWeights(IReadOnlyList<BOBChartTreemapNode<TY>> nodes)
    {
        double sum = 0;
        foreach (BOBChartTreemapNode<TY> n in nodes)
        {
            sum += AggregateValue(n);
        }

        return sum;
    }

    private static int MeasureDepth(IReadOnlyList<BOBChartTreemapNode<TY>> nodes)
    {
        int best = 1;
        foreach (BOBChartTreemapNode<TY> n in nodes)
        {
            int local = 1 + (n.Children is { Count: > 0 } kids ? MeasureDepth(kids) : 0);
            if (local > best)
            {
                best = local;
            }
        }

        return best;
    }

    // SVG arc path: M outerStart → A outerEnd → L innerEnd → A innerStart → Z.
    // Sweep flags are picked so the inner arc reverses direction (CCW) - that's what
    // closes the wedge cleanly without rendering an inner cap.
    private static string ArcPath(double cx, double cy, double r0, double r1, double a0, double a1)
    {
        (double x0Outer, double y0Outer) = PolarToCartesian(cx, cy, r1, a0);
        (double x1Outer, double y1Outer) = PolarToCartesian(cx, cy, r1, a1);
        (double x0Inner, double y0Inner) = PolarToCartesian(cx, cy, r0, a0);
        (double x1Inner, double y1Inner) = PolarToCartesian(cx, cy, r0, a1);
        int largeArc = a1 - a0 > 180 ? 1 : 0;

        if (r0 <= 0)
        {
            // Pie-style wedge - straight line from outer end back to centre.
            return string.Format(CultureInfo.InvariantCulture,
                "M {0:F2} {1:F2} A {2:F2} {2:F2} 0 {3} 1 {4:F2} {5:F2} L {6:F2} {7:F2} Z",
                x0Outer, y0Outer, r1, largeArc, x1Outer, y1Outer, cx, cy);
        }

        return string.Format(CultureInfo.InvariantCulture,
            "M {0:F2} {1:F2} A {2:F2} {2:F2} 0 {3} 1 {4:F2} {5:F2} L {6:F2} {7:F2} A {8:F2} {8:F2} 0 {3} 0 {9:F2} {10:F2} Z",
            x0Outer, y0Outer, r1, largeArc, x1Outer, y1Outer,
            x1Inner, y1Inner, r0, x0Inner, y0Inner);
    }

    // 12-o'clock = 0°, growing clockwise; convert to SVG (right = 0°, growing CW).
    private static (double X, double Y) PolarToCartesian(double cx, double cy, double r, double angleDeg)
    {
        double rad = (angleDeg - 90) * Math.PI / 180.0;
        return (cx + (r * Math.Cos(rad)), cy + (r * Math.Sin(rad)));
    }
}
