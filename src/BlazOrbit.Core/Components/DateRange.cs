namespace BlazOrbit.Components;

/// <summary>
/// Inclusive start/end date pair used by <c>BOBInputDateRange</c> (BlazOrbit.Components.Forms).
/// Either bound may be <see langword="null" /> to indicate an open-ended range.
/// </summary>
/// <param name="Start">Start of the range, inclusive.</param>
/// <param name="End">End of the range, inclusive. Must be greater than or equal to <paramref name="Start"/> when both are set.</param>
public readonly record struct DateRange(DateOnly? Start, DateOnly? End)
{
    /// <summary>Both bounds are set.</summary>
    public bool IsComplete => Start.HasValue && End.HasValue;

    /// <summary>Both bounds are unset.</summary>
    public bool IsEmpty => !Start.HasValue && !End.HasValue;

    /// <summary>
    /// Day count between <see cref="Start"/> and <see cref="End"/>, inclusive. Returns <c>0</c>
    /// when either bound is missing.
    /// </summary>
    public int DayCount => IsComplete ? End!.Value.DayNumber - Start!.Value.DayNumber + 1 : 0;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="date"/> falls within the inclusive
    /// range. Open-ended ranges treat the missing bound as unbounded on that side.
    /// </summary>
    public bool Contains(DateOnly date) =>
        (!Start.HasValue || date >= Start.Value) &&
        (!End.HasValue || date <= End.Value);

    /// <summary>Last 7 days ending today (inclusive).</summary>
    public static DateRange Last7Days() => FromTodayMinusDays(6);

    /// <summary>Last 30 days ending today (inclusive).</summary>
    public static DateRange Last30Days() => FromTodayMinusDays(29);

    /// <summary>The current calendar month.</summary>
    public static DateRange ThisMonth()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly start = new(today.Year, today.Month, 1);
        DateOnly end = start.AddMonths(1).AddDays(-1);
        return new DateRange(start, end);
    }

    private static DateRange FromTodayMinusDays(int days)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        return new DateRange(today.AddDays(-days), today);
    }
}