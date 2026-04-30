namespace BlazOrbit.Components.Layout;

/// <summary>
/// Defines the position of media content within a card.
/// </summary>
public enum CardMediaPosition
{
    /// <summary>
    /// Media is positioned at the top of the card.
    /// </summary>
    Top,

    /// <summary>
    /// Media is positioned after the header.
    /// </summary>
    AfterHeader,

    /// <summary>
    /// Media is positioned before the actions.
    /// </summary>
    BeforeActions,

    /// <summary>
    /// Media is positioned at the bottom of the card.
    /// </summary>
    Bottom
}

/// <summary>
/// Defines the alignment of actions within a card.
/// </summary>
public enum CardActionsAlignment
{
    /// <summary>
    /// Actions are aligned to the start.
    /// </summary>
    Start,

    /// <summary>
    /// Actions are aligned to the center.
    /// </summary>
    Center,

    /// <summary>
    /// Actions are aligned to the end.
    /// </summary>
    End,

    /// <summary>
    /// Actions are distributed with space between.
    /// </summary>
    SpaceBetween
}
