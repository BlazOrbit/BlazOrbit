namespace BlazOrbit.Charts.Components.Internal;

/// <summary>
/// Linear numeric scale: maps a domain interval [<see cref="DomainMin"/>,
/// <see cref="DomainMax"/>] to a pixel interval [<see cref="RangeMin"/>,
/// <see cref="RangeMax"/>].
/// <para>
/// Constructors compute a "nice" rounded extent (e.g. 0..120 instead of
/// 3.7..117.4) so axis labels read cleanly. Includes 0 in the domain when
/// all values are positive so bars render against a meaningful baseline.
/// </para>
/// </summary>
internal sealed class LinearScale
{
    public double DomainMin { get; }
    public double DomainMax { get; }
    public double RangeMin { get; }
    public double RangeMax { get; }
    public double TickStep { get; }

    /// <summary>
    /// Build a linear scale. <paramref name="rangeMin"/> usually represents
    /// the bottom of the SVG plot area (largest pixel value, since SVG Y
    /// grows downward) and <paramref name="rangeMax"/> the top.
    /// <para>
    /// <paramref name="includeZero"/>: when <c>true</c> the domain is
    /// extended to include <c>0</c> if the data is single-signed. Bar charts
    /// opt in (industry standard: bars are only readable against a zero
    /// baseline). Line / area / temporal axes leave it off so the plot area
    /// focuses on the data extent.
    /// </para>
    /// </summary>
    public LinearScale(IReadOnlyList<double> values, double rangeMin, double rangeMax,
        double? explicitMin = null, double? explicitMax = null,
        bool includeZero = false)
    {
        RangeMin = rangeMin;
        RangeMax = rangeMax;

        double rawMin = values.Count == 0 ? 0 : values.Min();
        double rawMax = values.Count == 0 ? 1 : values.Max();

        if (includeZero)
        {
            if (rawMin > 0)
            {
                rawMin = 0;
            }

            if (rawMax < 0)
            {
                rawMax = 0;
            }
        }

        // Degenerate (all-equal) data: pad ±1 so the scale isn't undefined.
        if (Math.Abs(rawMax - rawMin) < double.Epsilon)
        {
            rawMin -= 1;
            rawMax += 1;
        }

        double step = NiceTickStep(rawMin, rawMax, 5);
        DomainMin = explicitMin ?? Math.Floor(rawMin / step) * step;
        DomainMax = explicitMax ?? Math.Ceiling(rawMax / step) * step;
        TickStep = step;
    }

    /// <summary>Project a domain value onto the pixel range.</summary>
    public double Project(double domainValue)
    {
        double t = (domainValue - DomainMin) / (DomainMax - DomainMin);
        return RangeMin + (t * (RangeMax - RangeMin));
    }

    /// <summary>
    /// Inverse of <see cref="Project"/>: recover the domain value that maps
    /// to a given pixel position on the range. Used by zoom / brush
    /// interactions to translate cursor coordinates back into data space.
    /// </summary>
    public double Invert(double rangeValue)
    {
        double t = (rangeValue - RangeMin) / (RangeMax - RangeMin);
        return DomainMin + (t * (DomainMax - DomainMin));
    }

    /// <summary>
    /// Enumerate the tick values that fall within the domain, stepping by
    /// <see cref="TickStep"/>. Includes both endpoints.
    /// </summary>
    public IEnumerable<double> Ticks()
    {
        // Use a small epsilon to absorb floating-point drift on the upper bound.
        double eps = TickStep * 1e-9;
        for (double t = DomainMin; t <= DomainMax + eps; t += TickStep)
        {
            yield return t;
        }
    }

    /// <summary>
    /// Suggested decimal-place count for tick labels — derived from the
    /// magnitude of <see cref="TickStep"/>. Ensures zoomed-in axes (where
    /// step might be 0.1, 0.05, 0.01…) show enough precision while
    /// fully-extended axes (step = 1, 5, 10…) stay free of trailing zeros.
    /// </summary>
    public int SuggestedDecimals
    {
        get
        {
            if (TickStep >= 1)
            {
                return 0;
            }

            // -log10(0.1) = 1, -log10(0.01) = 2, -log10(0.05) ≈ 1.3 → 2.
            return Math.Min(10, Math.Max(0, (int)Math.Ceiling(-Math.Log10(TickStep))));
        }
    }

    /// <summary>
    /// .NET format string suitable for axis tick labels at the current
    /// <see cref="TickStep"/>. Returns <c>"0"</c> for integer steps and
    /// <c>"0.0…"</c> for fractional ones. Callers can substitute their
    /// own format via the chart's axis options when this default doesn't
    /// suit (currency, percent, scientific).
    /// </summary>
    public string SuggestedFormat() =>
        SuggestedDecimals == 0 ? "0" : "0." + new string('#', SuggestedDecimals);

    /// <summary>
    /// Compute a "nice" tick step (powers of 10 multiplied by 1, 2 or 5) so
    /// labels land on round numbers. Targets ~5 ticks across the domain.
    /// </summary>
    private static double NiceTickStep(double min, double max, int targetTicks)
    {
        double range = max - min;
        if (range <= 0)
        {
            return 1;
        }

        double rough = range / Math.Max(1, targetTicks);
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rough)));
        double residual = rough / magnitude;
        double nice = residual switch
        {
            < 1.5 => 1,
            < 3.5 => 2,
            < 7.5 => 5,
            _ => 10
        };
        return nice * magnitude;
    }
}