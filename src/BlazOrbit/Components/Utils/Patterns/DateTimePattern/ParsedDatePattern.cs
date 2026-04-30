namespace BlazOrbit.Components.Utils.Patterns.DateTimePattern;

internal sealed class ParsedDatePattern
{
    public List<DateComponent> Components { get; set; } = [];
    public string OriginalFormat { get; set; } = string.Empty;
    public string RegexPattern { get; set; } = string.Empty;
}