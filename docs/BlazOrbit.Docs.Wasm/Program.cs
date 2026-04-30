using BlazOrbit.Docs.Wasm;
using BlazOrbit.Docs.Wasm.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazOrbit();
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
