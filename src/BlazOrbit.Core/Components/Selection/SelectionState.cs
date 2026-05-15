namespace BlazOrbit.Abstractions;

/// <summary>Tracks the selected value(s) for a selection-capable component, supporting both single and multiple modes inferred from <typeparamref name="TValue"/>.</summary>
public sealed class SelectionState<TValue>
{
    private readonly IEqualityComparer<object> _comparer;
    private readonly HashSet<object> _selectedValues;
    private readonly SelectionTypeInfo _typeInfo;

    /// <summary>Creates a new state with the default value-equality comparer.</summary>
    public SelectionState() : this(new ValueEqualityComparer())
    {
    }

    /// <summary>Creates a new state with a custom equality comparer for selection items.</summary>
    public SelectionState(IEqualityComparer<object> comparer)
    {
        _typeInfo = new SelectionTypeInfo(typeof(TValue));
        _comparer = comparer;
        _selectedValues = new HashSet<object>(_comparer);
    }

    /// <summary>Raised whenever the set of selected values changes.</summary>
    public event Action? StateChanged;

    /// <summary>Number of currently selected items.</summary>
    public int Count => _selectedValues.Count;

    /// <summary>Element type of the selection (the inner type when <typeparamref name="TValue"/> is a collection).</summary>
    public Type ElementType => _typeInfo.ElementType;

    /// <summary>True when <typeparamref name="TValue"/> represents a multi-selection (collection) value.</summary>
    public bool IsMultiple => _typeInfo.IsMultiple;

    /// <summary>Snapshot of the currently selected values.</summary>
    public IReadOnlyCollection<object> SelectedValues => _selectedValues;

    /// <summary>Clears all selected values and raises <see cref="StateChanged"/>.</summary>
    public void Clear()
    {
        _selectedValues.Clear();
        NotifyStateChanged();
    }

    /// <summary>Removes a value from the selection. No-op when <paramref name="value"/> is null.</summary>
    public void Deselect(object? value)
    {
        if (value == null)
        {
            return;
        }

        _selectedValues.Remove(value);
        NotifyStateChanged();
    }

    /// <summary>Materializes the selection as a value of <typeparamref name="TValue"/>.</summary>
    public TValue GetValue()
        => _typeInfo.CreateValue<TValue>(_selectedValues);

    /// <summary>Returns whether <paramref name="value"/> is part of the current selection.</summary>
    public bool IsSelected(object? value)
        => value != null && _selectedValues.Contains(value);

    /// <summary>Adds <paramref name="value"/> to the selection (replacing the previous value in single mode).</summary>
    public void Select(object? value)
    {
        if (value == null)
        {
            return;
        }

        if (!IsMultiple)
        {
            _selectedValues.Clear();
        }

        _selectedValues.Add(value);
        NotifyStateChanged();
    }

    /// <summary>Adds every non-null entry of <paramref name="values"/> to the selection. No-op in single mode.</summary>
    public void SelectAll(IEnumerable<object?> values)
    {
        if (!IsMultiple)
        {
            return;
        }

        foreach (object? value in values)
        {
            if (value != null)
            {
                _selectedValues.Add(value);
            }
        }

        NotifyStateChanged();
    }

    /// <summary>Replaces the selection with a single value (or clears it when null), regardless of mode.</summary>
    public void SetSingleValue(object? value)
    {
        _selectedValues.Clear();

        if (value != null)
        {
            _selectedValues.Add(value);
        }

        NotifyStateChanged();
    }

    /// <summary>Replaces the selection from a <typeparamref name="TValue"/> instance.</summary>
    public void SetValue(TValue? value)
    {
        _selectedValues.Clear();

        if (value != null)
        {
            foreach (object item in _typeInfo.ExtractValues(value))
            {
                _selectedValues.Add(item);
            }
        }

        NotifyStateChanged();
    }

    /// <summary>Adds or removes <paramref name="value"/> based on its current selection state.</summary>
    public void Toggle(object? value)
    {
        if (value == null)
        {
            return;
        }

        if (IsSelected(value))
        {
            Deselect(value);
        }
        else
        {
            Select(value);
        }
    }

    private void NotifyStateChanged()
        => StateChanged?.Invoke();

    private sealed class ValueEqualityComparer : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y) =>
            (x == null && y == null) || (x != null && y != null && x.Equals(y));

        public int GetHashCode(object obj)
            => obj?.GetHashCode() ?? 0;
    }
}