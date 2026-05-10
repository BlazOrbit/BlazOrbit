namespace BlazOrbit.Components.Forms;

[Flags]
public enum PasswordStrengthFlags
{
    None = 0,
    HasUpper = 1 << 0,
    HasLower = 1 << 1,
    HasDigit = 1 << 2,
    HasSymbol = 1 << 3,
    MeetsLength = 1 << 4,
}
