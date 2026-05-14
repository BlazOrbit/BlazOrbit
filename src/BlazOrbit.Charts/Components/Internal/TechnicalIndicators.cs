namespace BlazOrbit.Charts.Components.Internal;

/// <summary>
/// Closed-form computations for the technical-analysis indicators exposed by
/// <see cref="BlazOrbit.Charts.Components.BOBStockChart{TX}"/>. Each routine returns a
/// list parallel to the input close series; values that fall before the indicator's
/// warm-up window are <see langword="null"/> so the renderer can leave the corresponding
/// X position blank.
/// </summary>
internal static class TechnicalIndicators
{
    /// <summary>
    /// Relative Strength Index over <paramref name="period"/> closes (default 14).
    /// Formula: <c>100 - 100 / (1 + RS)</c> where <c>RS = avgGain / avgLoss</c> using
    /// Wilder's smoothed averaging. Returns <see langword="null"/> for indices &lt;
    /// <paramref name="period"/>.
    /// </summary>
    public static double?[] Rsi(IReadOnlyList<double> closes, int period = 14)
    {
        double?[] output = new double?[closes.Count];
        if (closes.Count <= period)
        {
            return output;
        }

        double gain = 0;
        double loss = 0;
        for (int i = 1; i <= period; i++)
        {
            double delta = closes[i] - closes[i - 1];
            if (delta >= 0) { gain += delta; } else { loss -= delta; }
        }

        gain /= period;
        loss /= period;
        output[period] = ComputeRsi(gain, loss);

        for (int i = period + 1; i < closes.Count; i++)
        {
            double delta = closes[i] - closes[i - 1];
            double g = delta >= 0 ? delta : 0;
            double l = delta < 0 ? -delta : 0;
            gain = ((gain * (period - 1)) + g) / period;
            loss = ((loss * (period - 1)) + l) / period;
            output[i] = ComputeRsi(gain, loss);
        }

        return output;
    }

    /// <summary>
    /// MACD line + signal line + histogram. MACD = EMA(fast) − EMA(slow); signal =
    /// EMA(macd, signalPeriod); histogram = macd − signal. Default 12/26/9.
    /// </summary>
    public static (double?[] Macd, double?[] Signal, double?[] Histogram) Macd(
        IReadOnlyList<double> closes, int fast = 12, int slow = 26, int signalPeriod = 9)
    {
        double?[] emaFast = Ema(closes, fast);
        double?[] emaSlow = Ema(closes, slow);
        double?[] macd = new double?[closes.Count];
        for (int i = 0; i < closes.Count; i++)
        {
            if (emaFast[i] is double f && emaSlow[i] is double s)
            {
                macd[i] = f - s;
            }
        }

        double?[] signal = EmaOfNullable(macd, signalPeriod);
        double?[] histogram = new double?[closes.Count];
        for (int i = 0; i < closes.Count; i++)
        {
            if (macd[i] is double m && signal[i] is double sg)
            {
                histogram[i] = m - sg;
            }
        }

        return (macd, signal, histogram);
    }

    /// <summary>
    /// Bollinger Bands: middle = SMA(period), upper = middle + stdMultiplier × σ,
    /// lower = middle − stdMultiplier × σ over the same window. Defaults: period 20,
    /// stdMultiplier 2.
    /// </summary>
    public static (double?[] Middle, double?[] Upper, double?[] Lower) BollingerBands(
        IReadOnlyList<double> closes, int period = 20, double stdMultiplier = 2)
    {
        double?[] middle = Sma(closes, period);
        double?[] upper = new double?[closes.Count];
        double?[] lower = new double?[closes.Count];

        for (int i = 0; i < closes.Count; i++)
        {
            if (middle[i] is not double mid)
            {
                continue;
            }

            double sumSq = 0;
            for (int j = i - period + 1; j <= i; j++)
            {
                double diff = closes[j] - mid;
                sumSq += diff * diff;
            }

            double sigma = Math.Sqrt(sumSq / period);
            upper[i] = mid + (stdMultiplier * sigma);
            lower[i] = mid - (stdMultiplier * sigma);
        }

        return (middle, upper, lower);
    }

    /// <summary>Simple Moving Average over <paramref name="period"/>.</summary>
    public static double?[] Sma(IReadOnlyList<double> closes, int period)
    {
        double?[] output = new double?[closes.Count];
        if (closes.Count < period)
        {
            return output;
        }

        double window = 0;
        for (int i = 0; i < period; i++)
        {
            window += closes[i];
        }

        output[period - 1] = window / period;
        for (int i = period; i < closes.Count; i++)
        {
            window += closes[i] - closes[i - period];
            output[i] = window / period;
        }

        return output;
    }

    /// <summary>Exponential Moving Average over <paramref name="period"/>.</summary>
    public static double?[] Ema(IReadOnlyList<double> closes, int period)
    {
        double?[] output = new double?[closes.Count];
        if (closes.Count < period)
        {
            return output;
        }

        double k = 2.0 / (period + 1);

        // Seed with SMA of first `period` closes (standard EMA initialisation).
        double seed = 0;
        for (int i = 0; i < period; i++)
        {
            seed += closes[i];
        }

        double ema = seed / period;
        output[period - 1] = ema;
        for (int i = period; i < closes.Count; i++)
        {
            ema = (closes[i] * k) + (ema * (1 - k));
            output[i] = ema;
        }

        return output;
    }

    private static double?[] EmaOfNullable(double?[] series, int period)
    {
        double?[] output = new double?[series.Length];
        double k = 2.0 / (period + 1);
        double? ema = null;
        int count = 0;
        double seedSum = 0;

        for (int i = 0; i < series.Length; i++)
        {
            if (series[i] is not double v)
            {
                continue;
            }

            if (ema is null)
            {
                seedSum += v;
                count++;
                if (count == period)
                {
                    ema = seedSum / period;
                    output[i] = ema;
                }
            }
            else
            {
                ema = (v * k) + (ema.Value * (1 - k));
                output[i] = ema;
            }
        }

        return output;
    }

    private static double ComputeRsi(double avgGain, double avgLoss)
    {
        if (avgLoss <= 0)
        {
            return avgGain > 0 ? 100 : 50;
        }

        double rs = avgGain / avgLoss;
        return 100 - (100 / (1 + rs));
    }
}
