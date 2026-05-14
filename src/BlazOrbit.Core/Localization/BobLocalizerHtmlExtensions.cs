using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazOrbit.Localization;

/// <summary>
/// HTML-aware helpers over <see cref="IStringLocalizer{T}"/>. Use when a translation contains
/// markup (e.g. <c>&lt;strong&gt;</c>, <c>&lt;br /&gt;</c>) and the value must reach the DOM
/// without Blazor's default HTML escaping.
/// </summary>
/// <remarks>
/// <para>
/// <b>Threat model.</b> The translation <em>template</em> ships in source-controlled
/// <c>.tn</c> files — audited the same way as inline Razor markup. Format <em>arguments</em>
/// passed to <see cref="Html{T}"/> are treated as untrusted and HTML-encoded before
/// substitution so a runtime value can never inject markup. The resulting
/// <see cref="MarkupString"/> is therefore safe whenever the consumer keeps:
/// </para>
/// <list type="bullet">
///   <item><description>translations under version control (no end-user editing),</description></item>
///   <item><description>dynamic values passed only as format arguments — not concatenated into the key string.</description></item>
/// </list>
/// <para>
/// If a custom provider sources translations from an unvetted store (CMS, user-editable DB),
/// run an HTML sanitiser before wrapping the value in <see cref="MarkupString"/>; the
/// encoder here only neutralises the arguments, not the template.
/// </para>
/// </remarks>
public static class BobLocalizerHtmlExtensions
{
    /// <summary>
    /// Resolves <paramref name="name"/> as HTML markup. Each entry of <paramref name="args"/>
    /// is HTML-encoded via <see cref="HtmlEncoder.Default"/> before being substituted into the
    /// template, so untrusted runtime values cannot inject markup. The template itself is
    /// rendered verbatim — author it with safe markup only.
    /// </summary>
    /// <typeparam name="T">Bundle marker type.</typeparam>
    /// <param name="localizer">The <see cref="IStringLocalizer{T}"/> to resolve through.</param>
    /// <param name="name">Translation key.</param>
    /// <param name="args">Optional format arguments. Each is HTML-encoded before substitution.</param>
    /// <returns>A <see cref="MarkupString"/> ready to bind with <c>@</c> in a Razor file.</returns>
    public static MarkupString Html<T>(
        this IStringLocalizer<T> localizer,
        string name,
        params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(name);

        string template = localizer[name].Value;
        if (args is null || args.Length == 0)
        {
            return new MarkupString(template);
        }

        string[] encoded = new string[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            encoded[i] = HtmlEncoder.Default.Encode(args[i]?.ToString() ?? string.Empty);
        }

        return new MarkupString(string.Format(template, encoded));
    }
}