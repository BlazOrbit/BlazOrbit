namespace BlazOrbit.Components.Layout;

/// <summary>
/// Themed replacement for the native <c>window.confirm()</c> prompt. Renders a
/// <c>BOBConfirmDialog</c> via <see cref="BlazOrbit.Components.Layout.Services.IModalService"/>
/// and returns whether the user accepted.
/// </summary>
/// <remarks>
/// Auto-registered by <c>AddBlazOrbit()</c>. Requires <c>&lt;BOBModalHost /&gt;</c> mounted
/// somewhere in the app shell — the same prerequisite as the rest of the modal stack.
/// </remarks>
public interface IConfirmService
{
    /// <summary>
    /// Shows a confirm dialog and resolves with <see langword="true"/> when the user accepts,
    /// <see langword="false"/> when they cancel, dismiss, or the optional timeout elapses.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Body message rendered below the title.</param>
    /// <param name="severity">Visual tone. <see cref="ConfirmSeverity.Danger"/> styles the accept button as destructive.</param>
    /// <param name="yesLabel">Label for the accept button.</param>
    /// <param name="noLabel">Label for the cancel button.</param>
    /// <param name="timeout">Optional auto-cancel deadline. <see langword="null"/> = no timeout.</param>
    Task<bool> AskAsync(
        string title,
        string message,
        ConfirmSeverity severity = ConfirmSeverity.Info,
        string yesLabel = "Yes",
        string noLabel = "Cancel",
        TimeSpan? timeout = null);
}