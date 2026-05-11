namespace BlazOrbit.Components;

/// <summary>
/// Single breadcrumb entry consumed by <c>BOBBreadcrumbs</c>
/// (<c>BlazOrbit.Components.Navigation</c>).
/// </summary>
/// <param name="Text">Display text for the entry.</param>
/// <param name="Href">
/// Navigation target. Pass <see langword="null"/> for the current page (rendered as
/// non-link text and announced via <c>aria-current="page"</c>).
/// </param>
/// <param name="Icon">
/// Optional leading icon. Useful for the root entry (e.g. a home glyph).
/// </param>
public readonly record struct BreadcrumbItem(string Text, string? Href = null, IconKey? Icon = null)
{
    /// <summary>
    /// True when this entry has no <see cref="Href"/> and is therefore rendered as the
    /// current page.
    /// </summary>
    public bool IsCurrent => string.IsNullOrEmpty(Href);
}
