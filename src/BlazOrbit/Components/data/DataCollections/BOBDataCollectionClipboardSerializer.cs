using System.Globalization;
using System.Net;
using System.Text;

namespace BlazOrbit.Components;

/// <summary>
/// Serializes a snapshot of a data collection (rows + visible columns) to the two clipboard
/// payloads expected by spreadsheet applications:
/// <list type="bullet">
///   <item><c>text/plain</c> as TSV — Excel, Sheets, Numbers and plain editors all consume
///         tab-separated values directly.</item>
///   <item><c>text/html</c> as a <c>&lt;table&gt;</c> — preserves cell typing when the
///         destination's HTML clipboard reader is preferred (Excel especially: numbers stay
///         numbers, dates stay dates, leading-zero strings survive).</item>
/// </list>
/// Both forms write the same logical grid; the HTML variant additionally escapes special
/// characters so a cell value containing markup cannot break out of the table envelope.
/// </summary>
/// <typeparam name="TItem">Row item type, matching the parent data collection.</typeparam>
public static class BOBDataCollectionClipboardSerializer<TItem>
{
    /// <summary>
    /// Builds a TSV payload from <paramref name="items"/> using the visible subset of
    /// <paramref name="columns"/>. Tabs and newlines inside cell values are neutralised to
    /// spaces so the embedded character does not break TSV row/column structure when pasted.
    /// </summary>
    /// <param name="items">Row snapshot to serialise. Typically the current
    /// filtered / sorted view, not the full underlying source.</param>
    /// <param name="columns">Column definitions; <c>Visible == false</c> columns are skipped.</param>
    /// <param name="includeHeaders">When <see langword="true"/>, prepends a header row with
    /// each column's <c>Header</c> string. Null headers serialise as empty cells.</param>
    public static string BuildTsv(
        IEnumerable<TItem> items,
        IEnumerable<DataColumnRegistration<TItem>> columns,
        bool includeHeaders = true)
    {
        DataColumnRegistration<TItem>[] visible = columns.Where(c => c.Visible).ToArray();
        StringBuilder sb = new();

        // Use explicit \n (not AppendLine which is platform-dependent CRLF on Windows). TSV
        // consumers accept either, but \n keeps the wire format stable across hosts and avoids
        // a stray \r leaking into the last cell of each row when the destination splits on \n.
        if (includeHeaders)
        {
            sb.Append(string.Join('\t', visible.Select(c => NeutraliseSeparators(c.Header ?? string.Empty))));
            sb.Append('\n');
        }

        foreach (TItem item in items)
        {
            sb.Append(string.Join('\t', visible.Select(c => NeutraliseSeparators(FormatCellValue(item, c)))));
            sb.Append('\n');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds an HTML <c>&lt;table&gt;</c> payload from <paramref name="items"/> using the
    /// visible subset of <paramref name="columns"/>. Cell values are HTML-encoded so embedded
    /// markup cannot escape the cell. Numbers and dates are emitted via their formatted text
    /// only — Excel infers the cell type from the formatted value, so a numeric-looking string
    /// pastes as a number.
    /// </summary>
    /// <param name="items">Row snapshot to serialise.</param>
    /// <param name="columns">Column definitions; <c>Visible == false</c> columns are skipped.</param>
    /// <param name="includeHeaders">When <see langword="true"/>, wraps a <c>&lt;thead&gt;</c>
    /// with one <c>&lt;th&gt;</c> per visible column.</param>
    public static string BuildHtml(
        IEnumerable<TItem> items,
        IEnumerable<DataColumnRegistration<TItem>> columns,
        bool includeHeaders = true)
    {
        DataColumnRegistration<TItem>[] visible = columns.Where(c => c.Visible).ToArray();
        StringBuilder sb = new();

        sb.Append("<table>");

        if (includeHeaders)
        {
            sb.Append("<thead><tr>");
            foreach (DataColumnRegistration<TItem> col in visible)
            {
                sb.Append("<th>").Append(WebUtility.HtmlEncode(col.Header ?? string.Empty)).Append("</th>");
            }
            sb.Append("</tr></thead>");
        }

        sb.Append("<tbody>");
        foreach (TItem item in items)
        {
            sb.Append("<tr>");
            foreach (DataColumnRegistration<TItem> col in visible)
            {
                sb.Append("<td>").Append(WebUtility.HtmlEncode(FormatCellValue(item, col))).Append("</td>");
            }
            sb.Append("</tr>");
        }
        sb.Append("</tbody>");

        sb.Append("</table>");
        return sb.ToString();
    }

    /// <summary>
    /// Projects a single cell value through the column's <c>ValueSelector</c> + optional
    /// <c>Format</c>. Mirrors the runtime cell-render path so the clipboard content equals
    /// what the user sees on screen. <see langword="null"/> → empty string (Excel treats it
    /// as a blank cell rather than the literal text "null").
    /// </summary>
    private static string FormatCellValue(TItem item, DataColumnRegistration<TItem> col)
    {
        if (col.ValueSelector is null)
        {
            return string.Empty;
        }

        object? value = col.ValueSelector(item);
        if (value is null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(col.Format) && value is IFormattable formattable)
        {
            return formattable.ToString(col.Format, CultureInfo.CurrentCulture) ?? string.Empty;
        }

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Replaces TSV-significant characters (tab, CR, LF) with single spaces. Quoting would be
    /// the alternative but is incompatible with bare TSV — Excel does not consume quoted TSV
    /// the way it consumes quoted CSV. Spaces are a lossy but predictable normalisation; the
    /// HTML payload preserves the original characters for paste targets that read HTML first.
    /// </summary>
    private static string NeutraliseSeparators(string value) =>
        value.Length == 0 ? value : value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
}
