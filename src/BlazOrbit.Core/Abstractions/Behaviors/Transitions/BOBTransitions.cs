using System.Globalization;

namespace BlazOrbit.Components;

/// <summary>Interaction states that trigger a transition entry.</summary>
public enum TransitionTrigger
{
    /// <summary>Pointer hover (<c>:hover</c>).</summary>
    Hover,

    /// <summary>Keyboard or programmatic focus (<c>:focus</c>).</summary>
    Focus,

    /// <summary>Active / pressed (<c>:active</c>).</summary>
    Active
}

/// <summary>Single CSS property entry within a transition set.</summary>
public class TransitionEntry
{
    /// <summary>Target CSS property name.</summary>
    public string CssProperty { get; init; } = default!;

    /// <summary>Target CSS value for the property under the trigger.</summary>
    public string Value { get; init; } = default!;

    /// <summary>Optional transition duration.</summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>Optional CSS easing function.</summary>
    public string? Easing { get; init; }

    /// <summary>Optional delay before the transition starts.</summary>
    public TimeSpan? Delay { get; init; }
}

/// <summary>Aggregates transition entries keyed by <see cref="TransitionTrigger"/> and emits CSS variables / shorthand.</summary>
public class BOBTransitions
{
    private readonly Dictionary<TransitionTrigger, List<TransitionEntry>> _entries = [];

    /// <summary>True when at least one entry has been added.</summary>
    public bool HasTransitions => _entries.Count > 0;

    /// <summary>Emits the CSS custom-property map for the configured transitions plus the shorthand variable.</summary>
    public Dictionary<string, string> GetCssVariables()
    {
        Dictionary<string, string> variables = [];

        foreach ((TransitionTrigger trigger, List<TransitionEntry> entries) in _entries)
        {
            string triggerName = trigger.ToString().ToLowerInvariant();

            foreach (TransitionEntry entry in entries)
            {
                variables[FeatureDefinitions.Tokens.Transitions.VariableFor(triggerName, entry.CssProperty)] =
                    entry.Value;
            }
        }

        variables[FeatureDefinitions.Tokens.Transitions.Shorthand] = BuildTransitionShorthand();

        return variables;
    }

    /// <summary>Builds the space-separated <c>trigger:property</c> token list used in the data attribute.</summary>
    public string GetDataAttributeValue()
    {
        return string.Join(" ",
            _entries.SelectMany(t =>
                t.Value.Select(e =>
                    $"{t.Key.ToString().ToLowerInvariant()}:{e.CssProperty}"
                )).Distinct());
    }

    /// <summary>Returns a new <see cref="BOBTransitions"/> with this instance overlaid by <paramref name="overrides"/>.</summary>
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