using BlazOrbit.Hotkeys;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>DI registration helpers for the <c>BlazOrbit.Hotkeys</c> package.</summary>
public static class HotkeyServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IHotkeyService"/> + the JS interop bridge. Mount a
    /// <c>&lt;BOBHotkeyHost /&gt;</c> once in the app shell to wire the document-level
    /// keydown listener. Pass <paramref name="configure"/> to declare app-wide hotkeys at
    /// composition time:
    /// <code>
    /// builder.Services.AddBlazOrbitHotkeys(opts => opts
    ///     .Register("ctrl+k", "Open command palette", sp => /* navigate */ Task.CompletedTask)
    ///     .Register("?",       "Show shortcuts",      () => Task.CompletedTask));
    /// </code>
    /// </summary>
    public static IServiceCollection AddBlazOrbitHotkeys(
        this IServiceCollection services,
        Action<HotkeyOptions>? configure = null)
    {
        // Capture global registrations on a singleton so they survive across scope boundaries.
        // The scoped HotkeyService factory replays them every time a fresh circuit/page
        // resolves the service so per-circuit teardown does not drop the app-wide handlers.
        HotkeyOptions options = new();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddScoped<IHotkeyService>(sp =>
        {
            HotkeyService svc = new();
            HotkeyOptions opts = sp.GetRequiredService<HotkeyOptions>();
            foreach (HotkeyRegistration reg in opts.Registrations)
            {
                // Discard the IDisposable - global handlers live for the whole circuit /
                // page lifetime. The scoped HotkeyService instance disposes naturally
                // when DI tears down the scope.
                _ = svc.Register(
                    reg.Combo,
                    reg.Description,
                    () => reg.Handler(sp),
                    reg.Scope,
                    reg.PreventDefault);
            }

            return svc;
        });

        services.AddScoped<IHotkeyJsInterop, HotkeyJsInterop>();

        return services;
    }
}