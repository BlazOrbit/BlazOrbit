using BlazOrbit.Docs.Wasm.Layout;
using BlazOrbit.Docs.Wasm.Models;
using Microsoft.Extensions.Localization;
using System.Text.RegularExpressions;

namespace BlazOrbit.Docs.Wasm.Services;

public sealed class DocSearchService
{
    private readonly IStringLocalizer<NavMenu> _loc;
    private readonly DocSearchItem[] _index;

    public DocSearchService(IStringLocalizer<NavMenu> loc)
    {
        _loc = loc;
        _index = BuildIndex();
    }

    public IReadOnlyList<DocSearchResult> Search(string query, int maxResults = 8)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<DocSearchResult>();
        }

        string q = query.Trim();
        List<DocSearchResult> results = [];

        foreach (DocSearchItem item in _index)
        {
            int score = Score(item, q);
            if (score > 0)
            {
                results.Add(new DocSearchResult(item, score));
            }
        }

        return results
            .OrderByDescending(r => r.Score)
            .Take(maxResults)
            .ToList();
    }

    private int Score(DocSearchItem item, string query)
    {
        string title = item.Title;
        string breadcrumb = string.Join(" > ", item.Breadcrumb);
        string keywords = item.Keywords ?? string.Empty;

        if (title.Equals(query, StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        if (title.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 80;
        }

        if (breadcrumb.Equals(query, StringComparison.OrdinalIgnoreCase))
        {
            return 60;
        }

        if (breadcrumb.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 40;
        }

        if (keywords.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        string[] words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 1)
        {
            int wordScore = 0;
            foreach (string word in words)
            {
                if (title.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    wordScore += 30;
                }
                else if (breadcrumb.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    wordScore += 15;
                }
                else if (keywords.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    wordScore += 5;
                }
            }

            return wordScore;
        }

        return 0;
    }

    private DocSearchItem[] BuildIndex()
    {
        return
        [
            // Developer / Concepts
            new(_loc["Architecture"], "/concepts/architecture", [_loc["Developer"]], "architecture component base pipeline abstractions"),
            new(_loc["Variants"], "/concepts/variants", [_loc["Developer"]], "variant template registry customization theme"),
            new(_loc["Theming"], "/concepts/theming", [_loc["Developer"]], "theme light dark palette color css custom property tokens"),
            new(_loc["JS Interop"], "/concepts/js-interop", [_loc["Developer"]], "javascript interop typescript behavior service jsruntime"),
            new(_loc["Accessibility"], "/concepts/accessibility", [_loc["Developer"]], "a11y aria wcag keyboard focus screen reader semantic"),

            // Guide
            new("Getting started", "/getting-started", ["Guide"], "install nuget package setup quickstart register services"),

            // Forms
            new(_loc["Checkbox"], "/components/checkbox", [_loc["Components"], _loc["Forms"]], "check boolean form editcontext validation toggle"),
            new(_loc["Color Picker"], "/components/color", [_loc["Components"], _loc["Forms"]], "color picker hex rgb hsv palette input"),
            new(_loc["DateTime"], "/components/datetime", [_loc["Components"], _loc["Forms"]], "date time picker calendar format input"),
            new(_loc["Dropdown"], "/components/dropdown", [_loc["Components"], _loc["Forms"]], "dropdown select searchable option list input"),
            new(_loc["Number"], "/components/number", [_loc["Components"], _loc["Forms"]], "number input numeric spin min max form"),
            new(_loc["Radio"], "/components/radio", [_loc["Components"], _loc["Forms"]], "radio button group option single choice form"),
            new(_loc["Switch"], "/components/switch", [_loc["Components"], _loc["Forms"]], "switch toggle boolean on off form"),
            new(_loc["Text"], "/components/text", [_loc["Components"], _loc["Forms"]], "text input string single line form field"),
            new(_loc["TextArea"], "/components/textarea", [_loc["Components"], _loc["Forms"]], "textarea multiline text input form field"),

            // Generic
            new(_loc["Badge"], "/components/badge", [_loc["Components"], _loc["Generic"]], "badge chip pill status indicator count label"),
            new(_loc["Button"], "/components/button", [_loc["Components"], _loc["Generic"]], "button click action submit icon loading color"),
            new(_loc["Code Block"], "/components/codeblock", [_loc["Components"], _loc["Generic"]], "code syntax highlight snippet preformatted prism"),
            new(_loc["DataCards"], "/components/datacards", [_loc["Components"], _loc["Generic"]], "datacards card list collection template repeater"),
            new(_loc["DataGrid"], "/components/datagrid", [_loc["Components"], _loc["Generic"]], "datagrid table column sort filter pagination rows"),
            new(_loc["Draggable"], "/components/draggable", [_loc["Components"], _loc["Generic"]], "draggable drag drop reorder sortable move handle"),
            new(_loc["Loading"], "/components/loading", [_loc["Components"], _loc["Generic"]], "loading spinner progress indicator skeleton busy"),
            new(_loc["Svg Icon"], "/components/svgicon", [_loc["Components"], _loc["Generic"]], "svg icon material iconography image vector"),
            new(_loc["Tabs"], "/components/tabs", [_loc["Components"], _loc["Generic"]], "tabs tab panel navigation switch content segmented"),
            new(_loc["Tooltip"], "/components/tooltip", [_loc["Components"], _loc["Generic"]], "tooltip hint popover description title hover focus"),
            new(_loc["Tree Selector"], "/components/treeselector", [_loc["Components"], _loc["Generic"]], "tree selector hierarchical checkbox selection pick"),

            // Layout
            new(_loc["Card"], "/components/card", [_loc["Components"], _loc["Layout"]], "card container surface content panel elevation"),
            new(_loc["Dialog & Drawer"], "/components/modal", [_loc["Components"], _loc["Layout"]], "dialog drawer modal overlay popup host dismiss"),
            new(_loc["Flex Stack"], "/components/flex-stack", [_loc["Components"], _loc["Layout"]], "flex stack layout alignment gap spacer distribute"),
            new(_loc["Grid"], "/components/grid", [_loc["Components"], _loc["Layout"]], "grid layout column row responsive system container"),
            new(_loc["Theme Selector"], "/components/themeselector", [_loc["Components"], _loc["Layout"]], "theme selector toggle light dark mode switch"),
            new(_loc["Toast"], "/components/toast", [_loc["Components"], _loc["Layout"]], "toast snackbar notification alert message popup"),
            new(_loc["Tree Menu"], "/components/treemenu", [_loc["Components"], _loc["Layout"]], "tree menu nested hierarchical navigation expand collapse"),

            // Utils
            new(_loc["Theme Generator"], "/utils/themegenerator", [_loc["Utils"]], "theme generator custom palette builder creator"),        

            // Features
            new(_loc["Validation"], "/features/validation", [_loc["Features"]], "validation fluent editcontext data annotations form rules"),
            new(_loc["Localization"], "/features/localization", [_loc["Features"]], "i18n l10n culture language resx translation globalize"),
            new(_loc["Icon Gallery"], "/features/icons", [_loc["Features"]], "icons gallery"),

            // Live development
            new(_loc["Live Development"], "/live-development", [_loc["Live Development"]], "live development"),
        ];
    }

    public static string Highlight(string text, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return System.Net.WebUtility.HtmlEncode(text);
        }

        string encodedText = System.Net.WebUtility.HtmlEncode(text);
        string encodedQuery = Regex.Escape(System.Net.WebUtility.HtmlEncode(query.Trim()));
        return Regex.Replace(encodedText, encodedQuery, "<mark>$0</mark>", RegexOptions.IgnoreCase);
    }
}
