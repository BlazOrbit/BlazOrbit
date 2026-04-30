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

    public TreeStructure(Func<TreeNodeBuildContext<TItem>, TNode> nodeFactory) => _nodeFactory = nodeFactory;

    public IReadOnlySet<string> ExpandedKeys => _expandedKeys;
    public IReadOnlyDictionary<string, TNode> NodeMap => _nodeMap;
    public List<TNode> RootNodes { get; } = [];

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

    public void Clear()
    {
        _nodeMap.Clear();
        RootNodes.Clear();
        _expandedKeys.Clear();
    }

    public void Collapse(string key) => _expandedKeys.Remove(key);

    public void CollapseAll() => _expandedKeys.Clear();

    public void Expand(string key) => _expandedKeys.Add(key);

    public void ExpandAll()
    {
        foreach (TNode node in _nodeMap.Values.Where(n => n.HasChildren))
        {
            _expandedKeys.Add(node.Key);
        }
    }

    public void ExpandIfHasChildren(string key)
    {
        if (_nodeMap.TryGetValue(key, out TNode? node) && node.HasChildren)
        {
            _expandedKeys.Add(key);
        }
    }

    public TNode? GetNode(string key)
        => _nodeMap.TryGetValue(key, out TNode? node) ? node : null;

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

    public bool IsExpanded(string key) => _expandedKeys.Contains(key);

    public bool IsLoading(string key) => _loadingKeys.Contains(key);

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

    public void RegisterNode(TNode node) => _nodeMap[node.Key] = node;

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