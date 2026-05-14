namespace BlazOrbit.Hotkeys;

/// <summary>Scope rule applied to a registered <c>BOBHotkeyService</c> entry.</summary>
public enum HotkeyScope
{
    /// <summary>Always active. Use for app-wide shortcuts (Ctrl+S, Ctrl+/).</summary>
    Global = 0,

    /// <summary>
    /// Active until the registration is disposed. Page or feature handlers fall here —
    /// the consumer disposes the registration in <c>IDisposable.Dispose</c> /
    /// <c>IAsyncDisposable.DisposeAsync</c> when the page navigates away.
    /// </summary>
    Page = 1
}