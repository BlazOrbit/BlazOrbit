namespace BlazOrbit.Components.Forms;

/// <summary>Per-rule outcome of password strength evaluation.</summary>
[Flags]
public enum PasswordStrengthFlags
{
    /// <summary>No rule satisfied.</summary>
    None = 0,

    /// <summary>Contains at least one uppercase letter.</summary>
    HasUpper = 1 << 0,

    /// <summary>Contains at least one lowercase letter.</summary>
    HasLower = 1 << 1,

    /// <summary>Contains at least one digit.</summary>
    HasDigit = 1 << 2,

    /// <summary>Contains at least one symbol.</summary>
    HasSymbol = 1 << 3,

    /// <summary>Meets the configured minimum length.</summary>
    MeetsLength = 1 << 4
}
