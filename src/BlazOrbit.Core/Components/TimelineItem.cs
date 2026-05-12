namespace BlazOrbit.Components;

/// <summary>
/// Single event consumed by <c>BOBTimeline</c>
/// (<c>BlazOrbit.Components.Navigation</c>).
/// </summary>
/// <param name="Timestamp">Event time. Used for sorting headers when grouping by date.</param>
/// <param name="Title">Primary line shown next to the marker.</param>
/// <param name="Description">Optional secondary text below the title.</param>
/// <param name="Icon">Optional icon rendered inside the marker dot.</param>
/// <param name="Color">
/// Optional accent color applied to the marker. Accepts any valid CSS color value;
/// falls back to the palette highlight when unset.
/// </param>
public readonly record struct TimelineItem(
    DateTimeOffset Timestamp,
    string Title,
    string? Description = null,
    IconKey? Icon = null,
    string? Color = null);