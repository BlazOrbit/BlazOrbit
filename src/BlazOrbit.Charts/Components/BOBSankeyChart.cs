using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Sankey flow diagram — visualises mass / energy / monetary movement between named
/// nodes as variable-thickness ribbons. The chart auto-derives node columns from the
/// edge graph (topological rank by reachability from sources) so consumers only
/// describe the links; positions are computed.
/// </summary>
/// <typeparam name="TY">Numeric type of the link values.</typeparam>
public sealed class BOBSankeyChart<TY> : BOBChartBase<object, TY>
{
    /// <summary>Flow edges between named source / target nodes.</summary>
    [Parameter]
    public IEnumerable<BOBChartSankeyLink<TY>>? Links { get; set; }

    /// <summary>Pixel width of each node's column rectangle. Default 16.</summary>
    [Parameter]
    public double NodeWidth { get; set; } = 16;

    /// <summary>Vertical gap (pixels) between nodes in the same column. Default 12.</summary>
    [Parameter]
    public double NodePadding { get; set; } = 12;

    /// <summary>Pixel padding around the chart inside the SVG viewport. Default 16.</summary>
    [Parameter]
    public double Padding { get; set; } = 16;

    /// <summary>
    /// Fired when the user clicks a link ribbon. Carries the link record so the
    /// consumer can drill into the underlying dataset.
    /// </summary>
    [Parameter]
    public EventCallback<BOBChartSankeyLink<TY>> OnLinkClick { get; set; }

    /// <summary>
    /// Fired when the user clicks a node rectangle. Carries the node label so the
    /// consumer can pivot the view (filter, drill, etc.).
    /// </summary>
    [Parameter]
    public EventCallback<string> OnNodeClick { get; set; }

    /// <summary>
    /// When <c>true</c> (default), nodes with no outgoing edges (sinks) are
    /// forced to the rightmost column so terminal nodes align. Set to
    /// <c>false</c> for funnel-like flows where drops should appear adjacent
    /// to the step where they occur.
    /// </summary>
    [Parameter]
    public bool AlignSinksRight { get; set; } = true;

    /// <summary>Opacity of link ribbons, 0–1. Default 0.5.</summary>
    [Parameter]
    public double LinkOpacity { get; set; } = 0.5;

    /// <summary>Font size of node labels in pixels. Default 12.</summary>
    [Parameter]
    public double LabelFontSize { get; set; } = 12;

    /// <summary>
    /// Optional per-node colour overrides. Key = node label, value = any valid
    /// CSS colour. When a node is present in this dictionary its rectangle uses
    /// the supplied colour instead of the column palette.
    /// </summary>
    [Parameter]
    public System.Collections.Generic.IReadOnlyDictionary<string, string>? NodeColors { get; set; }

