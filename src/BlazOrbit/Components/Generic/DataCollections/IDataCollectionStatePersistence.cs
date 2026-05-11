namespace BlazOrbit.Components;

/// <summary>
/// Strategy for persisting <see cref="DataCollectionState{TItem}"/> across renders /
/// reloads. The default registration is <see cref="NullStatePersistence"/> (no-op);
/// register <see cref="LocalStorageStatePersistence"/> via
/// <c>AddBlazOrbitDataCollectionLocalStorage()</c> to opt every grid in to
/// <c>localStorage</c>-backed persistence, or supply a custom implementation for query
/// string / server-side stores.
/// </summary>
public interface IDataCollectionStatePersistence
{
    /// <summary>
    /// Reads a previously persisted snapshot. Returns <see langword="null"/> when no
    /// payload is stored, the entry is malformed, or the underlying storage is
    /// unavailable (e.g. SSR without a JS runtime).
    /// </summary>
    ValueTask<string?> LoadAsync(string key);

    /// <summary>
    /// Persists a JSON-shaped payload under <paramref name="key"/>. Implementations swallow
    /// transient storage failures so consumers don't need to wrap each call in a try/catch.
    /// </summary>
    ValueTask SaveAsync(string key, string payload);
}

/// <summary>No-op persistence — every <see cref="LoadAsync"/> returns <see langword="null"/>, every save is dropped. Default registration.</summary>
public sealed class NullStatePersistence : IDataCollectionStatePersistence
{
    /// <inheritdoc />
    public ValueTask<string?> LoadAsync(string key) => ValueTask.FromResult<string?>(null);

    /// <inheritdoc />
    public ValueTask SaveAsync(string key, string payload) => ValueTask.CompletedTask;
}
