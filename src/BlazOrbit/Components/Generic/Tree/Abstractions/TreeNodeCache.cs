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

    /// <summary>Drops the cached children for a single key.</summary>
    public void Invalidate(string key) => _cache.Remove(key);

    /// <summary>Drops every cached entry.</summary>
    public void InvalidateAll() => _cache.Clear();

    /// <summary>Stores the children list against the given key.</summary>
    public void Set(string key, IEnumerable<TItem> children)
        => _cache[key] = children.ToList();

    /// <summary>Attempts to retrieve cached children for the given key.</summary>
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