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
        new(
            @"bob-(input|helper|label|checkbox|radio|switch|number|textarea|input-color|datetime|input-number-slider|input-range-slider|carousel|otp)-[a-f0-9]{32}",
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

    // Razor SDK CSS isolation scope: `b-` followed by exactly 10 [a-z0-9]
    // chars, attached as an attribute on HTML elements (`<div b-xyz1234abc>`)
    // or as an attribute selector in scoped CSS (`[b-xyz1234abc]`). The
    // word-boundary anchors stop the regex from devouring ordinary tokens of
    // the form `bob-comfortabl` / `data-bob-transition` / `b-component`,
    // which are 10-char identifiers but never appear preceded/followed by a
    // word boundary together with a leading `b-` segment in real DOM output.
    private static readonly Regex CssIsolationScopeRegex =
        new(@"\bb-[a-z0-9]{10}\b",
            RegexOptions.Compiled);

    // BOBDatePicker / BOBTimePicker / BOBInputDateRange render the calendar grid with
    // `data-bob-active="true"` on the cell that matches DateTime.Today. That makes any
    // snapshot containing a date picker drift on the day boundary. Strip the attribute
    // when it lands on a `bob-picker__cell` element so the snapshot becomes date-stable.
    // The `\s+` keeps the leading space the attribute sits on consistent across days.
    private static readonly Regex PickerActiveCellRegex =
        new(@"(?<=class=""_bob-btn bob-picker__cell"") data-bob-active=""true""",
            RegexOptions.Compiled);

    private static readonly Regex PickerActiveCellRegexAlt =
        new(@" data-bob-active=""true""(?= class=""_bob-btn bob-picker__cell"")",
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

            text = PickerActiveCellRegex.Replace(text, string.Empty);
            text = PickerActiveCellRegexAlt.Replace(text, string.Empty);

            sb.Clear();
            sb.Append(text);
        });
    }
}
