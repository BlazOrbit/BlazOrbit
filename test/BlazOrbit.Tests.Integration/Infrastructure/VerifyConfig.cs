using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace BlazOrbit.Tests.Integration.Infrastructure;

public static class VerifyConfig
{
    private static readonly Regex ElementReferenceRegex =
        new(@"blazor:elementReference=""[a-f0-9]{8}(-[a-f0-9]{4}){3}-[a-f0-9]{12}""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex OnClickRegex =
        new(@"blazor:(onclick|onchange|oninput|onfocus|onblur|onsubmit|onkeydown|onkeyup)=""\d+""",
            RegexOptions.Compiled);

    private static readonly Regex BuiGeneratedIdRegex =
        new(@"bob-(input|helper|label|checkbox|radio|switch|number|textarea|input-color|datetime|input-number-slider|input-range-slider)-[a-f0-9]{32}",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PatternIdRegex =
        new(@"data-bob-pattern-id=""pattern_[a-f0-9]{32}""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DropdownIdRegex =
        new(@"bob-dropdown-[a-f0-9]{32}",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DialogTitleIdRegex =
        new(@"dialog-title-[a-f0-9]{32}",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CssIsolationScopeRegex =
        new(@"b-[a-z0-9]{10}",
            RegexOptions.Compiled);

    [ModuleInitializer]
    public static void Init()
    {
        CultureInfo enUs = new("en-US");
        CultureInfo.DefaultThreadCurrentCulture = enUs;
        CultureInfo.DefaultThreadCurrentUICulture = enUs;
        CultureInfo.CurrentCulture = enUs;
        CultureInfo.CurrentUICulture = enUs;

        VerifierSettings.DontScrubGuids();
        VerifierSettings.DontScrubDateTimes();

        VerifierSettings.AddScrubber(sb =>
        {
            string text = sb.ToString();

            text = ElementReferenceRegex.Replace(
                text,
                @"blazor:elementReference=""<GUID>""");

            text = OnClickRegex.Replace(
                text,
                @"blazor:$1=""<EVENT>""");

            text = BuiGeneratedIdRegex.Replace(
                text,
                @"bob-$1-<ID>");

            text = PatternIdRegex.Replace(
                text,
                @"data-bob-pattern-id=""<PATTERN_ID>""");

            text = DropdownIdRegex.Replace(
                text,
                @"bob-dropdown-<ID>");

            text = DialogTitleIdRegex.Replace(
                text,
                @"dialog-title-<ID>");

            text = CssIsolationScopeRegex.Replace(
                text,
                @"b-<SCOPE>");

            sb.Clear();
            sb.Append(text);
        });
    }
}