namespace BlazOrbit.Components.Forms;

/// <summary>
/// Defines the placement of the dropdown menu relative to its trigger.
/// </summary>
public enum DropdownPlacement
{
    /// <summary>
    /// Automatically determines the best placement.
    /// </summary>
    Auto,

    /// <summary>
    /// Positions the dropdown below the trigger.
    /// </summary>
    Bottom,

    /// <summary>
    /// Positions the dropdown above the trigger.
    /// </summary>
    Top,

    /// <summary>
    /// Positions the dropdown below and aligned to the start of the trigger.
    /// </summary>
    BottomStart,

    /// <summary>
    /// Positions the dropdown below and aligned to the end of the trigger.
    /// </summary>
    BottomEnd,

    /// <summary>
    /// Positions the dropdown above and aligned to the start of the trigger.
    /// </summary>
    TopStart,

    /// <summary>
    /// Positions the dropdown above and aligned to the end of the trigger.
    /// </summary>
    TopEnd
}
