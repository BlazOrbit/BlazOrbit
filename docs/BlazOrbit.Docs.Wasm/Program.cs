using BlazOrbit.Charts.Services;
﻿using BlazOrbit.Docs.Wasm;
using BlazOrbit.Docs.Wasm.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazOrbit();
builder.Services.AddBlazOrbitCharts();
// Opt every BOBDataGrid / BOBDataCards instance with a PersistenceKey in to
// localStorage-backed state persistence — the docs site uses it for the State Persistence
// demo so reloading the page restores the user's filter / sort / column order.
builder.Services.AddBlazOrbitDataCollectionLocalStorage();
// Hotkeys: per-component registrations (e.g. Ctrl+K for the DocSearch trigger) live
// inside the component itself so they auto-dispose with the page. Program.cs is the
// place to declare app-wide shortcuts that survive across pages.
builder.Services.AddBlazOrbitHotkeys();
builder.Services.AddBlazOrbitNotifications();

builder.Services.AddBlazOrbitLocalizationWasm(options =>
{
    options.SupportedCultures =
    [
        new CultureInfo("en-US"),
        new CultureInfo("es-ES")
    ];
    options.DefaultCulture = "en-US";

    // Route IStringLocalizer<T> for the docs site assembly to the docs translations sidecar.
    options.TranslationsAssemblies["BlazOrbit.Docs.Wasm"] = "BlazOrbit.Docs.Translations";
});

builder.Services.AddBOBFluentValidation<Program>();
builder.Services.AddBOBFluentValidation();

builder.Services.AddScoped<DocSearchService>();

WebAssemblyHost host = builder.Build();

await host.UseBlazOrbitLocalizationWasm();

await host.RunAsync();
