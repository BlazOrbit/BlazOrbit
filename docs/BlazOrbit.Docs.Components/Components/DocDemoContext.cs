using System.Reflection;

namespace BlazOrbit.Docs.Components;

public sealed class DocDemoContext
{
    private static readonly IReadOnlyDictionary<string, string> _empty
        = new Dictionary<string, string>(StringComparer.Ordinal);

    private readonly IReadOnlyDictionary<string, string> _codes;

    public DocDemoContext(object page)
    {
        ArgumentNullException.ThrowIfNull(page);
        _codes = Load(page);
    }

    public string? Get(string key) => _codes.TryGetValue(key, out string? v) ? v : null;

    private static IReadOnlyDictionary<string, string> Load(object page)
    {
        MethodInfo? m = page.GetType().GetMethod(
            "__GetDocDemoCodes",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        return m?.Invoke(page, null) as IReadOnlyDictionary<string, string> ?? _empty;
    }
}
