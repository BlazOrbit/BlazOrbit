using Microsoft.JSInterop;

namespace BlazOrbit.Localization.Wasm;

public interface ILocalizationPersistence
{
    Task<string?> GetStoredCultureAsync();

    Task SetStoredCultureAsync(string culture);
}

internal class WasmLocalizationPersistence : ILocalizationPersistence, IAsyncDisposable
{
    public const string CULTURE_KEY = "BlazOrbit.Culture";

    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public WasmLocalizationPersistence(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    public async Task<string?> GetStoredCultureAsync()
    {
        try
        {
            IJSObjectReference module = await GetModuleAsync();
            return await module.InvokeAsync<string?>("get", CULTURE_KEY);
        }
        catch (JSDisconnectedException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (JSException)
        {
            return null;
        }
    }

    public async Task SetStoredCultureAsync(string culture)
    {
        IJSObjectReference module = await GetModuleAsync();
        await module.InvokeVoidAsync("set", CULTURE_KEY, culture);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }
        catch (TaskCanceledException) { }
        finally
        {
            _module = null;
        }
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/BlazOrbit/js/Types/Storage/LocalStorageInterop.min.js");
        return _module;
    }
}