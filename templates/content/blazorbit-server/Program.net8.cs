using BlazorApp.Components;
//#if (IncludeCharts)
using BlazOrbit.Charts.Services;
//#endif

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
