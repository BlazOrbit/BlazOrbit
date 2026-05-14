namespace BlazOrbit.Charts.Components.Internal;

/// <summary>
/// Discrete categorical scale: each distinct domain value gets an equally
/// sized "band" along the pixel range. Bar charts pick the centre of each
/// band as the bar group's anchor point.
/// </summary>
/// <typeparam name="T">Domain type (typically <see cref="string"/>, but
/// any type with a sensible <see cref="object.Equals(object?)"/> works).</typeparam>
internal sealed class CategoricalScale<T> where T : notnull
{
    private readonly Dictionary<T, int> _index;

    public IReadOnlyList<T> Categories { get; }
    public double RangeMin { get; }
    public double RangeMax { get; }
    public double BandWidth { get; }

    public CategoricalScale(IEnumerable<T> categories, double rangeMin, double rangeMax)
    {
        Categories = categories.Distinct().ToList();
        RangeMin = rangeMin;
        RangeMax = rangeMax;
        BandWidth = Categories.Count == 0
            ? 0
            : (rangeMax - rangeMin) / Categories.Count;

        _index = new Dictionary<T, int>(Categories.Count);
        for (int i = 0; i < Categories.Count; i++)
        {
            _index[Categories[i]] = i;
        }
    }

    /// <summary>Pixel position of the centre of the band for <paramref name="category"/>.</summary>
    public double Center(T category)
    {
        if (!_index.TryGetValue(category, out int idx))
        {
            return RangeMin; // Unknown category: anchor to start.
        }

        return RangeMin + ((idx + 0.5) * BandWidth);
    }
}