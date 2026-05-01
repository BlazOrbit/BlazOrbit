using System.ComponentModel;

namespace BlazOrbit.Components;

/// <summary>
/// Internal tree-state container shared by `BOBTreeMenu` / `BOBTreeSelector`.
/// Public for reflection / DI / cross-component access; not part of the
/// user-facing API surface.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class TreeStructure<TNode, TItem>
    where TNode : TreeNodeBase<TItem, TNode>
{
    private readonly TreeNodeCache<TItem> _cache = new();
    private readonly HashSet<string> _expandedKeys = [];
    private readonly HashSet<string> _loadingKeys = [];
    private readonly Func<TreeNodeBuildContext<TItem>, TNode> _nodeFactory;
    private readonly Dictionary<string, TNode> _nodeMap = [];

    /// <summary>Creates a new tree state container that uses <paramref name="nodeFactory"/> to materialize nodes.</summary>
    public TreeStructure(Func<TreeNodeBuildContext<TItem>, TNode> nodeFactory) => _nodeFactory = nodeFactory;

    /// <summary>Keys of currently expanded nodes.</summary>
    public IReadOnlySet<string> ExpandedKeys => _expandedKeys;
    /// <summary>Lookup from key to materialized node.</summary>
    public IReadOnlyDictionary<string, TNode> NodeMap => _nodeMap;
    /// <summary>Roots of the materialized tree.</summary>
    public List<TNode> RootNodes { get; } = [];

    /// <summary>Rebuilds the tree from a flat or hierarchical item source.</summary>
    public void BuildFromItems(
        IEnumerable<TItem>? items,
        Func<TItem, string>? keySelector,
        Func<TItem, IEnumerable<TItem>?>? childrenSelector,
        Func<TItem, bool>? hasChildrenSelector,
        bool expandAll)
    {
        Clear();
        if (items == null)
        {
            return;
        }

        foreach (TItem item in items)
        {
            TNode node = BuildNodeFromItem(item, default, 0, keySelector, childrenSelector, hasChildrenSelector, expandAll);
            RootNodes.Add(node);
        }
    }

    /// <summary>Drops all materialized nodes, expanded keys, and roots.</summary>
    public void Clear()
    {
        _nodeMap.Clear();
        RootNodes.Clear();
        _expandedKeys.Clear();
    }

    /// <summary>Removes the node identified by <paramref name="key"/> from the expanded set.</summary>
    public void Collapse(string key) => _expandedKeys.Remove(key);

    /// <summary>Collapses every node.</summary>
    public void CollapseAll() => _expandedKeys.Clear();

    /// <summary>Marks the node identified by <paramref name="key"/> as expanded.</summary>
    public void Expand(string key) => _expandedKeys.Add(key);

    /// <summary>Expands every node that has children.</summary>
    public void ExpandAll()
    {
        foreach (TNode node in _nodeMap.Values.Where(n => n.HasChildren))
        {
            _expandedKeys.Add(node.Key);
        }
    }

    /// <summary>Expands the node only if it currently advertises children.</summary>
    public void ExpandIfHasChildren(string key)
    {
        if (_nodeMap.TryGetValue(key, out TNode? node) && node.HasChildren)
        {
            _expandedKeys.Add(key);
        }
    }

    /// <summary>Returns the materialized node for <paramref name="key"/>, or <see langword="null"/> when unknown.</summary>
    public TNode? GetNode(string key)
        => _nodeMap.TryGetValue(key, out TNode? node) ? node : null;

    /// <summary>Invalidates the lazy-children cache for one key (or every key when <paramref name="key"/> is <see langword="null"/>).</summary>
    public void InvalidateCache(string? key = null)
    {
        if (key != null)
        {
            _cache.Invalidate(key);
        }
        else
        {
            _cache.InvalidateAll();
        }
    }

    /// <summary>Returns <see langword="true"/> when the node is currently expanded.</summary>
    public bool IsExpanded(string key) => _expandedKeys.Contains(key);

    /// <summary>Returns <see langword="true"/> when the node's children are currently loading.</summary>
    public bool IsLoading(string key) => _loadingKeys.Contains(key);

    /// <summary>Lazily loads the children of <paramref name="key"/> via <paramref name="loadFunc"/>, with per-node memoization.</summary>
    public async Task<bool> LoadChildrenAsync(
        string key,
        Func<TItem, Task<IEnumerable<TItem>>>? loadFunc,
        Func<TItem, string>? keySelector,
        Func<TItem, IEnumerable<TItem>?>? childrenSelector,
        Func<TItem, bool>? hasChildrenSelector,
        Action onStateChanged)
    {
        if (loadFunc == null)
        {
            return false;
        }

        if (!_nodeMap.TryGetValue(key, out TNode? node) || node.Item == null)
        {
            return false;
        }

        if (node.ChildrenInternal.Count > 0)
        {
            return true;
        }

        _loadingKeys.Add(key);
        onStateChanged();

        try
        {
            IEnumerable<TItem> children;
            if (_cache.TryGet(key, out IEnumerable<TItem>? cached) && cached != null)
            {
                children = cached;
            }
            else
            {
                children = await loadFunc(node.Item);
                _cache.Set(key, children);
            }

            node.ChildrenInternal.Clear();
            foreach (TItem child in children)
            {
                TNode childNode = BuildNodeFromItem(
                    child, node, node.Depth + 1,
                    keySelector, childrenSelector, hasChildrenSelector, false);
                node.ChildrenInternal.Add(childNode);
            }

            node.HasChildrenFlag = node.ChildrenInternal.Count > 0;
            return true;
        }
        finally
        {
            _loadingKeys.Remove(key);
            onStateChanged();
        }
    }

    /// <summary>Registers <paramref name="node"/> in the key-to-node lookup.</summary>
    public void RegisterNode(TNode node) => _nodeMap[node.Key] = node;

    /// <summary>Replaces the expanded-keys set with the given collection (or empties it when <paramref name="keys"/> is <see langword="null"/>).</summary>
    public void SetExpandedKeys(IEnumerable<string>? keys)
    {
        _expandedKeys.Clear();
        if (keys != null)
        {
            foreach (string key in keys)
            {
                _expandedKeys.Add(key);
            }
        }
    }

    /// <summary>Flips the expanded state of the node identified by <paramref name="key"/>.</summary>
    public void Toggle(string key)
    {
        if (IsExpanded(key))
        {
            Collapse(key);
        }
        else
        {
            Expand(key);
        }
    }

    private TNode BuildNodeFromItem(
        TItem item,
        TNode? parent,
        int depth,
        Func<TItem, string>? keySelector,
        Func<TItem, IEnumerable<TItem>?>? childrenSelector,
        Func<TItem, bool>? hasChildrenSelector,
        bool expandAll)
    {
        string key = keySelector?.Invoke(item) ?? $"item-{Guid.NewGuid():N}";
        IEnumerable<TItem>? children = childrenSelector?.Invoke(item);
        bool hasChildren = hasChildrenSelector?.Invoke(item) ?? children?.Any() ?? false;

        TreeNodeBuildContext<TItem> context = new()
        {
            Key = key,
            Item = item,
            Depth = depth,
            Parent = parent,
            HasChildren = hasChildren
        };

        TNode node = _nodeFactory(context);
        node.HasChildrenFlag = hasChildren;
        _nodeMap[key] = node;

        if (expandAll && hasChildren)
        {
            _expandedKeys.Add(key);
        }

        if (children != null)
        {
            foreach (TItem child in children)
            {
                TNode childNode = BuildNodeFromItem(
                    child, node, depth + 1,
                    keySelector, childrenSelector, hasChildrenSelector, expandAll);
                node.ChildrenInternal.Add(childNode);
            }
        }

        return node;
    }
}