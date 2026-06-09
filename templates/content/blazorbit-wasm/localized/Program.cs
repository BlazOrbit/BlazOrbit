using System.Globalization;
using BlazorApp;
using BlazOrbit;
//#if (IncludeCharts)
using BlazOrbit.Charts.Services;
//#endif
using BlazOrbit.Localization.Wasm;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazOrbit();
//#if (IncludeCharts)
builder.Services.AddBlazOrbitCharts();
//#endif
//#if (UseNotificationsCenter)
builder.Services.AddBlazOrbitNotifications();
//#endif
//#if (UseHotKeys)
builder.Services.AddBlazOrbitHotkeys();
//#endif
builder.Services.AddBlazOrbitLocalizationWasm(opts =>
{
    opts.SupportedCultures = [
        new CultureInfo("en-US"),
        new CultureInfo("es-ES"),
        new CultureInfo("fr-FR")
    ];
    opts.DefaultCulture = "en-US";
});

var host = builder.Build();
await host.UseBlazOrbitLocalizationWasm();
await host.RunAsync();
