using System.Collections.Concurrent;

namespace BlazOrbit.Components;

public interface IBOBPerformanceService
{
    void RecordRenderTreeBuild(string componentType, double elapsedMs);
    void RecordInit(string componentType, double elapsedMs);
    void RecordParametersSet(string componentType, double elapsedMs);
    IReadOnlyCollection<BOBComponentMetrics> GetAll();
    BOBComponentMetrics? Get(string componentType);
    void Reset();
    event Action? MetricsUpdated;
}

public sealed class BOBPerformanceService : IBOBPerformanceService
{
    private readonly ConcurrentDictionary<string, BOBComponentMetrics> _metrics = new();

    public event Action? MetricsUpdated;

    public void RecordRenderTreeBuild(string componentType, double elapsedMs)
    {
        _metrics.AddOrUpdate(
            componentType,
            key => new BOBComponentMetrics
            {
                ComponentType = key,
                RenderCount = 1,
                TotalRenderTreeBuildTimeMs = elapsedMs,
                LastRenderTreeBuildTimeMs = elapsedMs
            },
            (_, existing) =>
            {
                existing.RenderCount++;
                existing.TotalRenderTreeBuildTimeMs += elapsedMs;
                existing.LastRenderTreeBuildTimeMs = elapsedMs;
                return existing;
            });

        MetricsUpdated?.Invoke();
    }

    public void RecordInit(string componentType, double elapsedMs)
    {
        _metrics.AddOrUpdate(
            componentType,
            key => new BOBComponentMetrics { ComponentType = key, InitTimeMs = elapsedMs },
            (_, existing) => { existing.InitTimeMs = elapsedMs; return existing; });

        MetricsUpdated?.Invoke();
    }

    public void RecordParametersSet(string componentType, double elapsedMs)
    {
        _metrics.AddOrUpdate(
            componentType,
            key => new BOBComponentMetrics
            {
                ComponentType = key,
                ParametersSetCount = 1,
                TotalParametersSetTimeMs = elapsedMs,
                LastParametersSetTimeMs = elapsedMs
            },
            (_, existing) =>
            {
                existing.ParametersSetCount++;
                existing.TotalParametersSetTimeMs += elapsedMs;
                existing.LastParametersSetTimeMs = elapsedMs;
                return existing;
            });

        MetricsUpdated?.Invoke();
    }

    public IReadOnlyCollection<BOBComponentMetrics> GetAll()
        => _metrics.Values.OrderByDescending(m => m.TotalRenderTreeBuildTimeMs).ToList();

    public BOBComponentMetrics? Get(string componentType)
        => _metrics.GetValueOrDefault(componentType);

    public void Reset()
    {
        _metrics.Clear();
        MetricsUpdated?.Invoke();
    }
}