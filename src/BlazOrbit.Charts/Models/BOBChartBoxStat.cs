namespace BlazOrbit.Charts.Models;

/// <summary>
/// Pre-computed five-number summary plus optional outliers for one
/// box on a <see cref="Components.BOBBoxplotChart{TX}"/>. Provide either
/// these directly, or use <see cref="BOBChartBoxStat{TX}.FromValues"/>
/// to compute from raw data.
/// </summary>
/// <typeparam name="TX">Categorical X key (e.g. group / segment).</typeparam>
public sealed class BOBChartBoxStat<TX>
    where TX : notnull
{
    /// <summary>Categorical X label.</summary>
    public TX X { get; init; } = default!;

    /// <summary>Whisker minimum (often <c>Q1 − 1.5·IQR</c>).</summary>
    public double Min { get; init; }
    /// <summary>First quartile.</summary>
    public double Q1 { get; init; }
    /// <summary>Median.</summary>
    public double Median { get; init; }
    /// <summary>Third quartile.</summary>
    public double Q3 { get; init; }
    /// <summary>Whisker maximum (often <c>Q3 + 1.5·IQR</c>).</summary>
    public double Max { get; init; }

    /// <summary>Outliers — observations beyond the whiskers, drawn as dots.</summary>
    public IEnumerable<double> Outliers { get; init; } = Array.Empty<double>();

    /// <summary>Optional explicit color for this box.</summary>
    public string? Color { get; init; }

    /// <summary>
    /// Compute a five-number summary from raw values using the inclusive
    /// quantile (linear interpolation) method, with whiskers clamped to
    /// the 1.5·IQR rule and outliers extracted beyond.
    /// </summary>
    public static BOBChartBoxStat<TX> FromValues(TX x, IEnumerable<double> values, string? color = null)
    {
        double[] sorted = values.OrderBy(v => v).ToArray();
        if (sorted.Length == 0)
        {
            return new BOBChartBoxStat<TX> { X = x, Color = color };
        }
        double q1 = Quantile(sorted, 0.25);
        double med = Quantile(sorted, 0.5);
        double q3 = Quantile(sorted, 0.75);
        double iqr = q3 - q1;
        double lo = q1 - 1.5 * iqr;
        double hi = q3 + 1.5 * iqr;
        double whiskMin = sorted.First(v => v >= lo);
        double whiskMax = sorted.Last(v => v <= hi);
        double[] outliers = sorted.Where(v => v < lo || v > hi).ToArray();
        return new BOBChartBoxStat<TX>
        {
            X = x,
            Min = whiskMin,
            Q1 = q1,
            Median = med,
            Q3 = q3,
            Max = whiskMax,
            Outliers = outliers,
            Color = color,
        };
    }

    private static double Quantile(double[] sorted, double q)
    {
        if (sorted.Length == 1) return sorted[0];
        double pos = q * (sorted.Length - 1);
        int lo = (int)Math.Floor(pos);
        int hi = (int)Math.Ceiling(pos);
        if (lo == hi) return sorted[lo];
        double t = pos - lo;
        return sorted[lo] * (1 - t) + sorted[hi] * t;
    }
}