    /// <inheritdoc />
    protected override string BuildAriaLabel()
    {
        int count = Links?.Count() ?? 0;
        return count == 0
            ? "Sankey diagram with no flows"
            : $"Sankey diagram with {count} flows";
    }

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Links is null)
        {
            return;
        }

        BOBChartSankeyLink<TY>[] links = Links
            .Where(l => !string.IsNullOrEmpty(l.Source) && !string.IsNullOrEmpty(l.Target))
            .Where(l => Math.Max(0, Numeric.ToDouble(l.Value)) > 0)
            .ToArray();
        if (links.Length == 0)
        {
            return;
        }

        // Build node graph: each unique label gets a node; columns derived by topological
        // depth (longest path from any source).
        Dictionary<string, SankeyNode> nodes = new(StringComparer.Ordinal);
        foreach (BOBChartSankeyLink<TY> l in links)
        {
            if (!nodes.ContainsKey(l.Source))
            {
                nodes[l.Source] = new SankeyNode(l.Source);
            }

            if (!nodes.ContainsKey(l.Target))
            {
                nodes[l.Target] = new SankeyNode(l.Target);
            }

            double v = Math.Max(0, Numeric.ToDouble(l.Value));
            nodes[l.Source].Out += v;
            nodes[l.Target].In += v;
        }

        AssignColumns(nodes, links, AlignSinksRight);

        double svgW = EffectiveWidth;
        double svgH = EffectiveHeight;
        double plotLeft = Padding;
        double plotTop = Padding;
        double plotRight = svgW - Padding;
        double plotBottom = svgH - Padding;
        double plotWidth = plotRight - plotLeft;
        double plotHeight = plotBottom - plotTop;

        int maxColumn = nodes.Values.Max(n => n.Column);
        if (maxColumn <= 0 || plotWidth <= 0 || plotHeight <= 0)
        {
            return;
        }

        // Column X positions evenly spread; node rectangles sit on column centres.
        double columnGap = (plotWidth - NodeWidth) / maxColumn;
        Dictionary<int, List<SankeyNode>> byColumn = new();
        foreach (SankeyNode n in nodes.Values)
        {
            if (!byColumn.TryGetValue(n.Column, out List<SankeyNode>? list))
            {
                list = [];
                byColumn[n.Column] = list;
            }

            list.Add(n);
        }

        // Each column's nodes share the plot height — pixel scale derived from the
        // largest column's total magnitude so ribbons stay proportional across columns.
        double maxColumnFlow = byColumn.Values
            .Select(col => col.Sum(n => Math.Max(n.In, n.Out)))
            .DefaultIfEmpty(0)
            .Max();
        if (maxColumnFlow <= 0)
        {
            return;
        }

        double scale = (plotHeight - (NodePadding * (byColumn.Values.Max(c => c.Count) - 1))) / maxColumnFlow;
        scale = Math.Max(0.1, scale);

        // Lay out each column top-to-bottom. Intermediate nodes (nodes that have
        // outgoing edges) are placed above sinks so the happy path reads top-to-bottom
        // and drops fall to the lower part of the column.
        HashSet<string> hasOutgoing = links.Select(l => l.Source).ToHashSet(StringComparer.Ordinal);
        foreach ((int col, List<SankeyNode> list) in byColumn)
        {
            double x = plotLeft + (col * columnGap);
            double y = plotTop;
            foreach (SankeyNode n in list
                .OrderByDescending(n => hasOutgoing.Contains(n.Label) ? 1 : 0)
                .ThenByDescending(n => Math.Max(n.In, n.Out)))
            {
                n.X = x;
                n.Y = y;
                n.Height = Math.Max(2, Math.Max(n.In, n.Out) * scale);
                y += n.Height + NodePadding;
            }
        }

        // Track per-node band cursors so multiple ribbons stack on top of each other
        // instead of overlapping.
        Dictionary<string, double> outCursor = new(StringComparer.Ordinal);
        Dictionary<string, double> inCursor = new(StringComparer.Ordinal);

        int seq = 100;

        // Render links first so node rectangles stack above their ribbons.
        foreach (BOBChartSankeyLink<TY> link in links)
        {
            SankeyNode src = nodes[link.Source];
            SankeyNode dst = nodes[link.Target];
            double v = Math.Max(0, Numeric.ToDouble(link.Value));
            double thickness = v * scale;

            double srcOffset = outCursor.TryGetValue(src.Label, out double so) ? so : 0;
            double dstOffset = inCursor.TryGetValue(dst.Label, out double di) ? di : 0;
            outCursor[src.Label] = srcOffset + thickness;
            inCursor[dst.Label] = dstOffset + thickness;

            double y0 = src.Y + srcOffset + (thickness / 2);
            double y1 = dst.Y + dstOffset + (thickness / 2);
            double x0 = src.X + NodeWidth;
            double x1 = dst.X;
            double midX = (x0 + x1) / 2;

            // Deterministic hash so snapshot tests stay stable across runs/processes —
            // string.GetHashCode is randomised per-process since .NET Core. We fold the
            // label bytes through a simple FNV-1a step which is plenty for an 8-slot
            // palette index.
            int labelHash = 0;
            foreach (char ch in src.Label)
            {
                labelHash = (labelHash * 31) + ch;
            }
            string color = link.Color ?? Palette.ColorAt(labelHash & 7);
            BOBChartSankeyLink<TY> capturedLink = link;

            // Cubic Bezier with horizontal control points centred between columns — the
            // classic Sankey "smooth ribbon" path.
            string d = string.Format(CultureInfo.InvariantCulture,
                "M {0:F2} {1:F2} C {2:F2} {1:F2} {2:F2} {3:F2} {4:F2} {3:F2}",
                x0, y0, midX, y1, x1);

            builder.OpenElement(seq++, "path");
            builder.AddAttribute(seq++, "class", "bob-sankey-chart__link");
            builder.AddAttribute(seq++, "d", d);
            builder.AddAttribute(seq++, "fill", "none");
            builder.AddAttribute(seq++, "stroke", color);
            builder.AddAttribute(seq++, "stroke-width", Squarified.F(thickness));
            builder.AddAttribute(seq++, "stroke-opacity",
                LinkOpacity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (OnLinkClick.HasDelegate)
            {
                builder.AddAttribute(seq++, "onclick",
                    EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                        this, _ => OnLinkClick.InvokeAsync(capturedLink)));
                builder.AddAttribute(seq++, "style", "cursor: pointer;");
            }

            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++, string.Format(CultureInfo.InvariantCulture, "{0} → {1}: {2:N0}", link.Source, link.Target, v));
            builder.CloseElement();
            builder.CloseElement();
        }

        // Render node rectangles + labels.
        foreach (SankeyNode n in nodes.Values)
        {
            string color = NodeColors?.TryGetValue(n.Label, out string? overrideColor) == true && overrideColor is not null
                ? overrideColor
                : Palette.ColorAt(n.Column);
            string capturedLabel = n.Label;

            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "class", "bob-sankey-chart__node");
            builder.AddAttribute(seq++, "x", Squarified.F(n.X));
            builder.AddAttribute(seq++, "y", Squarified.F(n.Y));
            builder.AddAttribute(seq++, "width", Squarified.F(NodeWidth));
            builder.AddAttribute(seq++, "height", Squarified.F(n.Height));
            builder.AddAttribute(seq++, "fill", color);
            if (OnNodeClick.HasDelegate)
            {
                builder.AddAttribute(seq++, "onclick",
                    EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                        this, _ => OnNodeClick.InvokeAsync(capturedLabel)));
                builder.AddAttribute(seq++, "style", "cursor: pointer;");
            }

            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++, string.Format(CultureInfo.InvariantCulture, "{0}: {1:N0}", n.Label, Math.Max(n.In, n.Out)));
            builder.CloseElement();
            builder.CloseElement();

            // Label sits to the right for left-half nodes and to the left for right-half
            // ones so it never overlaps the node rect.
            bool labelOnRight = n.Column < maxColumn;
            double labelX = labelOnRight ? n.X + NodeWidth + 4 : n.X - 4;
            string anchor = labelOnRight ? "start" : "end";
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "bob-sankey-chart__label");
            builder.AddAttribute(seq++, "x", Squarified.F(labelX));
            builder.AddAttribute(seq++, "y", Squarified.F(n.Y + (n.Height / 2) + 4));
            builder.AddAttribute(seq++, "text-anchor", anchor);
            builder.AddAttribute(seq++, "fill", "var(--palette-surface-contrast, currentColor)");
            builder.AddAttribute(seq++, "pointer-events", "none");
            builder.AddAttribute(seq++, "font-size",
                LabelFontSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
            builder.AddContent(seq++, n.Label);
            builder.CloseElement();
        }
    }

    // Topological column assignment: each node's column = 1 + max(source column).
    private static void AssignColumns(Dictionary<string, SankeyNode> nodes, IReadOnlyList<BOBChartSankeyLink<TY>> links, bool alignSinksRight)
    {
        // BFS over edges; cap iterations at links.Count * nodes.Count to bound runtime on
        // pathological inputs (cycles, etc.).
        int safetyCap = Math.Max(1, links.Count * nodes.Count);
        bool changed = true;
        int iterations = 0;
        while (changed && iterations++ < safetyCap)
        {
            changed = false;
            foreach (BOBChartSankeyLink<TY> link in links)
            {
                SankeyNode src = nodes[link.Source];
                SankeyNode dst = nodes[link.Target];
                int wanted = src.Column + 1;
                if (dst.Column < wanted)
                {
                    dst.Column = wanted;
                    changed = true;
                }
            }
        }

        // When enabled, promote sink nodes (no outgoing edges) to the rightmost column
        // so terminal boxes align cleanly — common Sankey convention for energy-flow
        // diagrams. Disabled for funnel-like flows where drops should stay near the
        // step that produced them.
        if (alignSinksRight)
        {
            int maxColumn = nodes.Values.DefaultIfEmpty(new SankeyNode("")).Max(n => n.Column);
            HashSet<string> hasOutgoing = links.Select(l => l.Source).ToHashSet(StringComparer.Ordinal);
            foreach (SankeyNode n in nodes.Values)
            {
                if (!hasOutgoing.Contains(n.Label) && n.Column < maxColumn)
                {
                    n.Column = maxColumn;
                }
            }
        }
    }

    private sealed class SankeyNode(string label)
    {
        public string Label { get; } = label;
        public int Column { get; set; }
        public double In { get; set; }
        public double Out { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Height { get; set; }
    }
}
