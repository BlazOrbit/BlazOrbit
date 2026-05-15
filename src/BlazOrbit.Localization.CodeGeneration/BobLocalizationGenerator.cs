using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using BlazOrbit.Localization.CodeGeneration.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BlazOrbit.Localization.CodeGeneration;

/// <summary>
/// Incremental source generator for the BOBLocalize system.
/// </summary>
/// <remarks>
/// <para>
/// Discovers <c>[assembly: BobLocalizationBundle(typeof(TResource))]</c> attributes on the
/// current compilation and matches them against <c>.tn</c> files declared as
/// <c>AdditionalFiles</c>. For each <c>(TResource, culture)</c> pair found it emits a static
/// dictionary constant. A single <c>[ModuleInitializer]</c> method registers every bundle
/// found in the assembly with <c>BobLocalize.RegisterBundle</c> at assembly load time.
/// </para>
/// <para>
/// File naming convention: <c>&lt;ResourceTypeName&gt;.&lt;culture&gt;.tn</c>, e.g.
/// <c>BOBFormsResources.es-ES.tn</c>. Files without a culture segment (<c>BOBFormsResources.tn</c>)
/// are assigned the bundle's <c>DefaultCulture</c>. Multi-culture catalogues with explicit
/// <c>@ culture</c> headers are also accepted; the headers take precedence over the filename.
/// </para>
/// <para>
/// This is the Phase 3 MVP - sufficient to wire <c>.tn</c> files end-to-end. Call-site
/// rewriting (Interceptors for <c>Loc[literal]</c>) lands in a follow-up phase.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class BobLocalizationGenerator : IIncrementalGenerator
{
    private const string BundleAttributeFullName = "BlazOrbit.Localization.BobLocalizationBundleAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Discover [assembly: BobLocalizationBundle(typeof(TResource))] declarations.
        IncrementalValueProvider<ImmutableArray<BundleDeclaration>> bundles =
            context.CompilationProvider.Select((compilation, ct) =>
            {
                ImmutableArray<BundleDeclaration>.Builder list = ImmutableArray.CreateBuilder<BundleDeclaration>();
                IAssemblySymbol asm = compilation.Assembly;
                foreach (AttributeData attr in asm.GetAttributes())
                {
                    INamedTypeSymbol? attrClass = attr.AttributeClass;
                    if (attrClass is null)
                    {
                        continue;
                    }

                    string fqn = attrClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                        .Replace("global::", "");
                    if (fqn != BundleAttributeFullName)
                    {
                        continue;
                    }

                    if (attr.ConstructorArguments.Length == 0)
                    {
                        continue;
                    }

                    TypedConstant typeArg = attr.ConstructorArguments[0];
                    if (typeArg.Value is not INamedTypeSymbol resourceType)
                    {
                        continue;
                    }

                    string defaultCulture = "en-US";
                    string translationsFolder = "Translations";
                    foreach (KeyValuePair<string, TypedConstant> named in attr.NamedArguments)
                    {
                        if (named.Key == "DefaultCulture" && named.Value.Value is string s && s.Length > 0)
                        {
                            defaultCulture = s;
                        }
                        else if (named.Key == "TranslationsFolder" && named.Value.Value is string f && f.Length > 0)
                        {
                            translationsFolder = f;
                        }
                    }

                    // Capture the using directives in scope where the attribute is written AND
                    // the exact `typeof(...)` text the consumer wrote. The generator prefers the
                    // verbatim source text - it resolves correctly once every generator (Razor
                    // SDK, custom ones) has finished, even when this generator's pass sees a
                    // partially-materialised symbol (Razor-generated partials can report a
                    // wrong namespace before sibling generators complete).
                    ImmutableArray<string>.Builder usingNamespaces = ImmutableArray.CreateBuilder<string>();
                    string verbatimTypeofArg = string.Empty;
                    SyntaxNode? attrSyntax = attr.ApplicationSyntaxReference?.GetSyntax(ct);
                    if (attrSyntax is AttributeSyntax attrNode)
                    {
                        AttributeArgumentSyntax? firstArg = attrNode.ArgumentList?.Arguments.FirstOrDefault();
                        if (firstArg?.Expression is TypeOfExpressionSyntax tofExpr)
                        {
                            verbatimTypeofArg = tofExpr.Type.ToString();
                        }
                    }

                    SyntaxNode? unitNode = attrSyntax?.SyntaxTree.GetRoot(ct);
                    if (unitNode is CompilationUnitSyntax compilationUnit)
                    {
                        foreach (UsingDirectiveSyntax u in compilationUnit.Usings)
                        {
                            if (u.Name is not null && u.Alias is null && u.StaticKeyword.Value is null)
                            {
                                usingNamespaces.Add(u.Name.ToString());
                            }
                        }
                    }

                    // Prefer namespace segments parsed from the consumer-written verbatim text -
                    // Razor-generated partial classes show as global namespace at this generator's
                    // pass, which would empty the symbol's ContainingNamespace and break file
                    // matching. The verbatim arg ("BlazOrbit.Docs.Wasm.Layout.NavMenu") preserves
                    // the dotted path regardless of generator ordering.
                    List<string> namespaceSegments = [];
                    string simpleTypeName;
                    if (!string.IsNullOrEmpty(verbatimTypeofArg) && verbatimTypeofArg.Contains('.'))
                    {
                        string[] tokens = verbatimTypeofArg.Split('.');
                        simpleTypeName = tokens[tokens.Length - 1];
                        for (int i = 0; i < tokens.Length - 1; i++)
                        {
                            namespaceSegments.Add(tokens[i]);
                        }
                    }
                    else
                    {
                        simpleTypeName = resourceType.Name;
                        INamespaceSymbol? ns = resourceType.ContainingNamespace;
                        while (ns != null && !ns.IsGlobalNamespace)
                        {
                            namespaceSegments.Insert(0, ns.Name);
                            ns = ns.ContainingNamespace;
                        }
                    }

                    list.Add(new BundleDeclaration(
                        resourceType, defaultCulture, translationsFolder, simpleTypeName,
                        verbatimTypeofArg,
                        usingNamespaces.ToImmutable(),
                        [..namespaceSegments]));
                }

                return list.ToImmutable();
            });

        // Collect .tn AdditionalFiles. Path stored relative to project root.
        IncrementalValuesProvider<TnFile> tnFiles = context.AdditionalTextsProvider
            .Where(t => t.Path.EndsWith(".tn", StringComparison.OrdinalIgnoreCase))
            .Select((t, ct) =>
            {
                string name = Path.GetFileNameWithoutExtension(t.Path);
                string? text = t.GetText(ct)?.ToString();
                return new TnFile(t.Path, name, text ?? string.Empty);
            });

        IncrementalValueProvider<ImmutableArray<TnFile>> tnFilesArr = tnFiles.Collect();

        // Combine + emit.
        IncrementalValueProvider<(ImmutableArray<BundleDeclaration> Bundles, ImmutableArray<TnFile> Files)>
            combined = bundles.Combine(tnFilesArr);

        context.RegisterSourceOutput(combined, (spc, pair) => Emit(spc, pair.Bundles, pair.Files));
    }

    private static void Emit(
        SourceProductionContext spc,
        ImmutableArray<BundleDeclaration> bundles,
        ImmutableArray<TnFile> tnFiles)
    {
        if (bundles.IsDefaultOrEmpty)
        {
            return;
        }

        // Aggregate consumer-side `using` directives once - Razor partials only resolve their
        // final namespace after every generator has run, so re-emitting the consumer's usings in
        // each generated file keeps `typeof(...)` references resolvable across generator passes.
        HashSet<string> alreadyEmitted = new(StringComparer.Ordinal)
        {
            "System",
            "System.Collections.Frozen",
            "System.Collections.Generic",
            "System.Runtime.CompilerServices",
            "BlazOrbit.Localization",
            "BlazOrbit.Localization.Providers"
        };
        SortedSet<string> consumerUsings = new(StringComparer.Ordinal);
        foreach (BundleDeclaration b in bundles)
        {
            foreach (string ns in b.UsingNamespaces)
            {
                if (alreadyEmitted.Add(ns))
                {
                    consumerUsings.Add(ns);
                }
            }
        }

        // Deduplicate hint-name suffix when two bundles share a simple type name (e.g. two
        // `NavMenu` markers in different namespaces). Counter keyed by simple name.
        Dictionary<string, int> nameCollision = new(StringComparer.Ordinal);

        int bundleIndex = 0;
        foreach (BundleDeclaration bundle in bundles)
        {
            // Prefer the consumer-written typeof argument verbatim - once every generator
            // (Razor SDK, custom) has produced its output the C# compiler resolves it the
            // same way it would resolve the original attribute. Falls back to namespace-walk
            // when the syntax wasn't recovered (defensive - should not happen in practice).
            string typeFqn = !string.IsNullOrEmpty(bundle.VerbatimTypeofArg)
                ? bundle.VerbatimTypeofArg
                : bundle.NamespaceSegments.Length > 0
                    ? string.Join(".", bundle.NamespaceSegments) + "." + bundle.SimpleTypeName
                    : bundle.SimpleTypeName;

            // Aggregate translations from matching .tn files.
            Dictionary<string, Dictionary<ulong, string>> aggregated =
                new(StringComparer.OrdinalIgnoreCase);

            foreach (TnFile file in tnFiles.OrderBy(f => f.Path, StringComparer.OrdinalIgnoreCase))
            {
                if (!MatchesBundle(file.Path, bundle, out string fileCulture))
                {
                    continue;
                }

                string defaultCulture = string.IsNullOrEmpty(fileCulture) ? bundle.DefaultCulture : fileCulture;
                TnParseResult parsed = TnParser.Parse(file.Content, defaultCulture);

                foreach (TnDiagnostic diag in parsed.Diagnostics)
                {
                    DiagnosticSeverity sev = diag.Severity switch
                    {
                        TnDiagnosticSeverity.Error => DiagnosticSeverity.Error,
                        TnDiagnosticSeverity.Warning => DiagnosticSeverity.Warning,
                        _ => DiagnosticSeverity.Info
                    };
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            diag.Severity == TnDiagnosticSeverity.Error ? "BL0002" : "BL0001",
                            ".tn parse",
                            diag.Message + " (" + file.Path + ":" + diag.LineNumber + ")",
                            "BobLocalize",
                            sev,
                            true),
                        null));
                }

                foreach (KeyValuePair<string, Dictionary<ulong, string>> cultureKv in parsed.Translations)
                {
                    if (!aggregated.TryGetValue(cultureKv.Key, out Dictionary<ulong, string>? byHash))
                    {
                        byHash = new Dictionary<ulong, string>();
                        aggregated[cultureKv.Key] = byHash;
                    }

                    foreach (KeyValuePair<ulong, string> entry in cultureKv.Value)
                    {
                        byHash[entry.Key] = entry.Value;
                    }
                }
            }

            // One file per bundle - keeps each generated source small and easy to inspect.
            // Method name is unique per bundle (uses index suffix); file name is unique even
            // when two bundles share a simple type name (e.g. NavMenu in two namespaces).
            string methodSuffix = "_b" + bundleIndex;
            string simpleName = SanitizeIdentifier(bundle.SimpleTypeName);
            int collision = nameCollision.TryGetValue(simpleName, out int prior) ? prior + 1 : 0;
            nameCollision[simpleName] = collision;
            string hintBase = collision == 0 ? simpleName : simpleName + "_" + collision;

            StringBuilder sb = new();
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// Generated by BlazOrbit.Localization.CodeGeneration. Do not edit.");
            sb.AppendLine("// Bundle: " + typeFqn);
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Frozen;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using BlazOrbit.Localization;");
            sb.AppendLine("using BlazOrbit.Localization.Providers;");
            foreach (string ns in consumerUsings)
            {
                sb.Append("using ").Append(ns).AppendLine(";");
            }

            sb.AppendLine();
            sb.AppendLine("namespace BlazOrbit.Localization.Generated");
            sb.AppendLine("{");
            sb.AppendLine("    internal static partial class _BobLocalizeBootstrap");
            sb.AppendLine("    {");
            sb.AppendLine("        [ModuleInitializer]");
            sb.AppendLine("        internal static void Register" + methodSuffix + "()");
            sb.AppendLine("        {");

            int cultureIndex = 0;
            List<(string Name, string FieldName)> cultureFields = [];
            foreach (KeyValuePair<string, Dictionary<ulong, string>> cultureKv in aggregated
                         .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
            {
                string field = "c" + cultureIndex + "Translations";
                cultureFields.Add((cultureKv.Key, field));
                sb.Append("            var ").Append(field).Append(" = new Dictionary<ulong, string>");
                sb.AppendLine();
                sb.AppendLine("            {");
                foreach (KeyValuePair<ulong, string> kv in cultureKv.Value
                             .OrderBy(kv2 => kv2.Key))
                {
                    sb.Append("                [").Append("0x").Append(kv.Key.ToString("X16"))
                        .Append("UL] = ").Append(TripleQuote(kv.Value)).AppendLine(",");
                }

                sb.AppendLine("            }.ToFrozenDictionary();");
                cultureIndex++;
            }

            sb.Append("            FrozenDictionary<string, FrozenDictionary<ulong, string>>? allTranslations = ");
            if (cultureFields.Count == 0)
            {
                sb.AppendLine("null;");
            }
            else
            {
                sb.AppendLine(
                    "new Dictionary<string, FrozenDictionary<ulong, string>>(StringComparer.OrdinalIgnoreCase)");
                sb.AppendLine("            {");
                foreach ((string name, string fieldName) in cultureFields)
                {
                    sb.Append("                [").Append(Quote(name)).Append("] = ")
                        .Append(fieldName).AppendLine(",");
                }

                sb.AppendLine("            }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);");
            }

            // SourceLiterals is populated by the call-site scanner in a follow-up phase. Emit
            // an empty FrozenDictionary so downstream code paths (LiteralProvider, GetAllStrings)
            // can rely on the field being non-null when the generator runs at all.
            sb.AppendLine(
                "            FrozenDictionary<ulong, string>? sourceLiterals = FrozenDictionary<ulong, string>.Empty;");

            sb.AppendLine("            BobLocalize.RegisterBundle(new BobLocalizationBundleSpec(");
            sb.Append("                ResourceType: typeof(").Append(typeFqn).AppendLine("),");
            sb.Append("                DefaultCulture: ").Append(Quote(bundle.DefaultCulture)).AppendLine(",");
            sb.AppendLine(
                "                Chain: new System.Type[] { typeof(BundleProvider), typeof(LiteralProvider) },");
            sb.AppendLine("                KeyRoutes: System.Array.Empty<BobLocalizationKeyRoute>(),");
            sb.AppendLine("                Translations: allTranslations,");
            sb.AppendLine("                SourceLiterals: sourceLiterals));");

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            spc.AddSource("BobLocalize.Bundle." + hintBase + ".g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            bundleIndex++;
        }
    }

    private static string SanitizeIdentifier(string raw)
    {
        StringBuilder sb = new(raw.Length);
        foreach (char c in raw)
        {
            sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
        }

        return sb.ToString();
    }

    private static string BuildFullyQualifiedName(INamedTypeSymbol type)
    {
        // Walk containing types first (for nested types), then containing namespaces.
        Stack<string> parts = new();
        parts.Push(type.Name);

        INamedTypeSymbol? container = type.ContainingType;
        while (container != null)
        {
            parts.Push(container.Name);
            container = container.ContainingType;
        }

        INamespaceSymbol? ns = type.ContainingNamespace;
        List<string> nsParts = [];
        while (ns != null && !ns.IsGlobalNamespace)
        {
            nsParts.Add(ns.Name);
            ns = ns.ContainingNamespace;
        }

        nsParts.Reverse();

        StringBuilder sb = new("global::");
        for (int i = 0; i < nsParts.Count; i++)
        {
            if (i > 0)
            {
                sb.Append('.');
            }

            sb.Append(nsParts[i]);
        }

        if (nsParts.Count > 0 && parts.Count > 0)
        {
            sb.Append('.');
        }

        bool first = true;
        foreach (string part in parts)
        {
            if (!first)
            {
                sb.Append('.');
            }

            sb.Append(part);
            first = false;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Matches a `.tn` file path against a bundle. Two acceptable layouts (both equivalent
    /// in semantics so the consumer can pick what reads better in their tree):
    ///
    /// 1. Hierarchical folder layout (mirrors namespace structure):
    ///      &lt;TranslationsFolder&gt;/Layout/NavMenu.tn
    ///      &lt;TranslationsFolder&gt;/Layout/NavMenu.es-ES.tn
    ///    Native-resx-equivalent: the folder hierarchy mirrors the marker type's containing
    ///    namespace below the assembly root namespace.
    ///
    /// 2. Dot-separated single-file layout (all .tn files at one level):
    ///      &lt;TranslationsFolder&gt;/Layout.NavMenu.tn
    ///      &lt;TranslationsFolder&gt;/Layout.NavMenu.es-ES.tn
    ///    Useful when a project has many bundles and a flat list is easier to scan.
    ///
    /// Both layouts disambiguate by namespace so two `NavMenu` types in different namespaces
    /// can ship distinct `.tn` files without collision.
    /// </summary>
    private static bool MatchesBundle(string filePath, BundleDeclaration bundle, out string fileCulture)
    {
        fileCulture = string.Empty;

        // Normalise the path: replace separators with `.`, strip `.tn`, drop the configured
        // translations folder prefix wherever it appears in the chain. Operate on the path's
        // tail (the file-portion after `<TranslationsFolder>`).
        string normalised = filePath.Replace('\\', '/');
        int folderIndex =
            normalised.LastIndexOf("/" + bundle.TranslationsFolder + "/", StringComparison.OrdinalIgnoreCase);
        string tail;
        if (folderIndex >= 0)
        {
            tail = normalised.Substring(folderIndex + bundle.TranslationsFolder.Length + 2);
        }
        else if (normalised.StartsWith(bundle.TranslationsFolder + "/", StringComparison.OrdinalIgnoreCase))
        {
            tail = normalised.Substring(bundle.TranslationsFolder.Length + 1);
        }
        else
        {
            // Not inside the configured folder - skip.
            return false;
        }

        if (!tail.EndsWith(".tn", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        tail = tail.Substring(0, tail.Length - 3);

        // Convert folder separators to dots so both layouts collapse to the same canonical form:
        //   "Layout/NavMenu.es-ES"   →  "Layout.NavMenu.es-ES"
        //   "Layout.NavMenu.es-ES"   →  "Layout.NavMenu.es-ES"
        string canonical = tail.Replace('/', '.');

        // Build the expected prefix from the bundle's namespace segments + type name.
        // Marker `BlazOrbit.Docs.Wasm.Layout.NavMenu` with assembly root `BlazOrbit.Docs.Wasm`:
        //   namespaceSegments = ["BlazOrbit", "Docs", "Wasm", "Layout"]
        //   typeName          = "NavMenu"
        //   Expected canonical: "Layout.NavMenu" (or anywhere ending in the segments+name).
        //
        // We accept any suffix-match against `<...>.{Segments[...]}.{TypeName}` so the consumer
        // doesn't have to know whether to include the root namespace prefix.
        string typeName = bundle.SimpleTypeName;
        string typePathFull = bundle.NamespaceSegments.Length == 0
            ? typeName
            : string.Join(".", bundle.NamespaceSegments) + "." + typeName;

        // Try, longest-first, suffixes of the expected path. This lets a consumer who omits
        // their root namespace ("Layout.NavMenu") still match while one who supplies it
        // ("BlazOrbit.Docs.Wasm.Layout.NavMenu") also matches.
        for (int skip = 0; skip <= bundle.NamespaceSegments.Length; skip++)
        {
            string[] segs = bundle.NamespaceSegments.Skip(skip).ToArray();
            string expected = segs.Length == 0 ? typeName : string.Join(".", segs) + "." + typeName;

            if (canonical.Equals(expected, StringComparison.OrdinalIgnoreCase))
            {
                return true; // no culture suffix
            }

            if (canonical.StartsWith(expected + ".", StringComparison.OrdinalIgnoreCase))
            {
                fileCulture = canonical.Substring(expected.Length + 1);
                return true;
            }
        }

        return false;
    }

    private static string TripleQuote(string value) => $"\"\"\"{value}\"\"\"";

    private static string Quote(string value)
    {
        StringBuilder sb = new("\"");
        foreach (char c in value)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\r': sb.Append("\\r"); break;
                case '\n': sb.Append("\\n"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 32 || c == 127)
                    {
                        sb.Append("\\u").Append(((int)c).ToString("X4"));
                    }
                    else
                    {
                        sb.Append(c);
                    }

                    break;
            }
        }

        sb.Append('"');
        return sb.ToString();
    }

    private sealed class BundleDeclaration
    {
        public BundleDeclaration(
            INamedTypeSymbol resourceType,
            string defaultCulture,
            string translationsFolder,
            string simpleTypeName,
            string verbatimTypeofArg,
            ImmutableArray<string> usingNamespaces,
            ImmutableArray<string> namespaceSegments)
        {
            ResourceType = resourceType;
            DefaultCulture = defaultCulture;
            TranslationsFolder = translationsFolder;
            SimpleTypeName = simpleTypeName;
            VerbatimTypeofArg = verbatimTypeofArg;
            UsingNamespaces = usingNamespaces;
            NamespaceSegments = namespaceSegments;
        }

        public INamedTypeSymbol ResourceType { get; }
        public string DefaultCulture { get; }
        public string TranslationsFolder { get; }
        public string SimpleTypeName { get; }

        /// <summary>Verbatim text inside <c>typeof(...)</c> from the consumer's source.</summary>
        public string VerbatimTypeofArg { get; }

        public ImmutableArray<string> UsingNamespaces { get; }

        /// <summary>Containing namespace split into segments (closer-to-root first).</summary>
        public ImmutableArray<string> NamespaceSegments { get; }
    }

    private sealed class TnFile
    {
        public TnFile(string path, string name, string content)
        {
            Path = path;
            Name = name;
            Content = content;
        }

        public string Path { get; }
        public string Name { get; }
        public string Content { get; }
    }
}