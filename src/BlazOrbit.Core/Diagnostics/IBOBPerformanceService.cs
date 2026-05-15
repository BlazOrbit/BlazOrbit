using System.Collections.Concurrent;

namespace BlazOrbit.Components;

/// <summary>DEBUG-only diagnostic service collecting per-component render/init timing.</summary>
public interface IBOBPerformanceService
{
    /// <summary>Records a <c>BuildRenderTree</c> sample for the given component type.</summary>
    void RecordRenderTreeBuild(string componentType, double elapsedMs);

    /// <summary>Records the <c>OnInitialized</c> sample for the given component type.</summary>
    void RecordInit(string componentType, double elapsedMs);

    /// <summary>Records an <c>OnParametersSet</c> sample for the given component type.</summary>
    void RecordParametersSet(string componentType, double elapsedMs);

    /// <summary>Returns a snapshot of all collected metrics, ordered by total render-tree time.</summary>
    IReadOnlyCollection<BOBComponentMetrics> GetAll();

    /// <summary>Returns the metrics for a specific component type, or null when none have been recorded.</summary>
    BOBComponentMetrics? Get(string componentType);

    /// <summary>Clears all collected metrics.</summary>
    void Reset();

    /// <summary>Raised after every recorded sample or <see cref="Reset"/>.</summary>
    event Action? MetricsUpdated;
}

/// <summary>Default in-memory <see cref="IBOBPerformanceService"/> implementation.</summary>
public sealed class BOBPerformanceService : IBOBPerformanceService
{
    private readonly ConcurrentDictionary<string, BOBComponentMetrics> _metrics = new();

    /// <inheritdoc />
    public event Action? MetricsUpdated;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void RecordInit(string componentType, double elapsedMs)
    {
        _metrics.AddOrUpdate(
            componentType,
            key => new BOBComponentMetrics { ComponentType = key, InitTimeMs = elapsedMs },
            (_, existing) =>
            {
                existing.InitTimeMs = elapsedMs;
                return existing;
            });

        MetricsUpdated?.Invoke();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IReadOnlyCollection<BOBComponentMetrics> GetAll()
        => _metrics.Values.OrderByDescending(m => m.TotalRenderTreeBuildTimeMs).ToList();

    /// <inheritdoc />
    public BOBComponentMetrics? Get(string componentType)
        => _metrics.GetValueOrDefault(componentType);

    /// <inheritdoc />
    public void Reset()
    {
        _metrics.Clear();
        MetricsUpdated?.Invoke();
    }
}
