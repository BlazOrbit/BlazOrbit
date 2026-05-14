using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Hierarchical treemap — subdivides the chart area into nested rectangles whose size
/// is proportional to each node's value. Uses the squarified algorithm
/// (Bruls / Huijing / van Wijk, 2000) so rectangles stay close to square, making
/// adjacent magnitudes easier to compare than the long thin strips of the classic
/// slice-and-dice approach.
/// </summary>
/// <typeparam name="TY">Numeric type of the node values.</typeparam>
public sealed class BOBTreemapChart<TY> : BOBChartBase<object, TY>
{
    /// <summary>Hierarchical dataset rendered into nested rectangles.</summary>
    [Parameter]
    public IEnumerable<BOBChartTreemapNode<TY>>? Nodes { get; set; }

    /// <summary>
    /// Pixel padding inside each branch rect so child rectangles don't bleed into the
    /// parent's border. Default 2.
    /// </summary>
    [Parameter]
    public double Padding { get; set; } = 2;

    /// <summary>
    /// Minimum width / height (in pixels) a cell must reach before its label is drawn.
    /// Smaller cells skip the label to avoid visual clutter. Default 32.
    /// </summary>
    [Parameter]
    public double MinLabelSize { get; set; } = 32;

    /// <summary>
    /// Fired when the user clicks any cell (leaf or branch). Carries the node, its
    /// root-to-self path and its depth in the tree.
    /// </summary>
    [Parameter]
    public EventCallback<BOBChartTreemapClickArgs<TY>> OnNodeClick { get; set; }

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int count = Nodes?.Count() ?? 0;
        return count == 0
            ? "Treemap chart with no data"
            : $"Treemap chart with {count} root nodes";
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

        SquarifiedRect viewport = new(0, 0, EffectiveWidth, EffectiveHeight);
        int seq = 100;
        RenderLevel(builder, ref seq, roots, viewport, depth: 0, parentPath: []);
    }

    private void RenderLevel(
        RenderTreeBuilder builder,
        ref int seq,
        IReadOnlyList<BOBChartTreemapNode<TY>> nodes,
        SquarifiedRect rect,
        int depth,
        IReadOnlyList<string> parentPath)
    {
        double[] weights = new double[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            weights[i] = AggregateValue(nodes[i]);
        }

        SquarifiedRect[] layout = Squarified.Layout(weights, rect);
        for (int i = 0; i < nodes.Count; i++)
        {
            SquarifiedRect cell = layout[i];
            if (cell.Width <= 0 || cell.Height <= 0)
            {
                continue;
            }

            BOBChartTreemapNode<TY> node = nodes[i];
            string color = node.Color ?? Palette.ColorAt(depth == 0 ? i : depth);
            string[] path = [.. parentPath, node.Label];
            BOBChartTreemapNode<TY> capturedNode = node;
            string[] capturedPath = path;
            int capturedDepth = depth;

            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "class", "bob-treemap-chart__cell");
            builder.AddAttribute(seq++, "x", Squarified.F(cell.X));
            builder.AddAttribute(seq++, "y", Squarified.F(cell.Y));
            builder.AddAttribute(seq++, "width", Squarified.F(cell.Width));
            builder.AddAttribute(seq++, "height", Squarified.F(cell.Height));
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
            builder.AddContent(seq++, FormatTooltip(node, weights[i]));
            builder.CloseElement();
            builder.CloseElement();

            // Children — recurse into the cell with the padded inner rect so the parent
            // border stays visible between nested levels.
            if (node.Children is { Count: > 0 } children)
            {
                SquarifiedRect inner = Pad(cell, Padding);
                if (inner.Width > 0 && inner.Height > 0)
                {
                    RenderLevel(builder, ref seq, children, inner, depth + 1, path);
                }
            }

            // Label — only draw when the cell is large enough to host readable text.
            if (cell.Width >= MinLabelSize && cell.Height >= MinLabelSize)
            {
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "class", "bob-treemap-chart__label");
                builder.AddAttribute(seq++, "x", Squarified.F(cell.X + (cell.Width / 2)));
                builder.AddAttribute(seq++, "y", Squarified.F(cell.Y + (cell.Height / 2) + 4));
                builder.AddAttribute(seq++, "text-anchor", "middle");
                builder.AddAttribute(seq++, "fill", "white");
                builder.AddAttribute(seq++, "pointer-events", "none");
                builder.AddAttribute(seq++, "font-size", "12");
                builder.AddContent(seq++, node.Label);
                builder.CloseElement();
            }
        }
    }

    private static SquarifiedRect Pad(SquarifiedRect r, double padding) =>
        new(r.X + padding, r.Y + padding, Math.Max(0, r.Width - (2 * padding)), Math.Max(0, r.Height - (2 * padding)));

    private static double AggregateValue(BOBChartTreemapNode<TY> node)
    {
        // Branch nodes derive their magnitude from descendants — sum children first.
        // We can't use `node.Value is not null` to detect "branch with explicit value"
        // because TY is unconstrained, so for value-typed TY (double, decimal, int)
        // an unset Value defaults to 0 — indistinguishable from "explicit 0". Branch
        // nodes that want to override the descendant sum should leave Children null.
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

    private static string FormatTooltip(BOBChartTreemapNode<TY> node, double weight) =>
        string.Format(CultureInfo.InvariantCulture, "{0}: {1:N0}", node.Label, weight);
}
