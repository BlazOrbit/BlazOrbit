using System.Text.RegularExpressions;

namespace BlazOrbit.Components;

/// <summary>
/// Minimal sanitizer for SVG fragments injected via <see cref="BOBSvgIcon.Icon"/>.
/// Strips script-like tags and event-handler attributes. Not a full HTML sanitizer:
/// callers are expected to supply trusted SVG markup (e.g. from BOBIcons).
/// </summary>
internal static partial class SvgMarkupSanitizer
{
    [GeneratedRegex(@"<\s*(script|iframe|object|embed|foreignObject)\b[^>]*>[\s\S]*?<\s*/\s*\1\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex DisallowedBlockTagRegex();

    [GeneratedRegex(@"<\s*(script|iframe|object|embed|foreignObject)\b[^>]*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex DisallowedSelfClosingRegex();

    [GeneratedRegex(@"\s+on[a-z]+\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerAttributeRegex();

    [GeneratedRegex(@"(href|xlink:href)\s*=\s*(?:""\s*javascript:[^""]*""|'\s*javascript:[^']*')", RegexOptions.IgnoreCase)]
    private static partial Regex JavaScriptUriRegex();

    public static string Sanitize(string? markup)
    {
        if (string.IsNullOrEmpty(markup))
        {
            return string.Empty;
        }

        string result = DisallowedBlockTagRegex().Replace(markup, string.Empty);
        result = DisallowedSelfClosingRegex().Replace(result, string.Empty);
        result = EventHandlerAttributeRegex().Replace(result, string.Empty);
        result = JavaScriptUriRegex().Replace(result, string.Empty);
        return result;
    }
}
