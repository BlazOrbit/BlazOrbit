using System.ComponentModel;

namespace BlazOrbit.Components;

/// <summary>
/// Per-key memoization for lazily loaded tree children. Plumbing used by
/// <see cref="TreeStructure{TNode, TItem}"/>; not user-facing.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class TreeNodeCache<TItem>
{
    private readonly Dictionary<string, IReadOnlyList<TItem>> _cache = [];

    public void Invalidate(string key) => _cache.Remove(key);

    public void InvalidateAll() => _cache.Clear();

    public void Set(string key, IEnumerable<TItem> children)
        => _cache[key] = children.ToList();

    public bool TryGet(string key, out IEnumerable<TItem>? children)
    {
        if (_cache.TryGetValue(key, out IReadOnlyList<TItem>? entry))
        {
            children = entry;
            return true;
        }

        children = null;
        return false;
    }
}