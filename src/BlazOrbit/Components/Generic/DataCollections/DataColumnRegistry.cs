namespace BlazOrbit.Components;

internal interface IDataColumnRegistry<TItem>
{
    IReadOnlyList<DataColumnRegistration<TItem>> Columns { get; }
    void RegisterColumn(DataColumnRegistration<TItem> column);
}

public sealed class DataColumnRegistry<TItem> : IDataColumnRegistry<TItem>
{
    private readonly List<DataColumnRegistration<TItem>> _columns = [];

    public IReadOnlyList<DataColumnRegistration<TItem>> Columns => _columns;

    public void Clear() => _columns.Clear();

    public void RegisterColumn(DataColumnRegistration<TItem> column)
        => _columns.Add(column);
}