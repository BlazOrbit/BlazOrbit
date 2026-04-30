namespace BlazOrbit.Components.Utils.Patterns.DateTimePattern;

internal sealed class DateComponent
{
    public string DefaultValue { get; set; } = string.Empty;
    public int MaxDigits { get; set; }
    public int MinDigits { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public string? SeparatorValue { get; set; }
    public DateComponentType Type { get; set; }
}