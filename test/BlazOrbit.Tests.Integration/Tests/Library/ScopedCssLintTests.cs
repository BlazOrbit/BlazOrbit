using FluentAssertions;
using System.Text.RegularExpressions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// mecaniza las reglas de CSS scoped documentadas sobre los archivos <c>*.razor.css</c> del
/// paquete principal.
///
/// Reglas cubiertas aquí (las restantes ya están mecanizadas por:
/// <see cref="CssArchitectureLintTests"/>,
/// <see cref="CssScopedSelectorAuditTests"/>,
/// <see cref="ComponentMarkupLintTests"/>,
/// <see cref="ComponentRootContractLintTests"/>):
///
/// 1. Selector form (regla 2): scoped CSS debe usar el formato corto
///    <c>[data-bob-component="..."]</c>. La forma larga
///    <c>bob-component[data-bob-component="..."]</c> sólo se permite en los
///    bundles globales generados por BuildTools.
/// 2. Private-var pattern (regla 4): cualquier referencia a
///    <c>var(--bob-inline-*)</c> debe estar dentro de la declaración de una
///    variable privada <c>--_X-...</c>; nunca consumida directamente por una
///    propiedad CSS final (color, background, border, …).
/// 3. Sizing/responsive flow (regla 5): <c>@media</c> queries sólo se
///    permiten en componentes de Layout (cambian flujo, no dimensiones).
/// 4. Theme colors (regla 8): prohibidos los literales de color
///    (<c>#hex</c>, <c>rgb(*)</c>, <c>rgba(*)</c>, <c>hsl(*)</c>,
///    <c>hsla(*)</c>) en scoped CSS. Excepción documentada:
///    <c>BOBColorPicker.razor.css</c> renderiza gradientes de saturación/hue
///    con stops fijos por contrato del color picker.
/// </summary>
[Trait("Library", "ScopedCssLint")]
public class ScopedCssLintTests
{
    private static readonly string SrcBlazOrbit = Path.GetFullPath(Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", "..",
        "src", "BlazOrbit"));

    /// <summary>
    /// Componentes cuya carpeta empieza por <c>Layout/</c> pueden cambiar
    /// el flujo de la página vía <c>@media</c> (grid template, drawer
    /// activation, toast position). El resto resuelve dimensiones vía los
    /// multiplicadores <c>--bob-size-multiplier</c> / <c>--bob-density-multiplier</c>.
    /// </summary>
    private static readonly string LayoutFolderToken =
        $"{Path.DirectorySeparatorChar}Layout{Path.DirectorySeparatorChar}";

    /// <summary>
    /// Archivos cuyo contrato visual exige literales de color fijos.
    /// Cada entrada debe tener una justificación. Cualquier nuevo allowlisted
    /// requiere una entrada en este diccionario y mención en el PR.
    /// </summary>
    private static readonly Dictionary<string, string> HardcodedColorAllowlist = new()
    {
        ["BOBColorPicker.razor.css"] =
            "Picker UI: gradient stops del canvas saturación/luminosidad (#000/#fff) y la rampa hue (hsl(0..360, 100%, 50%)) son intrínsecos al contrato del color picker, no temáticos.",
    };

    private static readonly Regex LongFormRootSelector = new(
        @"\bbui-component\s*\[data-bob-component\s*=\s*""[^""]+""\]",
        RegexOptions.Compiled);

    private static readonly Regex MediaQuery = new(
        @"@media\b",
        RegexOptions.Compiled);

    private static readonly Regex HardcodedColor = new(
        @"#[0-9a-fA-F]{3,8}\b|\brgb\(|\brgba\(|\bhsl\(|\bhsla\(",
        RegexOptions.Compiled);

    private static readonly Regex CssCommentPattern = new(
        @"/\*.*?\*/",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static IEnumerable<string> EnumerateScopedCssFiles()
        => Directory.GetFiles(SrcBlazOrbit, "*.razor.css", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"));

    [Fact]
    public void RazorCss_Should_Use_Short_Form_Root_Selector()
    {
        string[] files = EnumerateScopedCssFiles().ToArray();
        files.Should().NotBeEmpty();

        List<string> violations = [];

        foreach (string file in files)
        {
            string content = StripComments(File.ReadAllText(file));
            string fileName = Path.GetFileName(file);

            foreach (Match m in LongFormRootSelector.Matches(content))
            {
                int line = LineOf(content, m.Index);
                violations.Add($"{fileName}:{line}: long-form '{m.Value}' (use the short form [data-bob-component=\"...\"])");
            }
        }

        violations.Should().BeEmpty(
            because: "scoped CSS already runs inside Blazor's per-component [b-xxx] scope; " +
                     "the short form [data-bob-component=\"...\"] is canonical. " +
                     "The long form is reserved for global bundles in CssBundle/.\n\n" +
                     string.Join("\n", violations));
    }

    [Fact]
    public void RazorCss_Should_Not_Consume_Inline_Vars_Outside_Private_Var_Declarations()
    {
        string[] files = EnumerateScopedCssFiles().ToArray();
        List<string> violations = [];

        foreach (string file in files)
        {
            string content = StripComments(File.ReadAllText(file));
            string fileName = Path.GetFileName(file);

            // Walk declarations one by one. A declaration is `prop: value;`,
            // potentially across multiple lines. We split on top-level `;`
            // while respecting parentheses (so `;` inside a multiline value
            // does not trigger).
            foreach ((string declaration, int startIndex) in EnumerateDeclarations(content))
            {
                if (!declaration.Contains("var(--bob-inline-", StringComparison.Ordinal))
                {
                    continue;
                }

                int colon = declaration.IndexOf(':');
                if (colon <= 0)
                {
                    continue;
                }

                string propertyName = declaration[..colon].Trim();
                if (propertyName.StartsWith("--_", StringComparison.Ordinal))
                {
                    continue;
                }

                int line = LineOf(content, startIndex);
                violations.Add($"{fileName}:{line}: '{propertyName}' consumes var(--bob-inline-...) directly (declare a private --_X variable first)");
            }
        }

        violations.Should().BeEmpty(
            because: "the override surface is the private-var pattern: declare --_<comp>-X: var(--bob-inline-Y, default), " +
                     "then reference --_<comp>-X in the actual property. " +
                     string.Join("\n", violations));
    }

    [Fact]
    public void RazorCss_Should_Not_Use_Media_Queries_Outside_Layout_Components()
    {
        string[] files = EnumerateScopedCssFiles().ToArray();
        List<string> violations = [];

        foreach (string file in files)
        {
            if (file.Contains(LayoutFolderToken, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string content = StripComments(File.ReadAllText(file));
            string fileName = Path.GetFileName(file);

            foreach (Match m in MediaQuery.Matches(content))
            {
                int line = LineOf(content, m.Index);
                violations.Add($"{fileName}:{line}: @media query in non-layout component");
            }
        }

        violations.Should().BeEmpty(
            because: "non-layout components resolve sizing via the multiplier system (--bob-size-multiplier / --bob-density-multiplier). " +
                     "@media queries are only allowed in Layout/* components where they switch flow primitives " +
                     "(grid template columns, drawer activation, toast positioning). " +
                     string.Join("\n", violations));
    }

    [Fact]
    public void RazorCss_Should_Not_Contain_Hardcoded_Color_Literals()
    {
        string[] files = EnumerateScopedCssFiles().ToArray();
        List<string> violations = [];

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            if (HardcodedColorAllowlist.ContainsKey(fileName))
            {
                continue;
            }

            string content = StripComments(File.ReadAllText(file));

            foreach (Match m in HardcodedColor.Matches(content))
            {
                int line = LineOf(content, m.Index);
                violations.Add($"{fileName}:{line}: hardcoded color literal '{m.Value}'");
            }
        }

        violations.Should().BeEmpty(
            because: "scoped CSS must consume var(--palette-*) tokens (or color-mix() over them) so theming works. " +
                     "Hardcoded #hex / rgb() / rgba() / hsl() / hsla() literals bypass theme variables. " +
                     string.Join("\n", violations));
    }

    private static string StripComments(string content)
        => CssCommentPattern.Replace(content, string.Empty);

    private static int LineOf(string content, int index)
    {
        int line = 1;
        for (int i = 0; i < index && i < content.Length; i++)
        {
            if (content[i] == '\n')
            {
                line++;
            }
        }

        return line;
    }

    /// <summary>
    /// Yields each top-level CSS declaration along with its starting offset
    /// in <paramref name="content"/>. A declaration is the text up to a
    /// semicolon that lies outside any parentheses or braces. Comments are
    /// expected to be already stripped by the caller.
    /// </summary>
    private static IEnumerable<(string Declaration, int StartIndex)> EnumerateDeclarations(string content)
    {
        int depthParen = 0;
        int depthBrace = 0;
        int start = 0;

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            switch (c)
            {
                case '(':
                    depthParen++;
                    break;
                case ')':
                    if (depthParen > 0)
                    {
                        depthParen--;
                    }

                    break;
                case '{':
                    depthBrace++;
                    // After an opening brace, a fresh declaration begins.
                    start = i + 1;
                    break;
                case '}':
                    if (depthBrace > 0)
                    {
                        depthBrace--;
                    }

                    start = i + 1;
                    break;
                case ';':
                    if (depthParen == 0)
                    {
                        string slice = content[start..i];
                        if (!string.IsNullOrWhiteSpace(slice))
                        {
                            int firstNonWs = 0;
                            while (firstNonWs < slice.Length && char.IsWhiteSpace(slice[firstNonWs]))
                            {
                                firstNonWs++;
                            }

                            yield return (slice.Trim(), start + firstNonWs);
                        }

                        start = i + 1;
                    }

                    break;
            }
        }
    }
}
