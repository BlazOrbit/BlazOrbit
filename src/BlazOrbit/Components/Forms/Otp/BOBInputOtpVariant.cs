namespace BlazOrbit.Components.Forms;

/// <summary>
/// Represents a variant definition for the <see cref="BOBInputOtp"/> component.
/// </summary>
public sealed class BOBInputOtpVariant : Variant
{
    /// <summary>
    /// The default OTP variant — boxed slots with bottom underline.
    /// </summary>
    public static readonly BOBInputOtpVariant Default = new("Default");

    /// <summary>
    /// The boxed OTP variant — fully boxed slots with rounded borders.
    /// </summary>
    public static readonly BOBInputOtpVariant Boxed = new("Boxed");

    /// <summary>
    /// The underlined OTP variant — only bottom border per slot.
    /// </summary>
    public static readonly BOBInputOtpVariant Underlined = new("Underlined");

    /// <summary>
    /// Initializes a new instance of the <see cref="BOBInputOtpVariant"/> class with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    public BOBInputOtpVariant(string name) : base(name)
    {
    }

    /// <summary>
    /// Creates a custom OTP variant with the specified name.
    /// </summary>
    /// <param name="name">The variant name.</param>
    /// <returns>A new <see cref="BOBInputOtpVariant"/> instance.</returns>
    public static BOBInputOtpVariant Custom(string name) => new(name);
}
