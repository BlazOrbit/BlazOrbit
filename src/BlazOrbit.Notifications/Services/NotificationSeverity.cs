namespace BlazOrbit.Notifications;

/// <summary>Severity tier for <see cref="BOBNotification"/> entries.</summary>
public enum NotificationSeverity
{
    /// <summary>Neutral information.</summary>
    Info = 0,

    /// <summary>Successful operation completed.</summary>
    Success = 1,

    /// <summary>Cautionary state worth surfacing.</summary>
    Warning = 2,

    /// <summary>Error or failure event.</summary>
    Error = 3
}