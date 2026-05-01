namespace BlazOrbit.Components;

internal interface IDataColumnRegistry<TItem>
{
    IReadOnlyList<DataColumnRegistration<TItem>> Columns { get; }
    void RegisterColumn(DataColumnRegistration<TItem> column);
}

/// <summary>Holds the column definitions registered against a data collection component.</summary>
public sealed class DataColumnRegistry<TItem> : IDataColumnRegistry<TItem>
{
    private readonly List<DataColumnRegistration<TItem>> _columns = [];

    /// <summary>Currently registered columns, in declaration order.</summary>
    public IReadOnlyList<DataColumnRegistration<TItem>> Columns => _columns;

    /// <summary>Clears all registered columns.</summary>
    public void Clear() => _columns.Clear();

    /// <summary>Adds a column registration to the registry.</summary>
    public void RegisterColumn(DataColumnRegistration<TItem> column)
        => _columns.Add(column);
}