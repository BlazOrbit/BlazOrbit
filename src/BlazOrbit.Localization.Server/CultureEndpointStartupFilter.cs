using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Localization.Server;

public class CultureEndpointStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            ServerLocalizationSettings settings = app.ApplicationServices.GetRequiredService<ServerLocalizationSettings>();

            app.UseRequestLocalization(o =>
            {
                o.SupportedCultures = settings.SupportedCultures;
                o.SupportedUICultures = settings.SupportedCultures;
                o.DefaultRequestCulture = new RequestCulture(settings.DefaultCulture);
                o.ApplyCurrentCultureToResponseHeaders = true;
                o.RequestCultureProviders = new IRequestCultureProvider[]
                {
                    new CookieRequestCultureProvider { CookieName = settings.CultureCookieName }
                };
            });

            app.Use(async (context, nextMiddleware) =>
            {
                if (context.Request.Path.Equals(settings.CultureEndpointPath, StringComparison.OrdinalIgnoreCase))
                {
                    string? culture = context.Request.Query["culture"];
                    string redirectUri = context.Request.Query["redirectUri"].ToString();

                    // Reject absolute / scheme-relative redirects to avoid
                    // open-redirect: only same-origin paths are allowed.
                    if (string.IsNullOrEmpty(redirectUri)
                        || !Uri.TryCreate(redirectUri, UriKind.Relative, out _)
                        || redirectUri.StartsWith("//", StringComparison.Ordinal)
                        || redirectUri.StartsWith("/\\", StringComparison.Ordinal))
                    {
                        redirectUri = "/";
                    }

                    if (!string.IsNullOrEmpty(culture))
                    {
                        context.Response.Cookies.Append(
                            settings.CultureCookieName,
                            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture)),
                            new CookieOptions
                            {
                                Expires = DateTimeOffset.UtcNow.AddYears(1),
                                IsEssential = true,
                                SameSite = SameSiteMode.Lax,
                                HttpOnly = true
                            });
                    }

                    context.Response.Redirect(redirectUri);
                    return;
                }

                await nextMiddleware();
            });

            next(app);
        };
    }
}