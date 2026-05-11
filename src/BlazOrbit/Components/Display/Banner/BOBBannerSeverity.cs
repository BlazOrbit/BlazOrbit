namespace BlazOrbit.Components.Display;

/// <summary>
/// Tone of a <see cref="BOBBanner"/>. Drives the icon, palette accent and screen-reader role.
/// </summary>
public enum BOBBannerSeverity
{
    /// <summary>Neutral informational message.</summary>
    Info = 0,
    /// <summary>Confirmation of a successful action.</summary>
    Success = 1,
    /// <summary>Non-blocking warning the user should review.</summary>
    Warning = 2,
    /// <summary>Blocking error the user must address.</summary>
    Error = 3
}
