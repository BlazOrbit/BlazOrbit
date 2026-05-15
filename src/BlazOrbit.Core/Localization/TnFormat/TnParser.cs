using System.Collections.Frozen;
using System.Text;

namespace BlazOrbit.Localization.TnFormat;

/// <summary>
/// Parser for the BlazOrbit <c>.tn</c> translation format. Designed for minimal escape:
/// any line whose first non-whitespace character is not <c>#</c>, <c>@</c>, or <c>//</c> is
/// treated as literal content. The leading-character constraint is the only thing escaped -
/// prefix the line with <c>\</c> to put a literal <c>#</c>, <c>@</c>, or <c>/</c> at column 0.
/// </summary>
/// <remarks>
/// <para>
/// Grammar (informal):
/// <code>
/// document     := (blank | comment | culture_header | entry)*
/// culture_header := "@" SPACE+ CULTURE_NAME EOL
/// entry        := key_line+ value_line*
/// key_line     := "#" SPACE+ TEXT EOL
/// value_line   := TEXT EOL                  (* line that does not start a new key/header *)
/// comment      := "//" TEXT EOL
/// blank        := EOL
/// </code>
/// </para>
/// <para>
/// Multi-line keys: consecutive <c>#</c>-prefixed lines concatenate with a single newline.
/// Multi-line values: any non-special lines after the key until the next <c>#</c>/<c>@</c>/EOF
/// join the previous newline-separated value.
/// </para>
/// <para>
/// Hashing uses <see cref="BobLocalizationHash"/> so output is consistent with the source-gen
/// build-time call sites without coupling the parser to specific call sites.
/// </para>
/// </remarks>
public static class TnParser
{
    /// <summary>Parses a <c>.tn</c> file contents. Returns a document with translations + diagnostics.</summary>
    /// <param name="content">Raw file content. Mixed line endings (<c>\r\n</c> / <c>\n</c>) supported.</param>
    /// <param name="defaultCulture">
    /// Culture to assign entries that appear before any <c>@ culture</c> header. Typical caller
    /// (the source generator) supplies the filename-derived culture here.
    /// </param>
    public static TnDocument Parse(string content, string defaultCulture = "")
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(defaultCulture);

        Dictionary<string, Dictionary<ulong, string>> output = new(StringComparer.OrdinalIgnoreCase);
        List<TnDiagnostic> diagnostics = [];

        string currentCulture = defaultCulture;
        StringBuilder? pendingKey = null;
        StringBuilder? pendingValue = null;
        int pendingKeyLine = -1;

        int lineNumber = 0;
        foreach (string rawLine in EnumerateLines(content))
        {
            lineNumber++;
            string line = rawLine;
            string trimmed = line.TrimStart();

            // Comments: discard whole line.
            if (trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            // Blank line: terminates the in-flight value if any. Subsequent text re-starts as
            // a new value line only when a key is in scope.
            if (trimmed.Length == 0)
            {
                FlushPending();
                continue;
            }

            // Culture header.
            if (trimmed.StartsWith('@'))
            {
                FlushPending();
                string cultureName = trimmed[1..].Trim();
                if (cultureName.Length == 0)
                {
                    diagnostics.Add(new TnDiagnostic(TnDiagnosticSeverity.Error, "Culture header is missing a name.",
                        lineNumber));
                    continue;
                }

                currentCulture = cultureName;
                continue;
            }

            // Key line(s).
            if (trimmed.StartsWith('#'))
            {
                string keyText = trimmed[1..].TrimStart();
                if (pendingKey is not null && pendingValue is not null)
                {
                    // Previous (key, value) pair complete - flush and start a new entry.
                    FlushPending();
                }

                if (pendingKey is null)
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

            // Value line (after a key has been parsed).
            string content_ = UnescapeLeader(trimmed);
            if (pendingKey is null)
            {
                diagnostics.Add(new TnDiagnostic(
                    TnDiagnosticSeverity.Error,
                    $"Value text outside of a key block: \"{Truncate(content_)}\"",
                    lineNumber));
                continue;
            }

            pendingValue ??= new StringBuilder();
            if (pendingValue.Length > 0)
            {
                pendingValue.Append('\n');
            }

            pendingValue.Append(content_);
        }

        // Final flush - handles documents that don't end with a blank line.
        FlushPending();

        FrozenDictionary<string, FrozenDictionary<ulong, string>> frozen = output.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.ToFrozenDictionary(),
            StringComparer.OrdinalIgnoreCase).ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        return new TnDocument(frozen, diagnostics);

        void FlushPending()
        {
            if (pendingKey is null)
            {
                return;
            }

            string key = pendingKey.ToString();
            string value = pendingValue?.ToString() ?? string.Empty;
            ulong hash = BobLocalizationHash.Compute(key);

            if (!output.TryGetValue(currentCulture, out Dictionary<ulong, string>? byHash))
            {
                byHash = new Dictionary<ulong, string>();
                output[currentCulture] = byHash;
            }

            if (byHash.ContainsKey(hash))
            {
                diagnostics.Add(new TnDiagnostic(
                    TnDiagnosticSeverity.Warning,
                    $"Duplicate key in culture '{currentCulture}': \"{Truncate(key)}\". Previous value overwritten.",
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
            char c = content[i];
            if (c == '\n')
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

    /// <summary>
    /// Strips a single leading <c>\</c> when followed by <c>#</c>, <c>@</c>, or <c>/</c> - the
    /// three characters that would otherwise start a key, culture header, or comment.
    /// </summary>
    private static string UnescapeLeader(string line)
    {
        if (line.Length >= 2 && line[0] == '\\' && line[1] is '#' or '@' or '/')
        {
            return line[1..];
        }

        return line;
    }

    private static string Truncate(string s) => s.Length <= 60 ? s : s[..57] + "...";
}