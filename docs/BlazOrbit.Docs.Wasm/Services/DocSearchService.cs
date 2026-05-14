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

    private static readonly string[] DefaultSuggestionRoutes =
    [
        "/getting-started",
        "/components/forms",
        "/components/data/datagrid",
        "/components/charts",
        "/utils/themegenerator"
    ];

    public IReadOnlyList<DocSearchResult> GetDefaultSuggestions()
    {
        List<DocSearchResult> results = [];
        foreach (string route in DefaultSuggestionRoutes)
        {
            DocSearchItem? item = _index.FirstOrDefault(i => i.Route.Equals(route, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                results.Add(new DocSearchResult(item, 0));
            }
        }

        return results;
    }

    public IReadOnlyList<DocSearchResult> Search(string query, int maxResults = 25)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
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
        string route = item.Route;

        if (title.Equals(query, StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        if (title.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 80;
        }

        if (route.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 70;
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
            new DocSearchItem(_loc["Variants"], "/concepts/variants", [_loc["Developer"]],
                "variant template registry customization theme"),
            new DocSearchItem(_loc["Theming"], "/concepts/theming", [_loc["Developer"]],
                "theme light dark palette color css custom property tokens"),
            new DocSearchItem(_loc["Accessibility"], "/concepts/accessibility", [_loc["Developer"]],
                "a11y aria wcag keyboard focus screen reader semantic"),

            // Home
            new DocSearchItem("Home", "/", [],
                "home landing start blazorbit overview introduction"),

            // Getting Started
            new DocSearchItem(_loc["Getting Started"], "/getting-started", [_loc["Getting Started"]],
                "install nuget package setup quickstart register services"),
            new DocSearchItem(_loc["Templates"], "/getting-started/templates", [_loc["Getting Started"]],
                "templates dotnet new blazorbit-server blazorbit-wasm scaffolding starter project localization charts"),
            new DocSearchItem(_loc["AI Skill"], "/getting-started/ai-skill", [_loc["Getting Started"]],
                "ai skill claude assistant agent integration"),

            // Forms
            new DocSearchItem(_loc["Forms"], "/components/forms", [_loc["Components"], _loc["Forms"]],
                "forms overview validation editcontext fluent input controls"),
            new DocSearchItem(_loc["AutoComplete"], "/components/forms/autocomplete",
                [_loc["Components"], _loc["Forms"]],
                "autocomplete typeahead async search debounce suggestion combobox"),
            new DocSearchItem(_loc["Checkbox"], "/components/forms/checkbox", [_loc["Components"], _loc["Forms"]],
                "check boolean form editcontext validation toggle"),
            new DocSearchItem(_loc["Color Picker"], "/components/forms/color", [_loc["Components"], _loc["Forms"]],
                "color picker hex rgb hsv palette input"),
            new DocSearchItem(_loc["DateTime"], "/components/forms/datetime", [_loc["Components"], _loc["Forms"]],
                "date time picker calendar format input"),
            new DocSearchItem(_loc["Date Range"], "/components/forms/daterange", [_loc["Components"], _loc["Forms"]],
                "date range picker start end interval period dual calendar presets"),
            new DocSearchItem(_loc["Dropdown"], "/components/forms/dropdown", [_loc["Components"], _loc["Forms"]],
                "dropdown select searchable option list input"),
            new DocSearchItem(_loc["File Upload"], "/components/forms/file-upload", [_loc["Components"], _loc["Forms"]],
                "file upload drop zone drag drop attachment image preview multi multiple"),
            new DocSearchItem(_loc["Number"], "/components/forms/number", [_loc["Components"], _loc["Forms"]],
                "number input numeric spin min max form"),
            new DocSearchItem(_loc["Number Slider"], "/components/forms/numberslider",
                [_loc["Components"], _loc["Forms"]], "number slider range numeric form control"),
            new DocSearchItem(_loc["OTP"], "/components/forms/otp", [_loc["Components"], _loc["Forms"]],
                "otp one time code verification pin auth two factor 2fa mfa"),
            new DocSearchItem(_loc["Password"], "/components/forms/password", [_loc["Components"], _loc["Forms"]],
                "password input mask reveal toggle visibility strength meter rules secret credential"),
            new DocSearchItem(_loc["Radio"], "/components/forms/radio", [_loc["Components"], _loc["Forms"]],
                "radio button group option single choice form"),
            new DocSearchItem(_loc["Range Slider"], "/components/forms/rangeslider",
                [_loc["Components"], _loc["Forms"]], "range slider dual numeric form control"),
            new DocSearchItem(_loc["Switch"], "/components/forms/switch", [_loc["Components"], _loc["Forms"]],
                "switch toggle boolean on off form"),
            new DocSearchItem(_loc["Text"], "/components/forms/text", [_loc["Components"], _loc["Forms"]],
                "text input string single line form field"),
            new DocSearchItem(_loc["TextArea"], "/components/forms/textarea", [_loc["Components"], _loc["Forms"]],
                "textarea multiline text input form field"),

            // Actions
            new DocSearchItem(_loc["Button"], "/components/actions/button", [_loc["Components"], _loc["Actions"]],
                "button click action submit icon loading color"),

            // Display
            new DocSearchItem(_loc["Avatar"], "/components/display/avatar", [_loc["Components"], _loc["Display"]],
                "avatar profile photo initials gradient image fallback group stack overflow"),
            new DocSearchItem(_loc["Banner"], "/components/display/banner", [_loc["Components"], _loc["Display"]],
                "banner alert notice message severity inline persistent dismissible callout"),
            new DocSearchItem(_loc["Chip"], "/components/display/chip", [_loc["Components"], _loc["Display"]],
                "chip tag filter selectable removable badge label pill"),
            new DocSearchItem(_loc["Progress"], "/components/display/progress", [_loc["Components"], _loc["Display"]],
                "progress bar ring linear circular indeterminate determinate segments percentage loading"),
            new DocSearchItem(_loc["Rating"], "/components/display/rating", [_loc["Components"], _loc["Display"]],
                "rating stars review feedback half score keyboard"),
            new DocSearchItem(_loc["Stat Card"], "/components/display/stat-card", [_loc["Components"], _loc["Display"]],
                "stat card kpi metric dashboard delta trend sparkline value indicator"),

            // Layout & Containers
            new DocSearchItem(_loc["Accordion"], "/components/layout/accordion",
                [_loc["Components"], _loc["Layout & Containers"]],
                "accordion expand collapse panel section disclosure single multiple"),
            new DocSearchItem(_loc["Card"], "/components/layout/card",
                [_loc["Components"], _loc["Layout & Containers"]], "card container surface content panel elevation"),
            new DocSearchItem(_loc["Flex Stack"], "/components/layout/flex-stack",
                [_loc["Components"], _loc["Layout & Containers"]], "flex stack layout alignment gap spacer distribute"),
            new DocSearchItem(_loc["Grid"], "/components/layout/grid",
                [_loc["Components"], _loc["Layout & Containers"]],
                "grid layout column row responsive system container"),
            new DocSearchItem(_loc["Aspect Ratio"], "/components/layout/aspect-ratio",
                [_loc["Components"], _loc["Layout & Containers"]],
                "aspect ratio width height video iframe image container constraint 16/9 4/3 1/1"),
            new DocSearchItem(_loc["Container"], "/components/layout/container",
                [_loc["Components"], _loc["Layout & Containers"]],
                "container max-width section page width tier semantic wrapper"),
            new DocSearchItem(_loc["Page Header"], "/components/layout/page-header",
                [_loc["Components"], _loc["Layout & Containers"]],
                "page header title eyebrow lead breadcrumbs actions toolbar sticky"),
            new DocSearchItem(_loc["Splitter"], "/components/layout/splitter",
                [_loc["Components"], _loc["Layout & Containers"]],
                "splitter resize panes drag gripper sidebar split horizontal vertical multi-pane"),

            // Services — top-level group spanning the in-main Confirm + Dialog/Drawer/Toast/Tooltip
            // hosts plus the opt-in Hotkeys / Notifications NuGet packages.
            new DocSearchItem(_loc["Confirm"], "/components/services/confirm", [_loc["Services"]],
                "confirm dialog ask service async severity danger warning info modal replacement"),
            new DocSearchItem(_loc["Dialog & Drawer"], "/components/overlays/modal", [_loc["Services"]],
                "dialog drawer modal overlay popup host dismiss"),
            new DocSearchItem(_loc["Hotkey"], "/components/services/hotkey", [_loc["Services"]],
                "hotkey shortcut keyboard combo keybinding registry global page cheat sheet ctrl shift alt meta"),
            new DocSearchItem(_loc["Notifications"], "/components/services/notifications", [_loc["Services"]],
                "notification center inbox bell badge unread mark read severity push persistent toast alternative"),
            new DocSearchItem(_loc["Toast"], "/components/overlays/toast", [_loc["Services"]],
                "toast snackbar notification alert message popup"),
            new DocSearchItem(_loc["Tooltip"], "/components/overlays/tooltip", [_loc["Services"]],
                "tooltip hint popover description title hover focus"),

            // Navigation
            new DocSearchItem(_loc["Breadcrumbs"], "/components/navigation/breadcrumbs",
                [_loc["Components"], _loc["Navigation"]],
                "breadcrumbs breadcrumb trail path hierarchy back navigation crumbs"),
            new DocSearchItem(_loc["Tabs"], "/components/navigation/tabs", [_loc["Components"], _loc["Navigation"]],
                "tabs tab panel navigation switch content segmented"),
            new DocSearchItem(_loc["Tree Menu"], "/components/navigation/treemenu",
                [_loc["Components"], _loc["Navigation"]], "tree menu nested hierarchical navigation expand collapse"),
            new DocSearchItem(_loc["Tree Selector"], "/components/navigation/treeselector",
                [_loc["Components"], _loc["Navigation"]], "tree selector hierarchical checkbox selection pick"),
            new DocSearchItem(_loc["Timeline"], "/components/navigation/timeline",
                [_loc["Components"], _loc["Navigation"]],
                "timeline events history activity log chronology vertical horizontal alternating grouped"),
            new DocSearchItem(_loc["Stepper"], "/components/navigation/stepper",
                [_loc["Components"], _loc["Navigation"]],
                "stepper wizard steps multi-step flow progress horizontal vertical EditContext form sequence"),

            // Data Display
            new DocSearchItem(_loc["Code Block"], "/components/data/codeblock",
                [_loc["Components"], _loc["Data Display"]], "code syntax highlight snippet preformatted prism"),
            new DocSearchItem(_loc["DataCards"], "/components/data/datacards",
                [_loc["Components"], _loc["Data Display"]],
                "datacards card list collection template repeater aggregate row actions bulk skeleton row detail expansion empty cta"),
            new DocSearchItem(_loc["DataGrid"], "/components/data/datagrid", [_loc["Components"], _loc["Data Display"]],
                "datagrid table column sort filter pagination rows aggregate footer multi-sort row actions bulk row detail expansion skeleton loading error empty cta"),

            // Charts
            new DocSearchItem(_loc["Charts"], "/components/charts", [_loc["Components"], _loc["Charts"]],
                "charts overview gallery all types"),
            new DocSearchItem(_loc["Bar"], "/components/charts/bar", [_loc["Components"], _loc["Charts"]],
                "bar chart vertical column stacked percent bidirectional waterfall categorical"),
            new DocSearchItem(_loc["Line"], "/components/charts/line", [_loc["Components"], _loc["Charts"]],
                "line chart series temporal smooth markers crosshair zoom brush streaming annotations"),
            new DocSearchItem(_loc["Area"], "/components/charts/area", [_loc["Components"], _loc["Charts"]],
                "area chart filled stacked series temporal smooth"),
            new DocSearchItem(_loc["Pie"], "/components/charts/pie", [_loc["Components"], _loc["Charts"]],
                "pie chart slice percentage distribution circular"),
            new DocSearchItem(_loc["Donut"], "/components/charts/donut", [_loc["Components"], _loc["Charts"]],
                "donut chart ring slice percentage distribution kpi center label"),
            new DocSearchItem(_loc["Scatter"], "/components/charts/scatter", [_loc["Components"], _loc["Charts"]],
                "scatter bubble chart correlation distribution xy plot multi-dimensional size"),
            new DocSearchItem(_loc["Sparkline"], "/components/charts/sparkline", [_loc["Components"], _loc["Charts"]],
                "sparkline inline chart compact kpi card trend mini"),
            new DocSearchItem(_loc["Histogram"], "/components/charts/histogram", [_loc["Components"], _loc["Charts"]],
                "histogram distribution frequency bins sturges scott freedman-diaconis quantile"),
            new DocSearchItem(_loc["Candlestick"], "/components/charts/candlestick",
                [_loc["Components"], _loc["Charts"]],
                "candlestick ohlc finance trading open high low close volume price"),
            new DocSearchItem(_loc["Funnel"], "/components/charts/funnel", [_loc["Components"], _loc["Charts"]],
                "funnel chart pipeline conversion sales onboarding tapered"),
            new DocSearchItem(_loc["Gauge"], "/components/charts/gauge", [_loc["Components"], _loc["Charts"]],
                "gauge chart kpi semicircle full ring threshold zones speedometer"),
            new DocSearchItem(_loc["Radar"], "/components/charts/radar", [_loc["Components"], _loc["Charts"]],
                "radar spider chart multi-dimensional polygon polar comparison skills"),
            new DocSearchItem(_loc["Heatmap"], "/components/charts/heatmap", [_loc["Components"], _loc["Charts"]],
                "heatmap matrix grid intensity color ramp calendar correlation cohort"),
            new DocSearchItem(_loc["Boxplot"], "/components/charts/boxplot", [_loc["Components"], _loc["Charts"]],
                "boxplot box whisker quartile distribution outliers iqr median statistical"),
            new DocSearchItem(_loc["Polar Area"], "/components/charts/polar-area", [_loc["Components"], _loc["Charts"]],
                "polar area coxcomb rose chart cyclic uniform angular wind direction"),
            new DocSearchItem(_loc["Treemap"], "/components/charts/treemap", [_loc["Components"], _loc["Charts"]],
                "treemap hierarchy tree rectangles squarified drilldown portfolio storage budget"),
            new DocSearchItem(_loc["Sunburst"], "/components/charts/sunburst", [_loc["Components"], _loc["Charts"]],
                "sunburst radial hierarchy concentric arcs rings drilldown donut"),
            new DocSearchItem(_loc["Sankey"], "/components/charts/sankey", [_loc["Components"], _loc["Charts"]],
                "sankey flow diagram ribbons energy migration conversion supply chain"),
            new DocSearchItem(_loc["Stock"], "/components/charts/stock", [_loc["Components"], _loc["Charts"]],
                "stock candlestick OHLC RSI MACD bollinger volume technical analysis trading"),
            new DocSearchItem(_loc["Mixed"], "/components/charts/mixed", [_loc["Components"], _loc["Charts"]],
                "mixed combo combined bar line area secondary axis dual axis actual vs target"),
            new DocSearchItem(_loc["Filter Context"], "/components/charts/filter-context", [_loc["Components"], _loc["Charts"]],
                "cross filter context cascading dashboard interactive drill down click coordinated"),

            // Display & Media
            new DocSearchItem(_loc["Badge"], "/components/display/badge", [_loc["Components"], _loc["Display & Media"]],
                "badge chip pill status indicator count label"),
            new DocSearchItem(_loc["Carousel"], "/components/display/carousel",
                [_loc["Components"], _loc["Display & Media"]],
                "carousel slideshow slide gallery image rotator coverflow wheel autoplay loop swipe"),
            new DocSearchItem(_loc["Svg Icon"], "/components/display/svgicon",
                [_loc["Components"], _loc["Display & Media"]], "svg icon material iconography image vector"),

            // (Dialog & Drawer / Toast / Tooltip moved into the Services top-level group above.)

            // Utilities
            new DocSearchItem(_loc["Draggable"], "/components/utilities/draggable",
                [_loc["Components"], _loc["Utilities"]], "draggable drag drop reorder sortable move handle"),
            new DocSearchItem(_loc["Theme Selector"], "/components/utilities/themeselector",
                [_loc["Components"], _loc["Utilities"]], "theme selector toggle light dark mode switch"),

            // Utils
            new DocSearchItem(_loc["Theme Generator"], "/utils/themegenerator", [_loc["Utils"]],
                "theme generator custom palette design tokens builder creator typography sizing density borders outline radius z-index ripple"),

            // Features
            new DocSearchItem(_loc["Validation"], "/features/validation", [_loc["Features"]],
                "validation fluent editcontext data annotations form rules"),
            new DocSearchItem(_loc["Localization"], "/features/localization", [_loc["Features"]],
                "i18n l10n culture language resx translation globalize"),
            new DocSearchItem(_loc["Icon Gallery"], "/features/icons", [_loc["Features"]], "icons gallery"),

            // Live development
            new DocSearchItem(_loc["Live Development"], "/live-development", [_loc["Live Development"]],
                "live development"),

            // Misc
            new DocSearchItem(_loc["Privacy"], "/privacy", [_loc["Legal"]],
                "privacy policy data tracking cookies analytics")
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