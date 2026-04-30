namespace BlazOrbit.Components.Layout;

/// <summary>
/// Options for configuring a dialog modal.
/// </summary>
public sealed class DialogOptions : ModalOptionsBase
{
    /// <summary>
    /// When <see langword="true"/>, the dialog occupies the full screen.
    /// </summary>
    public bool FullScreen { get; set; }

    /// <summary>
    /// The maximum height of the dialog.
    /// </summary>
    public string? MaxHeight { get; set; } = "90vh";

    /// <summary>
    /// The maximum width of the dialog.
    /// </summary>
    public string? MaxWidth { get; set; } = "90vw";

    /// <summary>
    /// The minimum height of the dialog.
    /// </summary>
    public string? MinHeight { get; set; }

    /// <summary>
    /// The minimum width of the dialog.
    /// </summary>
    public string? MinWidth { get; set; } = "300px";

    /// <summary>
    /// The title displayed in the dialog.
    /// </summary>
    public string? Title { get; set; }
}

/// <summary>
/// Options for configuring a drawer modal.
/// </summary>
public sealed class DrawerOptions : ModalOptionsBase
{
    /// <summary>
    /// The position of the drawer on the screen.
    /// </summary>
    public DrawerPosition Position { get; set; } = DrawerPosition.Right;

    /// <summary>
    /// The size of the drawer.
    /// </summary>
    public string Size { get; set; } = "300px";
}

/// <summary>
/// Base class for modal options.
/// </summary>
public abstract class ModalOptionsBase
{
    /// <summary>
    /// When <see langword="true"/>, the modal can be closed by the user.
    /// </summary>
    public bool Closable { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, the modal closes when the Escape key is pressed.
    /// </summary>
    public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, the modal closes when the overlay is clicked.
    /// </summary>
    public bool CloseOnOverlayClick { get; set; } = true;

    /// <summary>
    /// Additional CSS classes to apply to the modal.
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Material Design elevation level (0–24) applied to the modal surface. Sets a derived
    /// shadow and lifts the surface in dark mode via a tint overlay. <see langword="null"/> (default)
    /// keeps the component-default shadow defined in CSS.
    /// </summary>
    public int? Elevation { get; set; }
}

/// <summary>
/// Defines the position of a drawer on the screen.
/// </summary>
public enum DrawerPosition
{
    /// <summary>
    /// Drawer opens from the left side.
    /// </summary>
    Left,

    /// <summary>
    /// Drawer opens from the right side.
    /// </summary>
    Right,

    /// <summary>
    /// Drawer opens from the top.
    /// </summary>
    Top,

    /// <summary>
    /// Drawer opens from the bottom.
    /// </summary>
    Bottom
}
