namespace BlazOrbit.Localization;

/// <summary>
/// Configuration knobs for the BOBLocalize runtime. Passed to the registration extension via
/// the standard <see cref="System.Action{T}"/> pattern; immutable after build.
/// </summary>
public sealed class BobLocalizationOptions
{
    /// <summary>
    /// Default UI culture set on the application when no other source overrides it. Individual
    /// bundles can still declare their own source-literal culture via
    /// <see cref="BobLocalizationBundleAttribute.DefaultCulture"/>. <see langword="null"/> leaves
    /// the runtime's current default untouched.
    /// </summary>
    public string? DefaultCulture { get; set; }

    /// <summary>
    /// When <see langword="true"/> (default), the localizer adapter emits a
    /// <see cref="System.Diagnostics.DebuggerDisplayAttribute"/>-friendly diagnostic when a
    /// translation falls through to the source literal. Only inspectable from debug builds.
    /// </summary>
    public bool EnableFallbackDiagnostics { get; set; } = true;
}