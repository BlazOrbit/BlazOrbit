using Microsoft.AspNetCore.Components.Routing;

namespace BlazOrbit.Components;

public sealed class NavigationInfo
{
    private static readonly HashSet<string> _allowedSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "http", "https", "mailto", "tel"
    };

    public bool HasNavigation => IsSafeHref(Href);
    public string? Href { get; init; }
    public NavLinkMatch Match { get; init; } = NavLinkMatch.Prefix;
    public string? Target { get; init; }

    // Untrusted sources (CMS content, API payloads) may set Href to `javascript:alert(1)`
    // or `data:text/html,...` — both execute on click. The component layer treats
    // HasNavigation as the render gate, so rejecting unsafe schemes here silently
    // degrades the node to the non-link branch instead of emitting the dangerous <a>.
    internal static bool IsSafeHref(string? href)
    {
        if (string.IsNullOrEmpty(href))
        {
            return false;
        }

        char first = href[0];
        if (first is '#' or '?' or '/')
        {
            return true;
        }

        int colon = href.IndexOf(':');
        if (colon < 0)
        {
            return true;
        }

        int slash = href.IndexOf('/');
        if (slash >= 0 && slash < colon)
        {
            return true;
        }

        string scheme = href[..colon];
        return _allowedSchemes.Contains(scheme);
    }
}
