using System.Globalization;

namespace BlazOrbit.Components.Utils.Patterns.DateTimePattern;

internal static class DateComponentValidator
{
    public static bool ValidateComponent(
        DateComponentType type,
        string value,
        ParsedDatePattern pattern,
        Dictionary<DateComponentType, string>? context)
    {
        return !string.IsNullOrWhiteSpace(value) && type switch
        {
            DateComponentType.Day => ValidateDay(value, context),
            DateComponentType.Month => ValidateMonth(value),
            DateComponentType.Year => ValidateYear(value),
            DateComponentType.Hour12 => ValidateHour12(value),
            DateComponentType.Hour24 => ValidateHour24(value),
            DateComponentType.Minute => ValidateMinute(value),
            DateComponentType.Second => ValidateSecond(value),
            DateComponentType.AmPm => ValidateAmPm(value),
            _ => true
        };
    }

    private static int ConvertTwoDigitYear(int year)
    {
        try
        {
            return CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(year);
        }
        catch
        {
            return 2000 + year;
        }
    }

    private static bool ValidateAmPm(string value)
    {
        string upper = value.ToUpperInvariant();
        return upper is "A" or "P" or "AM" or "PM";
    }

    private static bool ValidateDay(string value, Dictionary<DateComponentType, string>? ctx)
    {
        if (!int.TryParse(value, out int day) || day < 1 || day > 31)
        {
            return false;
        }

        if (ctx == null)
        {
            return true;
        }

        if (!ctx.TryGetValue(DateComponentType.Month, out string? monthStr) ||
            !int.TryParse(monthStr, out int month) ||
            month < 1 || month > 12)
        {
            return true;
        }

        int year = DateTime.Now.Year;

        if (ctx.TryGetValue(DateComponentType.Year, out string? yearStr) &&
            int.TryParse(yearStr, out int parsedYear))
        {
            year = parsedYear < 100
                ? ConvertTwoDigitYear(parsedYear)
                : parsedYear;
        }

        try
        {
            int maxDays = DateTime.DaysInMonth(year, month);
            return day <= maxDays;
        }
        catch
        {
            return false;
        }
    }

    private static bool ValidateHour12(string value)
        => int.TryParse(value, out int hour) && hour >= 1 && hour <= 12;

    private static bool ValidateHour24(string value)
        => int.TryParse(value, out int hour) && hour >= 0 && hour <= 23;

    private static bool ValidateMinute(string value)
        => int.TryParse(value, out int minute) && minute >= 0 && minute <= 59;

    private static bool ValidateMonth(string value)
                    => int.TryParse(value, out int month) && month >= 1 && month <= 12;

    private static bool ValidateSecond(string value)
        => int.TryParse(value, out int second) && second >= 0 && second <= 59;

    private static bool ValidateYear(string value) => int.TryParse(value, out int year) && (value.Length <= 2 || year is >= 1900 and <= 2100);
}