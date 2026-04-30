using System.Globalization;

namespace BlazOrbit.Localization;

/// <summary>
/// Default emoji flag lookup for the cultures most commonly used in BlazOrbit
/// localization. The <c>BOBCultureSelector</c> components in
/// <c>BlazOrbit.Localization.Server</c> / <c>.Wasm</c> call into this
/// when the consumer does not supply a custom <c>FlagResolver</c>.
/// </summary>
/// <remarks>
/// Emoji flags rely on the OS font set, so rendering varies (Windows is the
/// weakest); for SVG fidelity, supply a custom resolver via the component
/// parameter. The fallback when a culture is not mapped is <c>🌐</c>.
/// </remarks>
public static class CultureFlags
{
    /// <summary>
    /// Look up the flag for <paramref name="cultureName"/> (e.g. <c>en-US</c>).
    /// Returns <see langword="false"/> when the culture is not mapped, in
    /// which case <paramref name="flag"/> is set to the default globe glyph.
    /// </summary>
    public static bool TryGetFlag(string? cultureName, out string flag)
    {
        flag = cultureName?.ToUpperInvariant() switch
        {
            "EN-US" => "🇺🇸",
            "ES-ES" => "🇪🇸",
            "FR-FR" => "🇫🇷",
            "DE-DE" => "🇩🇪",
            "IT-IT" => "🇮🇹",
            "PT-BR" => "🇧🇷",
            "PT-PT" => "🇵🇹",
            "ZH-CN" => "🇨🇳",
            "JA-JP" => "🇯🇵",
            "KO-KR" => "🇰🇷",
            "RU-RU" => "🇷🇺",
            "AR-SA" => "🇸🇦",
            "HI-IN" => "🇮🇳",
            "NL-NL" => "🇳🇱",
            "SV-SE" => "🇸🇪",
            "NO-NO" => "🇳🇴",
            "DA-DK" => "🇩🇰",
            "FI-FI" => "🇫🇮",
            "PL-PL" => "🇵🇱",
            "TR-TR" => "🇹🇷",
            "CS-CZ" => "🇨🇿",
            "EL-GR" => "🇬🇷",
            "HE-IL" => "🇮🇱",
            "TH-TH" => "🇹🇭",
            "VI-VN" => "🇻🇳",
            "ID-ID" => "🇮🇩",
            "MS-MY" => "🇲🇾",
            "UK-UA" => "🇺🇦",
            _ => null!
        };

        if (flag is null)
        {
            flag = "🌐";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Convenience overload: returns the flag, falling back to <c>🌐</c>.
    /// </summary>
    public static string GetFlag(string? cultureName)
    {
        TryGetFlag(cultureName, out string flag);
        return flag;
    }

    /// <summary>
    /// <see cref="CultureInfo"/> overload, useful when the consumer's
    /// <c>FlagResolver</c> wants to delegate back to the default lookup.
    /// </summary>
    public static string GetFlag(CultureInfo culture) => GetFlag(culture?.Name);
}
