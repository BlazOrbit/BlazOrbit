namespace BlazOrbit.Components.Display;

/// <summary>Shape outline of <see cref="BOBAvatar"/>.</summary>
public enum BOBAvatarShape
{
    /// <summary>Fully circular (default).</summary>
    Circle = 0,
    /// <summary>Square with sharp corners.</summary>
    Square = 1,
    /// <summary>Square with rounded corners.</summary>
    Rounded = 2
}

/// <summary>Status dot rendered on <see cref="BOBAvatar"/>.</summary>
public enum BOBAvatarStatus
{
    /// <summary>No status dot.</summary>
    None = 0,
    /// <summary>Online (green).</summary>
    Online = 1,
    /// <summary>Offline (gray).</summary>
    Offline = 2,
    /// <summary>Busy (red).</summary>
    Busy = 3,
    /// <summary>Away (yellow).</summary>
    Away = 4
}
