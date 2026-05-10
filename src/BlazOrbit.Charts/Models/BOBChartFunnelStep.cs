namespace BlazOrbit.Charts.Models;

/// <summary>
/// A single stage of a <see cref="Components.BOBFunnelChart{TY}"/>:
/// label + numeric value. Steps are listed top → bottom in declaration
/// order; the rendered funnel auto-scales each row's width by the
/// largest value.
/// </summary>
/// <typeparam name="TY">Numeric value type.</typeparam>
public sealed class BOBChartFunnelStep<TY>
{
    /// <summary>Label rendered on the row.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Numeric value (typically count / amount).</summary>
    public TY Value { get; init; } = default!;

    /// <summary>Optional explicit color override.</summary>
    public string? Color { get; init; }
}
