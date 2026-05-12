namespace BlazOrbit.Charts.Models;

/// <summary>
/// Colored segment of a <see cref="Components.BOBGaugeChart"/> arc track.
/// Defines a fixed sub-range [<see cref="From"/>, <see cref="To"/>] inside
/// the gauge's [<c>Min</c>, <c>Max</c>] domain. Useful to indicate
/// thresholds visually (green / amber / red zones).
/// </summary>
public sealed class BOBChartGaugeSegment
{
    /// <summary>Inclusive lower bound on the gauge domain.</summary>
    public double From { get; init; }

    /// <summary>Inclusive upper bound on the gauge domain.</summary>
    public double To { get; init; }

    /// <summary>Fill / stroke color (CSS).</summary>
    public string Color { get; init; } = "var(--palette-primary, #2563eb)";

    /// <summary>Optional label for accessibility / tooltip.</summary>
    public string? Label { get; init; }
}