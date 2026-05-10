using Microsoft.JSInterop;

namespace BlazOrbit.Components;

/// <summary>
/// <c>localStorage</c>-backed implementation of <see cref="IDataCollectionStatePersistence"/>.
/// Goes through the browser's built-in <c>localStorage.getItem</c> /
/// <c>localStorage.setItem</c> via inline JS interop — no module load, no extra TypeScript.
/// Failures (SSR without circuit, runtime disposed, storage quota) are swallowed so a
/// missing browser environment never crashes the grid.
/// </summary>
public sealed class LocalStorageStatePersistence : IDataCollectionStatePersistence
{
    private readonly IJSRuntime _js;

    /// <summary>Initializes a new <see cref="LocalStorageStatePersistence"/>.</summary>
    public LocalStorageStatePersistence(IJSRuntime js)
    {
        _js = js;
    }

    /// <inheritdoc />
    public async ValueTask<string?> LoadAsync(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (JSDisconnectedException) { return null; }
        catch (ObjectDisposedException) { return null; }
        catch (InvalidOperationException) { return null; }
        catch (TaskCanceledException) { return null; }
        catch (JSException) { return null; }
    }

    /// <inheritdoc />
    public async ValueTask SaveAsync(string key, string payload)
    {
        if (string.IsNullOrEmpty(key)) return;
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", key, payload);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }
        catch (TaskCanceledException) { }
        catch (JSException) { }
    }
}
