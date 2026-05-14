namespace BlazOrbit.Charts.Models;

/// <summary>
/// Snapshot fired by the chart whenever a streaming append (or window
/// trim) settles. Useful for status banners ("12k points buffered") or
/// downstream observability.
/// </summary>
/// <typeparam name="TX">X-axis domain type.</typeparam>
/// <typeparam name="TY">Numeric Y-axis domain type.</typeparam>
public sealed class BOBChartStreamArgs<TX, TY>
    where TX : notnull
{
    /// <summary>Label of the series that received the update.</summary>
    public string SeriesLabel { get; init; } = string.Empty;

    /// <summary>Number of points appended in this batch.</summary>
    public int AppendedCount { get; init; }

    /// <summary>Number of points retained for the series after window trim.</summary>
    public int TotalCount { get; init; }

    /// <summary>Number of points evicted from the head by the FIFO window.</summary>
    public int DroppedByWindow { get; init; }
}