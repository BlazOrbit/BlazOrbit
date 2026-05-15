namespace BlazOrbit.Components;

/// <summary>Accumulated diagnostic metrics for a single component type.</summary>
public sealed class BOBComponentMetrics
{
    /// <summary>Fully-qualified component type name.</summary>
    public string ComponentType { get; init; } = string.Empty;

    /// <summary>Number of <c>BuildRenderTree</c> samples recorded.</summary>
    public int RenderCount { get; internal set; }

    /// <summary>Sum of all <c>BuildRenderTree</c> samples, in milliseconds.</summary>
    public double TotalRenderTreeBuildTimeMs { get; internal set; }

    /// <summary>Most recent <c>BuildRenderTree</c> sample, in milliseconds.</summary>
    public double LastRenderTreeBuildTimeMs { get; internal set; }

    /// <summary>First-render initialization time, in milliseconds.</summary>
    public double InitTimeMs { get; internal set; }

    /// <summary>Sum of all <c>OnParametersSet</c> samples, in milliseconds.</summary>
    public double TotalParametersSetTimeMs { get; internal set; }

    /// <summary>Most recent <c>OnParametersSet</c> sample, in milliseconds.</summary>
    public double LastParametersSetTimeMs { get; internal set; }

    /// <summary>Number of <c>OnParametersSet</c> samples recorded.</summary>
    public int ParametersSetCount { get; internal set; }

    /// <summary>Mean <c>BuildRenderTree</c> time across all samples.</summary>
    public double AverageRenderTreeBuildTimeMs
        => RenderCount > 0 ? TotalRenderTreeBuildTimeMs / RenderCount : 0;

    /// <summary>Mean <c>OnParametersSet</c> time across all samples.</summary>
    public double AverageParametersSetTimeMs
        => ParametersSetCount > 0 ? TotalParametersSetTimeMs / ParametersSetCount : 0;
}
