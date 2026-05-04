namespace BlazOrbit.Components;

/// <summary>
/// Payload of <see cref="BOBAccordion.OnExpandedChanged"/> describing a single item toggle.
/// </summary>
/// <param name="ItemId">Identifier of the item whose state changed.</param>
/// <param name="IsExpanded">New expanded state. <see langword="true"/> when the item just opened.</param>
public readonly record struct BOBAccordionItemToggle(string ItemId, bool IsExpanded);
