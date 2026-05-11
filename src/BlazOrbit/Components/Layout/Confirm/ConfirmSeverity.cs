namespace BlazOrbit.Components.Layout;

/// <summary>Severity tier shown by <c>BOBConfirmDialog</c> via <see cref="IConfirmService"/>.</summary>
public enum ConfirmSeverity
{
    /// <summary>Default neutral confirmation. Highlight palette colour, info icon.</summary>
    Info = 0,
    /// <summary>Cautionary confirmation. Warning palette colour, warning icon.</summary>
    Warning = 1,
    /// <summary>Destructive confirmation. Error palette colour, danger icon, primary button rendered as destructive.</summary>
    Danger = 2,
}
