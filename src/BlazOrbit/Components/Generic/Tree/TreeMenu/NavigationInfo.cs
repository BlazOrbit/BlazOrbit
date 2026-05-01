using Microsoft.AspNetCore.Components.Routing;

namespace BlazOrbit.Components;

/// <summary>Navigation metadata for a tree-menu node — href, match strategy, and link target.</summary>
public sealed class NavigationInfo
{
    private static readonly HashSet<string> _allowedSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "http", "https", "mailto", "tel"
    };

    /// <summary><see langword="true"/> when <see cref="Href"/> is set and uses a safe scheme.</summary>
    public bool HasNavigation => IsSafeHref(Href);
    /// <summary>Anchor href the node navigates to.</summary>
    public string? Href { get; init; }
    /// <summary>Active-route match strategy used when rendering as a <see cref="Microsoft.AspNetCore.Components.Routing.NavLink"/>.</summary>
    public NavLinkMatch Match { get; init; } = NavLinkMatch.Prefix;
    /// <summary>Optional anchor target attribute (e.g. <c>_blank</c>).</summary>
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
