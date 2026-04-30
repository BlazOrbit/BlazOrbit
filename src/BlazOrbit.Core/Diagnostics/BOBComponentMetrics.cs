namespace BlazOrbit.Components;

public sealed class BOBComponentMetrics
{
    public string ComponentType { get; init; } = string.Empty;
    public int RenderCount { get; internal set; }
    public double TotalRenderTreeBuildTimeMs { get; internal set; }
    public double LastRenderTreeBuildTimeMs { get; internal set; }
    public double InitTimeMs { get; internal set; }
    public double TotalParametersSetTimeMs { get; internal set; }
    public double LastParametersSetTimeMs { get; internal set; }
    public int ParametersSetCount { get; internal set; }

    public double AverageRenderTreeBuildTimeMs
        => RenderCount > 0 ? TotalRenderTreeBuildTimeMs / RenderCount : 0;

    public double AverageParametersSetTimeMs
        => ParametersSetCount > 0 ? TotalParametersSetTimeMs / ParametersSetCount : 0;
}