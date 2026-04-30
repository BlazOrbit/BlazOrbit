namespace BlazOrbit.Components.Forms;

/// <summary>
/// Defines how the textarea can be resized by the user.
/// </summary>
public enum TextAreaResize
{
    /// <summary>
    /// User cannot resize the textarea.
    /// </summary>
    None,

    /// <summary>
    /// User can resize vertically only (default).
    /// </summary>
    Vertical,

    /// <summary>
    /// User can resize horizontally only.
    /// </summary>
    Horizontal,

    /// <summary>
    /// User can resize in both directions.
    /// </summary>
    Both
}