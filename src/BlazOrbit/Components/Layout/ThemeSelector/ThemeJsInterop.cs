using BlazOrbit.Abstractions;
using BlazOrbit.Types;
using Microsoft.JSInterop;

namespace BlazOrbit.Components.Layout;

/// <summary>JS interop contract for the theme module — reads/writes the active theme on the document root.</summary>
public interface IThemeJsInterop
{
    /// <summary>Raised after a successful theme change with the new theme id.</summary>
    event Action<string>? OnThemeChanged;

    /// <summary>Returns the currently active theme id.</summary>
    ValueTask<string> GetThemeAsync();

    /// <summary>Initializes the theme module, optionally with a default theme id.</summary>
    ValueTask InitializeAsync(string? defaultTheme = null);

    /// <summary>Sets the active theme id and persists it.</summary>
    ValueTask SetThemeAsync(string theme);

    /// <summary>Cycles to the next id in <paramref name="themes"/> and returns the resulting theme.</summary>
    ValueTask<string> ToggleThemeAsync(string[] themes);

    /// <summary>Returns the active palette as a CSS-variable dictionary.</summary>
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