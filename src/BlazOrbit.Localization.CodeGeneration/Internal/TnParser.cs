using System.Collections.Generic;
using System.Text;

namespace BlazOrbit.Localization.CodeGeneration.Internal;

/// <summary>
/// Netstandard2.0 port of the runtime <c>TnParser</c>. Returns plain dictionaries so the
/// source generator can emit C# code that constructs <c>FrozenDictionary</c> instances on
/// the consumer side. Keep this in sync with <c>BlazOrbit.Core/Localization/TnFormat/TnParser.cs</c>.
/// </summary>
internal static class TnParser
{
    public static TnParseResult Parse(string content, string defaultCulture = "")
    {
        Dictionary<string, Dictionary<ulong, string>> output =
            new(StringComparer.OrdinalIgnoreCase);
        List<TnDiagnostic> diagnostics = [];

        string currentCulture = defaultCulture ?? string.Empty;
        StringBuilder? pendingKey = null;
        StringBuilder? pendingValue = null;
        int pendingKeyLine = -1;

        int lineNumber = 0;
        foreach (string rawLine in EnumerateLines(content ?? string.Empty))
        {
            lineNumber++;
            string trimmed = rawLine.TrimStart();

            if (trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (trimmed.Length == 0)
            {
                Flush();
                continue;
            }

            if (trimmed[0] == '@')
            {
                Flush();
                string cultureName = trimmed.Substring(1).Trim();
                if (cultureName.Length == 0)
                {
                    diagnostics.Add(new TnDiagnostic(
                        TnDiagnosticSeverity.Error,
                        "Culture header is missing a name.",
                        lineNumber));
                    continue;
                }

                currentCulture = cultureName;
                continue;
            }

            if (trimmed[0] == '#')
            {
                string keyText = trimmed.Substring(1).TrimStart();
                if (pendingKey != null && pendingValue != null)
                {
                    Flush();
                }

                if (pendingKey == null)
                {
                    pendingKey = new StringBuilder(keyText);
                    pendingKeyLine = lineNumber;
                }
                else
                {
                    pendingKey.Append('\n').Append(keyText);
                }

                continue;
            }

            string contentLine = UnescapeLeader(trimmed);
            if (pendingKey == null)
            {
                diagnostics.Add(new TnDiagnostic(
                    TnDiagnosticSeverity.Error,
                    "Value text outside of a key block: \"" + Truncate(contentLine) + "\"",
                    lineNumber));
                continue;
            }

            if (pendingValue == null)
            {
                pendingValue = new StringBuilder();
            }

            if (pendingValue.Length > 0)
            {
                pendingValue.Append('\n');
            }

            pendingValue.Append(contentLine);
        }

        Flush();

        return new TnParseResult(output, diagnostics);

        void Flush()
        {
            if (pendingKey == null)
            {
                return;
            }

            string key = pendingKey.ToString();
            string value = pendingValue?.ToString() ?? string.Empty;
            ulong hash = LocalizationHash.Compute(key);

            if (!output.TryGetValue(currentCulture, out Dictionary<ulong, string>? byHash))
            {
                byHash = new Dictionary<ulong, string>();
                output[currentCulture] = byHash;
            }

            if (byHash.ContainsKey(hash))
            {
                diagnostics.Add(new TnDiagnostic(
                    TnDiagnosticSeverity.Warning,
                    "Duplicate key in culture '" + currentCulture + "': \"" + Truncate(key) +
                    "\". Previous value overwritten.",
                    pendingKeyLine));
            }

            byHash[hash] = value;

            pendingKey = null;
            pendingValue = null;
            pendingKeyLine = -1;
        }
    }

    private static IEnumerable<string> EnumerateLines(string content)
    {
        int start = 0;
        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] == '\n')
            {
                int len = i - start;
                if (len > 0 && content[i - 1] == '\r')
                {
                    len--;
                }

                yield return content.Substring(start, len);
                start = i + 1;
            }
        }

        if (start < content.Length)
        {
            yield return content.Substring(start);
        }
    }

    private static string UnescapeLeader(string line)
    {
        if (line.Length >= 2 && line[0] == '\\' && (line[1] == '#' || line[1] == '@' || line[1] == '/'))
        {
            return line.Substring(1);
        }

        return line;
    }

    private static string Truncate(string s) => s.Length <= 60 ? s : s.Substring(0, 57) + "...";
}

internal sealed class TnParseResult
{
    public TnParseResult(Dictionary<string, Dictionary<ulong, string>> translations,
        List<TnDiagnostic> diagnostics)
    {
        Translations = translations;
        Diagnostics = diagnostics;
    }

    public Dictionary<string, Dictionary<ulong, string>> Translations { get; }
    public List<TnDiagnostic> Diagnostics { get; }
}

internal sealed class TnDiagnostic
{
    public TnDiagnostic(TnDiagnosticSeverity severity, string message, int lineNumber)
    {
        Severity = severity;
        Message = message;
        LineNumber = lineNumber;
    }

    public TnDiagnosticSeverity Severity { get; }
    public string Message { get; }
    public int LineNumber { get; }
}

internal enum TnDiagnosticSeverity { Warning, Error, Info }

internal static class LocalizationHash
{
    private const ulong FnvOffsetBasis = 0xCBF29CE484222325UL;
    private const ulong FnvPrime = 0x100000001B3UL;

    public static ulong Compute(string source)
    {
        ulong hash = FnvOffsetBasis;
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            hash ^= (byte)(c & 0xFF);
            hash *= FnvPrime;
            hash ^= (byte)((c >> 8) & 0xFF);
            hash *= FnvPrime;
        }

        return hash;
    }
}