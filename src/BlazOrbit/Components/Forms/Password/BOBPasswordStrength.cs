namespace BlazOrbit.Components.Forms;

/// <summary>
/// Coarse-grained password strength score emitted by <see cref="BOBInputPassword"/>.
/// Values are ordered: higher ordinal means stronger.
/// </summary>
public enum BOBPasswordStrength
{
    /// <summary>No password entered, or shorter than 1 character.</summary>
    None = 0,

    /// <summary>Fails the minimum length or any required character class.</summary>
    Weak = 1,

    /// <summary>Meets minimum length but only one character class.</summary>
    Fair = 2,

    /// <summary>Meets minimum length and at least two character classes.</summary>
    Good = 3,

    /// <summary>Meets minimum length and all enabled character classes plus length ≥ 12.</summary>
    Strong = 4
}