namespace BlazOrbit.Components.Utils.Patterns.DateTimePattern;

internal static class DatePatternParser
{
    private static readonly Dictionary<string, DateComponentType> ComponentMap = new()
    {
        { "d", DateComponentType.Day }, { "dd", DateComponentType.Day },
        { "M", DateComponentType.Month }, { "MM", DateComponentType.Month },
        { "yy", DateComponentType.Year }, { "yyyy", DateComponentType.Year },
        { "h", DateComponentType.Hour12 }, { "hh", DateComponentType.Hour12 },
        { "H", DateComponentType.Hour24 }, { "HH", DateComponentType.Hour24 },
        { "m", DateComponentType.Minute }, { "mm", DateComponentType.Minute },
        { "s", DateComponentType.Second }, { "ss", DateComponentType.Second },
        { "t", DateComponentType.AmPm }, { "tt", DateComponentType.AmPm }
    };

    public static ParsedDatePattern Parse(string format)
    {
        ParsedDatePattern parsed = new() { OriginalFormat = format };

        int i = 0;
        while (i < format.Length)
        {
            char c = format[i];

            if ("dMyhHmst".Contains(c))
            {
                int count = 1;
                while (i + count < format.Length && format[i + count] == c)
                {
                    count++;
                }

                string specifier = new(c, count);
                DateComponent component = CreateComponent(specifier);
                parsed.Components.Add(component);
                i += count;
            }
            else
            {
                int start = i;
                while (i < format.Length && !"dMyhHmst".Contains(format[i]))
                {
                    i++;
                }

                string sep = format[start..i];
                parsed.Components.Add(new DateComponent
                {
                    Type = DateComponentType.Separator,
                    SeparatorValue = sep,
                    DefaultValue = sep
                });
            }
        }

        return parsed;
    }

    private static DateComponent CreateComponent(string spec)
    {
        DateComponentType type = ComponentMap.TryGetValue(spec, out DateComponentType t)
            ? t
            : ComponentMap[spec[0].ToString()];

        int minDigits = spec.Length;
        int maxDigits = type switch
        {
            DateComponentType.Day or DateComponentType.Month or
            DateComponentType.Hour12 or DateComponentType.Hour24 or
            DateComponentType.Minute or DateComponentType.Second => 2,
            DateComponentType.Year => spec.Length == 2 ? 2 : 4,
            DateComponentType.AmPm => spec.Length == 1 ? 1 : 2,
            _ => spec.Length
        };

        DateComponent component = new()
        {
            Type = type,
            MinDigits = minDigits,
            MaxDigits = maxDigits,
            DefaultValue = GetDefaultValue(type, maxDigits)
        };

        return component;
    }

    private static string GetDefaultValue(DateComponentType type, int digits)
    {
        return type switch
        {
            DateComponentType.Day => "dd",
            DateComponentType.Month => "MM",
            DateComponentType.Year => digits == 2 ? "yy" : "yyyy",
            DateComponentType.Hour12 => "hh",
            DateComponentType.Hour24 => "HH",
            DateComponentType.Minute => "mm",
            DateComponentType.Second => "ss",
            DateComponentType.AmPm => digits == 1 ? "A" : "AM",
            _ => ""
        };
    }
}