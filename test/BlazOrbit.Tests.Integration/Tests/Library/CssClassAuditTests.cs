using FluentAssertions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Detects CSS classes applied in markup (.razor / .cs) that have no corresponding
/// selector in any .css or .razor.css file under src/BlazOrbit.
///
/// The audit distinguishes two cases per CSS-OPT-05:
///   (a) Intentional public override / test hooks — allowed via <see cref="GhostClassAllowlist"/>.
///   (b) Residuals from refactor — must be removed from markup.
/// </summary>
[Trait("Library", "CssAudit")]
public class CssClassAuditTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();
    private static readonly string SrcBlazOrbit = Path.Combine(RepoRoot, "src", "BlazOrbit");

    // Regex for a valid CSS class name (used both in CSS selectors and in markup).
    private const string ClassName = @"[a-zA-Z_-][a-zA-Z0-9_-]*";

    // Matches a CSS class selector: .classname when it appears after a combinator,
    // at the start of a selector list, or inside :is() / :where() / :not() / :has().
    // This deliberately avoids false positives like '.md' inside media-query features.
    private static readonly Regex CssClassSelector =
        new(@"(?<=[\s,>+~\[\(:]|^)\." + $"(?<name>{ClassName})", RegexOptions.Compiled);

    // Matches class="..." or class='...' in Razor markup.
    private static readonly Regex RazorClassLiteral =
        new(@"class\s*=\s*[""'](?<value>[^""']*)[""']", RegexOptions.Compiled);

    // Matches C# string literals that look like CSS class names used in class composition.
    private static readonly Regex CSharpClassLiteral =
        new("\"(?<name>bob-[a-zA-Z0-9_-]+(?:__[a-zA-Z0-9_-]+)?(?:--[a-zA-Z0-9_-]+)?)\"", RegexOptions.Compiled);

    /// <summary>
    /// Classes applied in markup that intentionally have no CSS selector.
    /// Each entry must explain why the class is kept (public hook, test anchor, etc.)
    /// and which test verifies its survival.
    /// </summary>
    private static readonly Dictionary<string, string> GhostClassAllowlist = new(StringComparer.Ordinal)
    {
        ["bob-button__icon"] = "Public hook for button icon styling; verified by BOBButtonStateTests.",
        ["bob-button__icon--leading"] = "Modifier for leading icon; verified by BOBButtonStateTests.",
        ["bob-button__icon--trailing"] = "Modifier for trailing icon; verified by BOBButtonStateTests.",
        ["bob-tabs__tab-label"] = "Structural/test hook for tab label text; verified by BOBTabsRenderingTests.",
        ["bob-theme-editor"] = "Root hook for theme editor container; verified by BOBThemeEditorRenderingTests.",
        ["bob-theme-editor__category"] = "Structural hook for category group; verified by BOBThemeEditorStateTests.",
        ["bob-theme-editor__category-title"] = "Structural hook for category heading; verified by BOBThemeEditorAccessibilityTests.",
        ["bob-theme-preview"] = "Root hook for theme preview; verified by BOBThemePreviewRenderingTests.",
        ["bob-theme-preview__row"] = "Structural hook for preview row; verified by BOBThemePreviewRenderingTests.",
        ["bob-theme-preview__section"] = "Structural hook for preview section; verified by BOBThemePreviewRenderingTests.",
        ["bob-tree-menu__submenu"] = "Structural/test hook for submenu container; verified by BOBTreeMenuInteractionTests.",
        ["bob-tree-selector__container"] = "Structural/test hook for tree root; verified by BOBTreeSelectorAccessibilityTests.",
        ["bob-tree-selector__expander"] = "Structural/test hook for expand button; verified by BOBTreeSelectorStateTests.",
    };

    [Fact]
    public void Should_Have_Css_Selector_For_Each_Class_Used_In_Markup()
    {
        HashSet<string> css = ExtractClassesFromCss();
        HashSet<string> markup = ExtractClassesFromMarkup();

        IEnumerable<string> ghosts = markup
            .Except(css)
            .Except(GhostClassAllowlist.Keys)
            .OrderBy(s => s, StringComparer.Ordinal);

        ghosts.Should().BeEmpty(
            because: "every CSS class written onto the DOM must be selected by at least one " +
                     "CSS rule, otherwise the class is dead weight or styling is missing. " +
                     "If the class is an intentional public hook or test anchor, add it to " +
                     "GhostClassAllowlist with a justification and a test reference.\n\n" +
                     "Ghost classes:\n  " +
                     string.Join("\n  ", ghosts.Select(o => $".{o}")));
    }

    [Fact]
    public void Allowlist_Should_Not_Contain_Stale_Entries()
    {
        HashSet<string> css = ExtractClassesFromCss();
        HashSet<string> markup = ExtractClassesFromMarkup();

        List<string> stale = [];

        foreach (string key in GhostClassAllowlist.Keys)
        {
            if (!markup.Contains(key))
            {
                stale.Add($"GhostClassAllowlist[\"{key}\"] — code no longer emits .{key}");
            }
            else if (css.Contains(key))
            {
                stale.Add($"GhostClassAllowlist[\"{key}\"] — CSS now selects on .{key}; remove the entry");
            }
        }

        stale.Should().BeEmpty(
            because: "stale allowlist entries hide future drift. Remove the entries listed below.\n\n" +
                     string.Join("\n", stale));
    }

    private static HashSet<string> ExtractClassesFromCss()
    {
        HashSet<string> result = new(StringComparer.Ordinal);
        if (!Directory.Exists(SrcBlazOrbit))
        {
            return result;
        }

        foreach (string file in Directory.EnumerateFiles(SrcBlazOrbit, "*.css", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}wwwroot{Path.DirectorySeparatorChar}"))
            {
                continue;
            }

            string content = File.ReadAllText(file);
            foreach (Match m in CssClassSelector.Matches(content))
            {
                result.Add(m.Groups["name"].Value);
            }
        }

        return result;
    }

    private static HashSet<string> ExtractClassesFromMarkup()
    {
        HashSet<string> result = new(StringComparer.Ordinal);
        if (!Directory.Exists(SrcBlazOrbit))
        {
            return result;
        }

        // .razor files — literal class attributes
        foreach (string file in Directory.EnumerateFiles(SrcBlazOrbit, "*.razor", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            {
                continue;
            }

            string content = File.ReadAllText(file);
            foreach (Match m in RazorClassLiteral.Matches(content))
            {
                string value = m.Groups["value"].Value;
                foreach (string part in value.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries))
                {
                    // Skip Razor expressions like @(...) and plain @variables
                    if (part.StartsWith("@"))
                    {
                        continue;
                    }

                    // Keep only names that look like CSS classes (letters, digits, -, _)
                    if (Regex.IsMatch(part, $"^{ClassName}$"))
                    {
                        result.Add(part);
                    }
                }
            }
        }

        // .razor.cs and .cs files — string literals that look like CSS classes
        foreach (string ext in new[] { "*.razor.cs", "*.cs" })
        {
            foreach (string file in Directory.EnumerateFiles(SrcBlazOrbit, ext, SearchOption.AllDirectories))
            {
                if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                    || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                {
                    continue;
                }

                string content = File.ReadAllText(file);
                foreach (Match m in CSharpClassLiteral.Matches(content))
                {
                    result.Add(m.Groups["name"].Value);
                }
            }
        }

        return result;
    }

    private static string ResolveRepoRoot([CallerFilePath] string? thisFile = null)
    {
        DirectoryInfo? dir = new FileInfo(thisFile!).Directory;
        for (int i = 0; i < 4 && dir is not null; i++)
        {
            dir = dir.Parent;
        }

        return dir!.FullName;
    }
}
