using System.Numerics;

namespace BlazOrbit.Components;

/// <summary>
/// Immutable inclusive numeric range used by range-style components.
/// Wraps a <typeparamref name="T"/> minimum and maximum and provides validation,
/// containment, and clamping helpers backed by generic math.
/// </summary>
/// <typeparam name="T">Numeric value type that participates in <see cref="INumber{TSelf}"/>.</typeparam>
public readonly record struct BOBNumericRange<T>(T Min, T Max) where T : struct, INumber<T>
{
    /// <summary><see langword="true"/> when <see cref="Min"/> is less than or equal to <see cref="Max"/>.</summary>
    public bool IsValid => Min <= Max;

    /// <summary>Distance between <see cref="Min"/> and <see cref="Max"/>; equals <c>Max - Min</c>.</summary>
    public T Length => Max - Min;

    /// <summary>Returns <see langword="true"/> when <paramref name="value"/> falls inside the inclusive range.</summary>
    public bool Contains(T value) => value >= Min && value <= Max;

    /// <summary>Clamps <paramref name="value"/> so the result lies inside the inclusive range.</summary>
    public T Clamp(T value)
    {
        if (value < Min)
        {
            return Min;
        }

        if (value > Max)
        {
            return Max;
        }

        return value;
    }

    /// <summary>Returns a copy of the range with <see cref="Min"/> replaced.</summary>
    public BOBNumericRange<T> WithMin(T min) => this with { Min = min };

    /// <summary>Returns a copy of the range with <see cref="Max"/> replaced.</summary>
    public BOBNumericRange<T> WithMax(T max) => this with { Max = max };
}
