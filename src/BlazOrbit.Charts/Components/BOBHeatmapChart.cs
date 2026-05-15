using BlazOrbit.Charts.Components.Internal;
using BlazOrbit.Charts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace BlazOrbit.Charts.Components;

/// <summary>
/// Heatmap - 2D matrix where each cell's intensity maps to a color in a
/// sequential ramp between <see cref="LowColor"/> and <see cref="HighColor"/>.
/// Useful for correlation matrices, calendar heatmaps (GitHub-style),
/// hourly traffic, retention cohorts.
/// </summary>
/// <typeparam name="TX">Column key (e.g. day, hour, bucket).</typeparam>
/// <typeparam name="TY">Row key (e.g. day-of-week, segment).</typeparam>
public sealed class BOBHeatmapChart<TX, TY> : BOBChartBase<TX, TY>
    where TX : notnull
    where TY : notnull
{
    /// <summary>Cells; each cell defines its column, row and intensity value.</summary>
    [Parameter]
    public IEnumerable<BOBChartHeatmapCell<TX, TY>>? Cells { get; set; }

    /// <summary>Optional explicit column order; otherwise auto-derived from cells.</summary>
    [Parameter]
    public IEnumerable<TX>? Columns { get; set; }

    /// <summary>Optional explicit row order; otherwise auto-derived from cells.</summary>
    [Parameter]
    public IEnumerable<TY>? Rows { get; set; }

    /// <summary>Lower-bound color of the ramp (mapped to min cell value). Defaults to a
    /// theme-aware variable so the ramp shifts with the active palette; pass any valid
    /// CSS color (literal or <c>var(--palette-…)</c>) to override.</summary>
    [Parameter]
    public string LowColor { get; set; } = "var(--palette-primary-50, #e0f2fe)";

    /// <summary>Upper-bound color of the ramp (mapped to max cell value). Same theming
    /// conventions as <see cref="LowColor"/>.</summary>
    [Parameter]
    public string HighColor { get; set; } = "var(--palette-primary, #1e40af)";

    /// <summary>Pixel gap between adjacent cells. Default 1.</summary>
    [Parameter]
    public double CellGap { get; set; } = 1;

    /// <summary>When <c>true</c>, the cell value is rendered as text inside the cell.</summary>
    [Parameter]
    public bool ShowValues { get; set; }

    /// <summary>Format for cell-value text (when <see cref="ShowValues"/> is on).</summary>
    [Parameter]
    public string ValueFormat { get; set; } = "0.#";

    /// <inheritdoc />
    protected override string BuildAriaLabel() => $"Heatmap with {Cells?.Count() ?? 0} cells";

    /// <inheritdoc />
    protected override void RenderSvg(RenderTreeBuilder builder)
    {
        if (Cells is null)
        {
            return;
        }

        BOBChartHeatmapCell<TX, TY>[] cells = Cells.ToArray();
        if (cells.Length == 0)
        {
            return;
        }

        TX[] cols = (Columns ?? cells.Select(c => c.X).Distinct()).ToArray();
        TY[] rows = (Rows ?? cells.Select(c => c.Y).Distinct()).ToArray();
        if (cols.Length == 0 || rows.Length == 0)
        {
            return;
        }

        ChartLayout layout = ChartLayout.Default(EffectiveWidth, EffectiveHeight);
        double colW = layout.PlotWidth / cols.Length;
        double rowH = layout.PlotHeight / rows.Length;

        double minV = cells.Min(c => c.Value);
        double maxV = cells.Max(c => c.Value);
        if (Math.Abs(maxV - minV) < double.Epsilon)
        {
            maxV = minV + 1;
        }

        Dictionary<TX, int> colIdx = cols.Select((x, i) => (x, i)).ToDictionary(t => t.x, t => t.i);
        Dictionary<TY, int> rowIdx = rows.Select((y, i) => (y, i)).ToDictionary(t => t.y, t => t.i);

        (int lr, int lg, int lb) = ParseHexColor(LowColor);
        (int hr, int hg, int hb) = ParseHexColor(HighColor);

        int seq = 100;
        // Cells.
        foreach (BOBChartHeatmapCell<TX, TY> cell in cells)
        {
            if (!colIdx.TryGetValue(cell.X, out int c) || !rowIdx.TryGetValue(cell.Y, out int r))
            {
                continue;
            }

            double t = (cell.Value - minV) / (maxV - minV);
            int rr = (int)(lr + ((hr - lr) * t));
            int gg = (int)(lg + ((hg - lg) * t));
            int bb = (int)(lb + ((hb - lb) * t));
            string fill = $"#{rr:X2}{gg:X2}{bb:X2}";

            double x = layout.PlotLeft + (c * colW);
            double y = layout.PlotTop + (r * rowH);

            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "class", "bob-heatmap-chart__cell");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(x + (CellGap / 2)));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(y + (CellGap / 2)));
            builder.AddAttribute(seq++, "width", ChartLayout.ToInvariant(Math.Max(0, colW - CellGap)));
            builder.AddAttribute(seq++, "height", ChartLayout.ToInvariant(Math.Max(0, rowH - CellGap)));
            builder.AddAttribute(seq++, "fill", fill);
            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++, $"{cell.X} × {cell.Y}: {cell.Value}");
            builder.CloseElement();
            builder.CloseElement();

            if (ShowValues)
            {
                builder.OpenElement(seq++, "text");
                builder.AddAttribute(seq++, "class", "bob-heatmap-chart__value");
                builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(x + (colW / 2)));
                builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(y + (rowH / 2) + 4));
                builder.AddAttribute(seq++, "text-anchor", "middle");
                builder.AddAttribute(seq++, "fill", t > 0.55 ? "white" : "var(--palette-surface-contrast, #1f2937)");
                builder.AddContent(seq++, cell.Value.ToString(ValueFormat, CultureInfo.InvariantCulture));
                builder.CloseElement();
            }
        }

        // Column labels.
        for (int i = 0; i < cols.Length; i++)
        {
            double x = layout.PlotLeft + (i * colW) + (colW / 2);
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "bob-heatmap-chart__col-label");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(x));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(layout.PlotBottom + 18));
            builder.AddAttribute(seq++, "text-anchor", "middle");
            builder.AddContent(seq++, cols[i]?.ToString() ?? string.Empty);
            builder.CloseElement();
        }

        // Row labels.
        for (int i = 0; i < rows.Length; i++)
        {
            double y = layout.PlotTop + (i * rowH) + (rowH / 2) + 4;
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "bob-heatmap-chart__row-label");
            builder.AddAttribute(seq++, "x", ChartLayout.ToInvariant(layout.PlotLeft - 8));
            builder.AddAttribute(seq++, "y", ChartLayout.ToInvariant(y));
            builder.AddAttribute(seq++, "text-anchor", "end");
            builder.AddContent(seq++, rows[i]?.ToString() ?? string.Empty);
            builder.CloseElement();
        }
    }

    /// <summary>Parses <c>#rrggbb</c> hex colors. CSS variables fall back to a neutral mid-grey.</summary>
    private static (int r, int g, int b) ParseHexColor(string color)
    {
        if (color.StartsWith("#") && color.Length == 7)
        {
            return (
                Convert.ToInt32(color.Substring(1, 2), 16),
                Convert.ToInt32(color.Substring(3, 2), 16),
                Convert.ToInt32(color.Substring(5, 2), 16));
        }

        // CSS variable - runtime evaluation isn't available server-side;
        // pick a neutral pivot so the gradient still renders.
        return (200, 200, 200);
    }
}