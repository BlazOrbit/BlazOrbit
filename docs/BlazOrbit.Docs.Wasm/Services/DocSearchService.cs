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

            // Getting Started
            new(_loc["Getting Started"], "/getting-started", [_loc["Getting Started"]], "install nuget package setup quickstart register services"),

            // Forms
            new(_loc["Checkbox"], "/components/forms/checkbox", [_loc["Components"], _loc["Forms"]], "check boolean form editcontext validation toggle"),
            new(_loc["Color Picker"], "/components/forms/color", [_loc["Components"], _loc["Forms"]], "color picker hex rgb hsv palette input"),
            new(_loc["DateTime"], "/components/forms/datetime", [_loc["Components"], _loc["Forms"]], "date time picker calendar format input"),
            new(_loc["Dropdown"], "/components/forms/dropdown", [_loc["Components"], _loc["Forms"]], "dropdown select searchable option list input"),
            new(_loc["Number"], "/components/forms/number", [_loc["Components"], _loc["Forms"]], "number input numeric spin min max form"),
            new(_loc["Number Slider"], "/components/forms/numberslider", [_loc["Components"], _loc["Forms"]], "number slider range numeric form control"),
            new(_loc["Radio"], "/components/forms/radio", [_loc["Components"], _loc["Forms"]], "radio button group option single choice form"),
            new(_loc["Range Slider"], "/components/forms/rangeslider", [_loc["Components"], _loc["Forms"]], "range slider dual numeric form control"),
            new(_loc["Switch"], "/components/forms/switch", [_loc["Components"], _loc["Forms"]], "switch toggle boolean on off form"),
            new(_loc["Text"], "/components/forms/text", [_loc["Components"], _loc["Forms"]], "text input string single line form field"),
            new(_loc["TextArea"], "/components/forms/textarea", [_loc["Components"], _loc["Forms"]], "textarea multiline text input form field"),

            // Actions
            new(_loc["Button"], "/components/actions/button", [_loc["Components"], _loc["Actions"]], "button click action submit icon loading color"),

            // Layout & Containers
            new(_loc["Accordion"], "/components/layout/accordion", [_loc["Components"], _loc["Layout & Containers"]], "accordion expand collapse panel section disclosure single multiple"),
            new(_loc["Card"], "/components/layout/card", [_loc["Components"], _loc["Layout & Containers"]], "card container surface content panel elevation"),
            new(_loc["Flex Stack"], "/components/layout/flex-stack", [_loc["Components"], _loc["Layout & Containers"]], "flex stack layout alignment gap spacer distribute"),
            new(_loc["Grid"], "/components/layout/grid", [_loc["Components"], _loc["Layout & Containers"]], "grid layout column row responsive system container"),

            // Navigation
            new(_loc["Tabs"], "/components/navigation/tabs", [_loc["Components"], _loc["Navigation"]], "tabs tab panel navigation switch content segmented"),
            new(_loc["Tree Menu"], "/components/navigation/treemenu", [_loc["Components"], _loc["Navigation"]], "tree menu nested hierarchical navigation expand collapse"),
            new(_loc["Tree Selector"], "/components/navigation/treeselector", [_loc["Components"], _loc["Navigation"]], "tree selector hierarchical checkbox selection pick"),

            // Data Display
            new(_loc["Code Block"], "/components/data/codeblock", [_loc["Components"], _loc["Data Display"]], "code syntax highlight snippet preformatted prism"),
            new(_loc["DataCards"], "/components/data/datacards", [_loc["Components"], _loc["Data Display"]], "datacards card list collection template repeater"),
            new(_loc["DataGrid"], "/components/data/datagrid", [_loc["Components"], _loc["Data Display"]], "datagrid table column sort filter pagination rows"),

            // Display & Media
            new(_loc["Badge"], "/components/display/badge", [_loc["Components"], _loc["Display & Media"]], "badge chip pill status indicator count label"),
            new(_loc["Carousel"], "/components/display/carousel", [_loc["Components"], _loc["Display & Media"]], "carousel slideshow slide gallery image rotator coverflow wheel autoplay loop swipe"),
            new(_loc["Loading"], "/components/display/loading", [_loc["Components"], _loc["Display & Media"]], "loading spinner progress indicator skeleton busy"),
            new(_loc["Svg Icon"], "/components/display/svgicon", [_loc["Components"], _loc["Display & Media"]], "svg icon material iconography image vector"),

            // Overlays & Feedback
            new(_loc["Dialog & Drawer"], "/components/overlays/modal", [_loc["Components"], _loc["Overlays & Feedback"]], "dialog drawer modal overlay popup host dismiss"),
            new(_loc["Toast"], "/components/overlays/toast", [_loc["Components"], _loc["Overlays & Feedback"]], "toast snackbar notification alert message popup"),
            new(_loc["Tooltip"], "/components/overlays/tooltip", [_loc["Components"], _loc["Overlays & Feedback"]], "tooltip hint popover description title hover focus"),

            // Utilities
            new(_loc["Draggable"], "/components/utilities/draggable", [_loc["Components"], _loc["Utilities"]], "draggable drag drop reorder sortable move handle"),
            new(_loc["Theme Selector"], "/components/utilities/themeselector", [_loc["Components"], _loc["Utilities"]], "theme selector toggle light dark mode switch"),

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
