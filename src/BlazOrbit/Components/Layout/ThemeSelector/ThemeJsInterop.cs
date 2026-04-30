using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.JSInterop;

namespace BlazOrbit.Components.Layout;

public interface IThemeJsInterop
{
    event Action<string>? OnThemeChanged;

    ValueTask<string> GetThemeAsync();

    ValueTask InitializeAsync(string? defaultTheme = null);

    ValueTask SetThemeAsync(string theme);

    ValueTask<string> ToggleThemeAsync(string[] themes);

    ValueTask<Dictionary<string, string>> GetPaletteAsync();
}

internal sealed class ThemeJsInterop(IJSRuntime jsRuntime)
    : ModuleJsInteropBase(jsRuntime, JSModulesReference.ThemeJs),
      IThemeJsInterop
{
    public event Action<string>? OnThemeChanged;

    public async ValueTask<string> GetThemeAsync()
    {
        IJSObjectReference module = await ModuleTask.Value;
        return await module.InvokeAsync<string>("getTheme");
    }

    public async ValueTask InitializeAsync(string? defaultTheme = null)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("initialize", defaultTheme);
    }

    public async ValueTask SetThemeAsync(string theme)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("setTheme", theme);
        OnThemeChanged?.Invoke(theme);
    }

    public async ValueTask<string> ToggleThemeAsync(params string[] themes)
    {
        IJSObjectReference module = await ModuleTask.Value;
        string newTheme = await module.InvokeAsync<string>("toggleTheme", new object[] { themes });
        OnThemeChanged?.Invoke(newTheme);
        return newTheme;
    }

    public async ValueTask<Dictionary<string, string>> GetPaletteAsync()
    {
        IJSObjectReference module = await ModuleTask.Value;
        return await module.InvokeAsync<Dictionary<string, string>>("getPalette");
    }
}