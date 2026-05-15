using System.Globalization;

namespace BlazOrbit.Charts.Components.Internal;

/// <summary>
/// Helpers to project arbitrary <c>TY</c> generic values onto the
/// <see cref="double"/> values the scale engines expect. Charts accept any
/// type the consumer fancies (<c>int</c>, <c>decimal</c>, <c>double</c>, etc.);
/// the scale engines are double-only for performance and arithmetic
/// simplicity.
/// </summary>
internal static class Numeric
{
    /// <summary>
    /// Convert <paramref name="value"/> to <see cref="double"/> using the
    /// invariant culture. Returns <c>0</c> for <c>null</c>. Throws for
    /// non-convertible types - chart callers should constrain numeric type
    /// parameters to <c>int</c>, <c>long</c>, <c>float</c>, <c>double</c>,
    /// <c>decimal</c>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>
    /// or <see cref="TimeSpan"/> at the API boundary.
    /// <para>
    /// Temporal types are projected via <c>Ticks</c> so a single linear
    /// scale handles both numeric and time domains.
    /// </para>
    /// </summary>
    public static double ToDouble<T>(T? value)
    {
        if (value is null)
        {
            return 0d;
        }

        if (value is double d)
        {
            return d;
        }

        if (value is float f)
        {
            return f;
        }

        if (value is decimal m)
        {
            return (double)m;
        }

        if (value is int i)
        {
            return i;
        }

        if (value is long l)
        {
            return l;
        }

        if (value is short sh)
        {
            return sh;
        }

        if (value is uint ui)
        {
            return ui;
        }

        if (value is ulong ul)
        {
            return ul;
        }

        if (value is ushort us)
        {
            return us;
        }

        if (value is byte b)
        {
            return b;
        }

        if (value is sbyte sb)
        {
            return sb;
        }

        if (value is DateTime dt)
        {
            return dt.Ticks;
        }

        if (value is DateTimeOffset dto)
        {
            return dto.UtcTicks;
        }

        if (value is TimeSpan ts)
        {
            return ts.Ticks;
        }

        if (value is IConvertible c)
        {
            return c.ToDouble(CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException(
            $"Cannot project value of type '{typeof(T).FullName}' onto a numeric chart axis.");
    }

    /// <summary>
    /// True when <typeparamref name="T"/> is a type the chart engines treat
    /// as a continuous numeric / temporal axis (vs. a categorical one).
    /// </summary>
    public static bool IsContinuous<T>()
    {
        Type t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        return t == typeof(double)
               || t == typeof(float)
               || t == typeof(decimal)
               || t == typeof(int)
               || t == typeof(long)
               || t == typeof(short)
               || t == typeof(uint)
               || t == typeof(ulong)
               || t == typeof(ushort)
               || t == typeof(byte)
               || t == typeof(sbyte)
               || t == typeof(DateTime)
               || t == typeof(DateTimeOffset)
               || t == typeof(TimeSpan);
    }

    /// <summary>
    /// Inverse of <see cref="ToDouble"/>: project a <see cref="double"/>
    /// (typically a domain coordinate inverted from a pixel position) back
    /// into the original <typeparamref name="T"/> type. Temporal types are
    /// reconstructed from <c>Ticks</c>; numeric types pass through the
    /// usual narrowing conversions.
    /// </summary>
    public static T FromDouble<T>(double value)
    {
        Type t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (t == typeof(double))
        {
            return (T)(object)value;
        }

        if (t == typeof(float))
        {
            return (T)(object)(float)value;
        }

        if (t == typeof(decimal))
        {
            return (T)(object)(decimal)value;
        }

        if (t == typeof(int))
        {
            return (T)(object)(int)value;
        }

        if (t == typeof(long))
        {
            return (T)(object)(long)value;
        }

        if (t == typeof(short))
        {
            return (T)(object)(short)value;
        }

        if (t == typeof(uint))
        {
            return (T)(object)(uint)value;
        }

        if (t == typeof(ulong))
        {
            return (T)(object)(ulong)value;
        }

        if (t == typeof(ushort))
        {
            return (T)(object)(ushort)value;
        }

        if (t == typeof(byte))
        {
            return (T)(object)(byte)value;
        }

        if (t == typeof(sbyte))
        {
            return (T)(object)(sbyte)value;
        }

        if (t == typeof(DateTime))
        {
            return (T)(object)new DateTime((long)value);
        }

        if (t == typeof(DateTimeOffset))
        {
            return (T)(object)new DateTimeOffset(new DateTime((long)value, DateTimeKind.Utc));
        }

        if (t == typeof(TimeSpan))
        {
            return (T)(object)TimeSpan.FromTicks((long)value);
        }

        throw new InvalidOperationException(
            $"Cannot project double back to type '{typeof(T).FullName}'.");
    }

    /// <summary>
    /// Format a tick value for the X axis given the original
    /// <typeparamref name="T"/> domain type. Temporal ticks are projected
    /// back from <c>double</c> ticks-space; numeric values use the supplied
    /// format (or general-purpose <c>"G"</c>).
    /// </summary>
    public static string FormatContinuous<T>(double tickValue, string? format, string? numericDefault = null)
    {
        Type t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (t == typeof(DateTime))
        {
            DateTime dt = new((long)tickValue);
            return dt.ToString(format ?? "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (t == typeof(DateTimeOffset))
        {
            DateTimeOffset dto = new(new DateTime((long)tickValue, DateTimeKind.Utc));
            return dto.ToString(format ?? "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (t == typeof(TimeSpan))
        {
            TimeSpan ts = TimeSpan.FromTicks((long)tickValue);
            return ts.ToString(format ?? "c", CultureInfo.InvariantCulture);
        }

        return tickValue.ToString(format ?? numericDefault ?? "G", CultureInfo.InvariantCulture);
    }
}