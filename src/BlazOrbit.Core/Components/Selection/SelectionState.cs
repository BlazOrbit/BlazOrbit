namespace BlazOrbit.Abstractions;

public sealed class SelectionState<TValue>
{
    private readonly IEqualityComparer<object> _comparer;
    private readonly HashSet<object> _selectedValues;
    private readonly SelectionTypeInfo _typeInfo;

    public SelectionState() : this(new ValueEqualityComparer())
    {
    }

    public SelectionState(IEqualityComparer<object> comparer)
    {
        _typeInfo = new SelectionTypeInfo(typeof(TValue));
        _comparer = comparer;
        _selectedValues = new HashSet<object>(_comparer);
    }

    public event Action? StateChanged;

    public int Count => _selectedValues.Count;
    public Type ElementType => _typeInfo.ElementType;
    public bool IsMultiple => _typeInfo.IsMultiple;
    public IReadOnlyCollection<object> SelectedValues => _selectedValues;

    public void Clear()
    {
        _selectedValues.Clear();
        NotifyStateChanged();
    }

    public void Deselect(object? value)
    {
        if (value == null)
        {
            return;
        }

        _selectedValues.Remove(value);
        NotifyStateChanged();
    }

    public TValue GetValue()
                => _typeInfo.CreateValue<TValue>(_selectedValues);

    public bool IsSelected(object? value)
        => value != null && _selectedValues.Contains(value);

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

    public void SetSingleValue(object? value)
    {
        _selectedValues.Clear();

        if (value != null)
        {
            _selectedValues.Add(value);
        }

        NotifyStateChanged();
    }

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
        public new bool Equals(object? x, object? y) => (x == null && y == null) || (x != null && y != null && x.Equals(y));

        public int GetHashCode(object obj)
            => obj?.GetHashCode() ?? 0;
    }
}