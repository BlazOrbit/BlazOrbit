using BlazorApp;
//#if (IncludeCharts)
using BlazOrbit.Charts.Services;
//#endif
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

await builder.Build().RunAsync();
