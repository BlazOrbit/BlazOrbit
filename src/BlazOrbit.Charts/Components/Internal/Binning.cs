namespace BlazOrbit.Charts.Components.Internal;

/// <summary>
/// Histogram binning helpers. Three rules supported:
/// <list type="bullet">
/// <item><description><b>Sturges</b> — <c>k = ⌈log2(n) + 1⌉</c>, classic
///     default. Good for ≤200 normally-distributed values.</description></item>
/// <item><description><b>Scott</b> — bin width <c>= 3.5σ / n^(1/3)</c>;
///     well-behaved for unimodal data.</description></item>
/// <item><description><b>FreedmanDiaconis</b> — bin width <c>= 2·IQR /
///     n^(1/3)</c>; robust against outliers.</description></item>
/// </list>
/// </summary>
internal static class Binning
{
    public enum Rule { Sturges, Scott, FreedmanDiaconis, FixedCount }

    public readonly record struct Bin(double Lower, double Upper, int Count);

    /// <summary>
    /// Build histogram bins for <paramref name="values"/>. Returns an empty
    /// array when the input is empty.
    /// </summary>
    public static Bin[] Compute(IReadOnlyList<double> values, Rule rule, int? fixedCount = null)
    {
        if (values.Count == 0)
        {
            return [];
        }

        double[] sorted = values.OrderBy(v => v).ToArray();
        double min = sorted[0];
        double max = sorted[^1];
        if (Math.Abs(max - min) < double.Epsilon)
        {
            return [new Bin(min - 0.5, max + 0.5, sorted.Length)];
        }

        int k = rule switch
        {
            Rule.FixedCount => Math.Max(1, fixedCount ?? 10),
            Rule.Sturges => Math.Max(1, (int)Math.Ceiling(Math.Log2(sorted.Length) + 1)),
            Rule.Scott => Math.Max(1, (int)Math.Ceiling((max - min) / ScottWidth(sorted))),
            Rule.FreedmanDiaconis => Math.Max(1, (int)Math.Ceiling((max - min) / FdWidth(sorted))),
            _ => 10
        };

        double binWidth = (max - min) / k;
        Bin[] bins = new Bin[k];
        for (int i = 0; i < k; i++)
        {
            double lo = min + (i * binWidth);
            double hi = i == k - 1 ? max : lo + binWidth;
            bins[i] = new Bin(lo, hi, 0);
        }

        foreach (double v in sorted)
        {
            int idx = (int)Math.Floor((v - min) / binWidth);
            if (idx >= k)
            {
                idx = k - 1;
            }

            bins[idx] = bins[idx] with { Count = bins[idx].Count + 1 };
        }

        return bins;
    }

    private static double ScottWidth(double[] sorted)
    {
        double mean = sorted.Average();
        double variance = sorted.Sum(v => (v - mean) * (v - mean)) / sorted.Length;
        double sigma = Math.Sqrt(variance);
        if (sigma <= 0)
        {
            return 1;
        }

        return 3.5 * sigma / Math.Pow(sorted.Length, 1.0 / 3.0);
    }

    private static double FdWidth(double[] sorted)
    {
        double q1 = Quantile(sorted, 0.25);
        double q3 = Quantile(sorted, 0.75);
        double iqr = q3 - q1;
        if (iqr <= 0)
        {
            return 1;
        }

        return 2 * iqr / Math.Pow(sorted.Length, 1.0 / 3.0);
    }

    /// <summary>Inclusive quantile via linear interpolation.</summary>
    public static double Quantile(double[] sorted, double q)
    {
        if (sorted.Length == 0)
        {
            return 0;
        }

        if (sorted.Length == 1)
        {
            return sorted[0];
        }

        double pos = q * (sorted.Length - 1);
        int lo = (int)Math.Floor(pos);
        int hi = (int)Math.Ceiling(pos);
        if (lo == hi)
        {
            return sorted[lo];
        }

        double t = pos - lo;
        return (sorted[lo] * (1 - t)) + (sorted[hi] * t);
    }
}