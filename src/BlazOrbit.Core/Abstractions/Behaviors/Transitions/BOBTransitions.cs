using System.Globalization;

namespace BlazOrbit.Components;

public enum TransitionTrigger
{
    Hover,
    Focus,
    Active
}

public class TransitionEntry
{
    public string CssProperty { get; init; } = default!;
    public string Value { get; init; } = default!;
    public TimeSpan? Duration { get; init; }
    public string? Easing { get; init; }
    public TimeSpan? Delay { get; init; }
}

public class BOBTransitions
{
    private readonly Dictionary<TransitionTrigger, List<TransitionEntry>> _entries = [];

    public bool HasTransitions => _entries.Count > 0;

    public Dictionary<string, string> GetCssVariables()
    {
        Dictionary<string, string> variables = [];

        foreach ((TransitionTrigger trigger, List<TransitionEntry> entries) in _entries)
        {
            string triggerName = trigger.ToString().ToLowerInvariant();

            foreach (TransitionEntry entry in entries)
            {
                variables[FeatureDefinitions.Tokens.Transitions.VariableFor(triggerName, entry.CssProperty)] = entry.Value;
            }
        }

        variables[FeatureDefinitions.Tokens.Transitions.Shorthand] = BuildTransitionShorthand();

        return variables;
    }

    public string GetDataAttributeValue()
    {
        return string.Join(" ",
            _entries.SelectMany(t =>
                t.Value.Select(e =>
                    $"{t.Key.ToString().ToLowerInvariant()}:{e.CssProperty}"
                )).Distinct());
    }

    public BOBTransitions MergeWith(BOBTransitions overrides)
    {
        BOBTransitions merged = new();

        foreach ((TransitionTrigger trigger, List<TransitionEntry> entries) in _entries)
        {
            foreach (TransitionEntry entry in entries)
            {
                merged.AddEntry(trigger, entry);
            }
        }

        foreach ((TransitionTrigger trigger, List<TransitionEntry> entries) in overrides._entries)
        {
            foreach (TransitionEntry entry in entries)
            {
                if (merged._entries.TryGetValue(trigger, out List<TransitionEntry>? existing))
                {
                    int index = existing.FindIndex(e => e.CssProperty == entry.CssProperty);
                    if (index >= 0)
                    {
                        existing[index] = entry;
                    }
                    else
                    {
                        existing.Add(entry);
                    }
                }
                else
                {
                    merged.AddEntry(trigger, entry);
                }
            }
        }

        return merged;
    }

    internal void AddEntry(TransitionTrigger trigger, TransitionEntry entry)
    {
        if (!_entries.TryGetValue(trigger, out List<TransitionEntry>? list))
        {
            list = [];
            _entries[trigger] = list;
        }

        list.Add(entry);
    }

    private string BuildTransitionShorthand()
    {
        Dictionary<string, TransitionEntry> byProperty = [];

        foreach ((TransitionTrigger _, List<TransitionEntry> entries) in _entries)
        {
            foreach (TransitionEntry entry in entries)
            {
                if (!byProperty.TryGetValue(entry.CssProperty, out TransitionEntry? existing)
                    || (entry.Duration ?? TimeSpan.Zero) > (existing.Duration ?? TimeSpan.Zero))
                {
                    byProperty[entry.CssProperty] = entry;
                }
            }
        }

        return string.Join(", ", byProperty.Values.Select(e =>
        {
            string duration = ((int)(e.Duration ?? TimeSpan.FromMilliseconds(200)).TotalMilliseconds)
                .ToString(CultureInfo.InvariantCulture) + "ms";
            string easing = e.Easing ?? "ease-in-out";
            string delay = e.Delay.HasValue
                ? " " + ((int)e.Delay.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture) + "ms"
                : "";
            return $"{e.CssProperty} {duration} {easing}{delay}";
        }));
    }
}