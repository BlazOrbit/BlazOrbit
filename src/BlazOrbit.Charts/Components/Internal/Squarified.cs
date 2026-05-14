using System.Globalization;

namespace BlazOrbit.Charts.Components.Internal;

/// <summary>
/// Squarified treemap layout (Bruls / Huijing / van Wijk, "Squarified Treemaps", 2000).
/// Subdivides a rectangle into proportionally-sized child rectangles while minimising
/// aspect ratio (rects stay as square as possible — easier to compare visually than
/// long thin strips of the naive slice-and-dice approach).
/// </summary>
internal static class Squarified
{
    /// <summary>
    /// Tile <paramref name="weights"/> into rectangles that fill <paramref name="rect"/>.
    /// Each weight maps to one output rect in the same order; the total area of the
    /// returned rects equals <c>rect.Width * rect.Height</c>. Empty / negative weights
    /// are dropped; the corresponding output slot receives an empty rect (zero-area)
    /// so the caller's index correspondence with the input list is preserved.
    /// </summary>
    /// <param name="weights">Relative magnitudes, one per child node.</param>
    /// <param name="rect">Parent rectangle to subdivide.</param>
    /// <returns>Layout per input weight, in the same order as <paramref name="weights"/>.</returns>
    public static SquarifiedRect[] Layout(IReadOnlyList<double> weights, SquarifiedRect rect)
    {
        SquarifiedRect[] output = new SquarifiedRect[weights.Count];
        if (weights.Count == 0 || rect.Width <= 0 || rect.Height <= 0)
        {
            return output;
        }

        // Build a list of (index, weight) pairs sorted by descending weight — the
        // squarified algorithm processes children largest-first so the early rows fill
        // the dominant area + the later rows handle the residue.
        List<(int Index, double Weight)> sorted = [];
        double total = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            double w = weights[i];
            if (w <= 0 || double.IsNaN(w) || double.IsInfinity(w))
            {
                continue;
            }

            sorted.Add((i, w));
            total += w;
        }

        if (total <= 0)
        {
            return output;
        }

        sorted.Sort(static (a, b) => b.Weight.CompareTo(a.Weight));

        // Scale weights to area units inside the rect so we can compare aspect ratios
        // directly to rect side lengths during the row build-up.
        double area = rect.Width * rect.Height;
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i] = (sorted[i].Index, sorted[i].Weight / total * area);
        }

        Squarify(sorted, [], rect, output);
        return output;
    }

    private static void Squarify(
        List<(int Index, double Weight)> remaining,
        List<(int Index, double Weight)> row,
        SquarifiedRect rect,
        SquarifiedRect[] output)
    {
        while (remaining.Count > 0)
        {
            double w = ShortSide(rect);
            if (w <= 0)
            {
                // Out of room — assign zero-area rects to the rest so the indices stay
                // aligned. Real treemap engines also bail here; visually empty cells.
                foreach ((int Index, double Weight) entry in remaining)
                {
                    output[entry.Index] = new SquarifiedRect(rect.X, rect.Y, 0, 0);
                }

                row.Clear();
                remaining.Clear();
                return;
            }

            (int Index, double Weight) head = remaining[0];

            // Worst aspect ratio of the row "as-is" vs. with head appended. If adding
            // head improves (or matches) the worst ratio, keep filling the row; else
            // commit the row and start a fresh one with head as the first entry.
            double currentWorst = row.Count == 0 ? double.PositiveInfinity : Worst(row, w);
            row.Add(head);
            double withHeadWorst = Worst(row, w);

            if (row.Count > 1 && withHeadWorst > currentWorst)
            {
                // Backtrack — head made the row worse. Commit the prior row, recurse
                // on the residue rect, and let the outer loop retry head against the
                // residue.
                row.RemoveAt(row.Count - 1);
                rect = CommitRow(row, rect, output);
                row.Clear();
            }
            else
            {
                // Keep head in the row; remove from remaining and continue filling.
                remaining.RemoveAt(0);
            }
        }

        if (row.Count > 0)
        {
            CommitRow(row, rect, output);
        }
    }

    private static SquarifiedRect CommitRow(
        List<(int Index, double Weight)> row,
        SquarifiedRect rect,
        SquarifiedRect[] output)
    {
        double rowSum = 0;
        foreach ((int _, double w) in row)
        {
            rowSum += w;
        }

        double shortSide = ShortSide(rect);
        double longSide = LongSide(rect);
        double rowThickness = shortSide <= 0 ? 0 : rowSum / shortSide;

        // The row is placed along the shortest edge — that's the whole point of
        // squarified: row width = shortSide of the parent rect, row height = rowSum /
        // shortSide. After commit the parent rect shrinks by rowThickness on the long
        // side so the residue covers the remaining children.
        if (rect.Width <= rect.Height)
        {
            double x = rect.X;
            double y = rect.Y;
            foreach ((int Index, double Weight) entry in row)
            {
                double cellWidth = rect.Width <= 0 ? 0 : entry.Weight / rowThickness;
                output[entry.Index] = new SquarifiedRect(x, y, cellWidth, rowThickness);
                x += cellWidth;
            }

            return new SquarifiedRect(rect.X, rect.Y + rowThickness, rect.Width, rect.Height - rowThickness);
        }
        else
        {
            double x = rect.X;
            double y = rect.Y;
            foreach ((int Index, double Weight) entry in row)
            {
                double cellHeight = rect.Height <= 0 ? 0 : entry.Weight / rowThickness;
                output[entry.Index] = new SquarifiedRect(x, y, rowThickness, cellHeight);
                y += cellHeight;
            }

            return new SquarifiedRect(rect.X + rowThickness, rect.Y, rect.Width - rowThickness, rect.Height);
        }
    }

    // Worst aspect ratio across the row at given short-side length. The classic
    // squarified ratio: max(max(width/height, height/width)) — the closer to 1, the
    // more square-like the row.
    private static double Worst(List<(int Index, double Weight)> row, double shortSide)
    {
        double rowSum = 0;
        double rowMax = double.MinValue;
        double rowMin = double.MaxValue;
        foreach ((int _, double w) in row)
        {
            rowSum += w;
            if (w > rowMax) { rowMax = w; }
            if (w < rowMin) { rowMin = w; }
        }

        if (rowSum <= 0 || shortSide <= 0)
        {
            return double.PositiveInfinity;
        }

        double s2 = shortSide * shortSide;
        double sum2 = rowSum * rowSum;
        return Math.Max(s2 * rowMax / sum2, sum2 / (s2 * rowMin));
    }

    private static double ShortSide(SquarifiedRect r) => Math.Min(r.Width, r.Height);
    private static double LongSide(SquarifiedRect r) => Math.Max(r.Width, r.Height);

    /// <summary>Formats a coordinate to two decimals in the invariant culture.</summary>
    public static string F(double value) => value.ToString("F2", CultureInfo.InvariantCulture);
}

/// <summary>Rectangle in SVG coordinate space.</summary>
/// <param name="X">Left edge.</param>
/// <param name="Y">Top edge.</param>
/// <param name="Width">Horizontal extent.</param>
/// <param name="Height">Vertical extent.</param>
internal readonly record struct SquarifiedRect(double X, double Y, double Width, double Height);
