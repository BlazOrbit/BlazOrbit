using FluentAssertions;
using System.Text.RegularExpressions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// COMP-AUDIT-CHECKLIST-01 (mecanizado): detecta anti-patrones en markup .razor
/// que violan la separación de concerns o el modelo de eventos de Blazor.
///
/// Reglas:
/// 1. Sin atributos de evento nativos (onclick, onkeydown, etc.) con JS inline.
///    Usar @onclick / @onkeydown con handlers C# o behaviors JS.
/// 2. Sin tags &lt;script&gt; dentro de componentes .razor.
/// 3. Sin atributos style="..." hardcodeados. El estilo debe venir del pipeline
///    (ComputedAttributes / --bob-inline-*) o de CSS scoped.
///
/// El allowlist cubre excepciones documentadas (ej: atributos ARIA o tests).
/// </summary>
[Trait("Library", "ComponentAudit")]
public class ComponentMarkupLintTests
{
    private static readonly string SrcBlazOrbit = Path.GetFullPath(Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", "..",
        "src", "BlazOrbit"));

    private static readonly string[] EventAttrs =
    [
        "onclick", "ondblclick", "onmousedown", "onmouseup", "onmouseover",
        "onmousemove", "onmouseout", "onkeydown", "onkeyup", "onkeypress",
        "onfocus", "onblur", "onchange", "onsubmit", "onreset", "onselect",
        "onload", "onunload", "onerror", "onresize", "onscroll", "onwheel",
        "ondrag", "ondrop", "ondragover", "ondragenter", "ondragleave",
        "ontouchstart", "ontouchmove", "ontouchend", "ontouchcancel",
        "oncontextmenu", "onfocusin", "onfocusout"
    ];

    /// <summary>
    /// Excepciones permitidas: líneas que contienen estos tokens se ignoran.
    /// Cada entrada debe tener una justificación y un test de referencia.
    /// </summary>
    private static readonly Dictionary<string, string> ScriptTagAllowlist = new()
    {
        ["BOBInitializer.razor"] = "Standard Blazor <HeadContent> pattern for injecting external StaticWebAsset script."
    };

    private static readonly Dictionary<string, string> StyleAllowlist = new()
    {
        ["BOBPerformanceDashboard.razor"] = "Internal diagnostic component; not part of public visual contract."
    };

    /// <summary>
    /// Verifica que no haya atributos de evento nativos con valores que no sean
    /// bindings de Blazor (es decir, no empiecen con @).
    /// </summary>
    [Fact]
    public void Razor_Should_Not_Contain_Native_Event_Attributes_With_Inline_Handlers()
    {
        string[] razorFiles = Directory.GetFiles(SrcBlazOrbit, "*.razor", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();

        razorFiles.Should().NotBeEmpty();

        List<string> violations = [];

        foreach (string file in razorFiles)
        {
            string[] lines = File.ReadAllLines(file);
            string fileName = Path.GetFileName(file);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.TrimStart().StartsWith("@"))
                {
                    continue; // Razor directive
                }

                if (line.TrimStart().StartsWith("<!--"))
                {
                    continue; // HTML comment
                }

                foreach (string attr in EventAttrs)
                {
                    // Look for attr="..." or attr='...' where:
                    // - the attr is NOT preceded by @ (Blazor directive)
                    // - the value does NOT start with @ (Razor expression)
                    // We search case-SENSITIVELY so PascalCase like OnClick (component parameter)
                    // does NOT match lowercase onclick (native HTML attribute).
                    string pattern = $@"(?<!\@)\b{attr}\s*=\s*[""'](?![@])[^""']*[""']";
                    if (Regex.IsMatch(line, pattern))
                    {
                        violations.Add($"{fileName}:{i + 1}: native event attribute '{attr}' with inline value");
                    }
                }
            }
        }

        violations.Should().BeEmpty(
            because: "native HTML event attributes with inline handlers are an anti-pattern in Blazor. " +
                     "Use @onclick, @onkeydown, etc. with C# handlers or JS behaviors. " +
                     "See COMP-AUDIT-CHECKLIST-01 (markup complexity / keyboard events).\n\n" +
                     string.Join("\n", violations));
    }

    /// <summary>
    /// Verifica que no haya &lt;script&gt; tags dentro de archivos .razor.
    /// </summary>
    [Fact]
    public void Razor_Should_Not_Contain_Script_Tags()
    {
        string[] razorFiles = Directory.GetFiles(SrcBlazOrbit, "*.razor", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();

        List<string> violations = [];

        foreach (string file in razorFiles)
        {
            string content = File.ReadAllText(file);
            string fileName = Path.GetFileName(file);

            // Match <script> or <script ...> (case-insensitive)
            if (Regex.IsMatch(content, @"<script\b", RegexOptions.IgnoreCase))
            {
                if (ScriptTagAllowlist.ContainsKey(fileName))
                {
                    continue;
                }

                violations.Add($"{fileName}: contains <script> tag");
            }
        }

        violations.Should().BeEmpty(
            because: "<script> tags inside .razor files violate encapsulation. " +
                     "Use TypeScript under Types/ and IJSObjectReference for JS interop. " +
                     "See COMP-AUDIT-CHECKLIST-01.\n\n" +
                     string.Join("\n", violations));
    }

    /// <summary>
    /// Verifica que no haya atributos style="..." hardcodeados en el markup.
    /// Excepción: atributos que usan @(...) (binding dinámico controlado por el pipeline).
    /// </summary>
    [Fact]
    public void Razor_Should_Not_Contain_Hardcoded_Style_Attributes()
    {
        string[] razorFiles = Directory.GetFiles(SrcBlazOrbit, "*.razor", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();

        List<string> violations = [];

        foreach (string file in razorFiles)
        {
            string[] lines = File.ReadAllLines(file);
            string fileName = Path.GetFileName(file);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.TrimStart().StartsWith("@"))
                {
                    continue;
                }

                if (line.TrimStart().StartsWith("<!--"))
                {
                    continue;
                }

                // Match style="..." or style='...' where the value does NOT contain @.
                // Values with @ are dynamic Razor expressions and are permitted.
                // We flag literal CSS like style="display: none" or style="color: red".
                string pattern = @"\bstyle\s*=\s*[""'](?![^""']*@)[^""']*[""']";
                if (Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase))
                {
                    if (StyleAllowlist.ContainsKey(fileName))
                    {
                        continue;
                    }

                    violations.Add($"{fileName}:{i + 1}: hardcoded style attribute");
                }
            }
        }

        violations.Should().BeEmpty(
            because: "hardcoded style attributes bypass the design-token system and the CSS pipeline. " +
                     "Use ComputedAttributes or CSS custom properties. " +
                     "See COMP-AUDIT-CHECKLIST-01.\n\n" +
                     string.Join("\n", violations));
    }
}
