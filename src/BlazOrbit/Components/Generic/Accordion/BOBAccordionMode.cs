namespace BlazOrbit.Components;

/// <summary>
/// Controls how many items can be expanded simultaneously inside a <see cref="BOBAccordion"/>.
/// </summary>
public enum BOBAccordionMode
{
    /// <summary>
    /// Multiple items can be expanded at the same time. Default.
    /// </summary>
    Multiple = 0,

    /// <summary>
    /// At most one item is expanded at a time. The user can collapse the active item, leaving none open.
    /// </summary>
    Single = 1,

    /// <summary>
    /// Exactly one item is expanded at all times. The user cannot collapse the active item by clicking
    /// it; collapse only happens implicitly when another item is expanded. The first registered item
    /// is auto-expanded.
    /// </summary>
    SingleStrict = 2,
}
