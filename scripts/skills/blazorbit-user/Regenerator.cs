#:property NoWarn=IL2026;IL2070;IL2075
#:property Nullable=disable
// Reflection-based regenerator for the blazorbit-user SKILL references.
//
// Run as a .NET 10 file-based app (no project, no compile step):
//
//     dotnet run scripts/Regenerator.cs -- <core.dll> <main.dll> <refPath> <sharedFrameworkPath>
//
// Arguments:
//   core.dll              BlazOrbit.Core.dll on disk
//   main.dll              BlazOrbit.dll on disk
//   refPath               .agents/skills/blazorbit-user/references/
//   sharedFrameworkPath   Microsoft.AspNetCore.App\<version>\ (probe dir for ComponentBase deps)
//
// Outputs (regenerated, do not hand-edit):
//   references/components.md   component catalog with INHERITED [Parameter]s
//   references/variants.md     variant types + built-in instances
//   references/icons.md        BOBIconKeys catalog
//
// Handcrafted (NOT touched by this script):
//   references/theming.md
//   references/presets.md
//   references/patterns.md
//   references/recipes.md
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

if (args.Length < 4)
{
    Console.Error.WriteLine(
        "Usage: dotnet run Regenerator.cs -- <coreAssembly> <mainAssembly> <refPath> <sharedFrameworkPath>");
    return 1;
}

Regenerator.Run(args[0], args[1], args[2], args[3]);
return 0;

internal class Regenerator
{
    static Assembly _coreAsm;
    static Assembly _mainAsm;
    static Type _componentBaseType;
    static Type _parameterAttrType;
    static Type _variantBaseType;
    static Type _iconKeyType;

    public static void Run(string coreAssemblyPath, string mainAssemblyPath, string refPath, string sharedFrameworkPath)
    {
        var probeDirs = new List<string>
        {
            Path.GetDirectoryName(Path.GetFullPath(mainAssemblyPath)),
        };
        if (!string.IsNullOrWhiteSpace(sharedFrameworkPath) && Directory.Exists(sharedFrameworkPath))
        {
            probeDirs.Add(sharedFrameworkPath);
        }

        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var name = new AssemblyName(args.Name).Name + ".dll";
            foreach (var dir in probeDirs)
            {
                var path = Path.Combine(dir, name);
                if (File.Exists(path))
                {
                    try { return Assembly.LoadFrom(path); } catch { /* fall through */ }
                }
            }
            return null;
        };

        // Pre-load shared framework first so ComponentBase resolves before LoadFrom of our DLLs.
        foreach (var dir in probeDirs)
        {
            foreach (var dll in Directory.GetFiles(dir, "*.dll"))
            {
                try { Assembly.LoadFrom(dll); } catch { /* skip native / mismatched */ }
            }
        }

        _coreAsm = Assembly.LoadFrom(coreAssemblyPath);
        _mainAsm = Assembly.LoadFrom(mainAssemblyPath);

        // Resolve well-known types via late binding to avoid hard refs at compile time.
        _componentBaseType = Type.GetType("Microsoft.AspNetCore.Components.ComponentBase, Microsoft.AspNetCore.Components");
        _parameterAttrType = Type.GetType("Microsoft.AspNetCore.Components.ParameterAttribute, Microsoft.AspNetCore.Components");

        if (_componentBaseType == null || _parameterAttrType == null)
        {
            // Fallback: pick from loaded assemblies.
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (_componentBaseType == null) _componentBaseType = asm.GetType("Microsoft.AspNetCore.Components.ComponentBase");
                if (_parameterAttrType == null) _parameterAttrType = asm.GetType("Microsoft.AspNetCore.Components.ParameterAttribute");
            }
        }
        if (_componentBaseType == null) throw new InvalidOperationException("Cannot resolve ComponentBase.");
        if (_parameterAttrType == null) throw new InvalidOperationException("Cannot resolve ParameterAttribute.");

        _variantBaseType = _coreAsm.GetType("BlazOrbit.Components.Variant");
        if (_variantBaseType == null) _variantBaseType = _mainAsm.GetType("BlazOrbit.Components.Variant");
        _iconKeyType = _coreAsm.GetType("BlazOrbit.Components.IconKey");
        if (_iconKeyType == null) _iconKeyType = _mainAsm.GetType("BlazOrbit.Components.IconKey");

        var components = DiscoverComponents();
        var variants = DiscoverVariants();
        var iconCatalogs = DiscoverIconCatalogs();

        WriteComponentsMd(components, refPath);
        WriteVariantsMd(variants, refPath);
        WriteIconsMd(iconCatalogs, refPath);

        Console.WriteLine();
        Console.WriteLine("Regeneration complete.");
        Console.WriteLine("  Components: " + components.Count);
        Console.WriteLine("  Variants:   " + variants.Count);
        Console.WriteLine("  Icon sets:  " + iconCatalogs.Count);
    }

    // ------------------------------------------------------------------
    //  Component discovery
    // ------------------------------------------------------------------

    static List<ComponentInfo> DiscoverComponents()
    {
        var result = new List<ComponentInfo>();
        var assemblies = new[] { _mainAsm };
        foreach (var asm in assemblies)
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }

            foreach (var t in types)
            {
                if (t == null) continue;
                if (!_componentBaseType.IsAssignableFrom(t)) continue;
                if (t.IsAbstract) continue;
                if (!t.IsPublic && !t.IsNestedPublic) continue;
                if (t.Name.StartsWith("_")) continue;
                if (t.Name.Contains("<")) continue; // anonymous / closure types

                var info = new ComponentInfo
                {
                    Type = t,
                    Name = FriendlyTypeName(t, false, true),
                    Namespace = t.Namespace ?? "",
                    BaseClass = FriendlyTypeName(t.BaseType ?? typeof(object), false, false),
                    Implements = t.GetInterfaces()
                        .Where(i => i.Name.StartsWith("IHas", StringComparison.Ordinal)
                                   || i.Name.EndsWith("FamilyComponent", StringComparison.Ordinal))
                        .Select(i => i.Name)
                        .Distinct()
                        .OrderBy(n => n)
                        .ToList(),
                    Parameters = ExtractParameters(t),
                };
                result.Add(info);
            }
        }
        return result.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    static List<ParamInfo> ExtractParameters(Type t)
    {
        // Walk full inheritance chain so inherited [Parameter]s are surfaced.
        // Dedupe by property name (most-derived declaration wins).
        var seen = new Dictionary<string, ParamInfo>(StringComparer.Ordinal);

        for (var cursor = t; cursor != null && cursor != typeof(object); cursor = cursor.BaseType)
        {
            var props = cursor.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var p in props)
            {
                if (!HasAttribute(p, _parameterAttrType)) continue;
                if (seen.ContainsKey(p.Name)) continue;
                seen[p.Name] = new ParamInfo
                {
                    Name = p.Name,
                    Type = FriendlyTypeName(p.PropertyType, false, false),
                    DeclaredOn = StripGenericArity(cursor.Name),
                };
            }
        }
        return seen.Values.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    // ------------------------------------------------------------------
    //  Variants
    // ------------------------------------------------------------------

    static List<VariantInfo> DiscoverVariants()
    {
        var result = new List<VariantInfo>();
        if (_variantBaseType == null) return result;
        var assemblies = new[] { _coreAsm, _mainAsm };
        foreach (var asm in assemblies)
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(x => x != null).ToArray(); }

            foreach (var t in types)
            {
                if (t == null) continue;
                if (!_variantBaseType.IsAssignableFrom(t)) continue;
                if (t == _variantBaseType) continue;
                if (t.IsAbstract) continue;
                if (!t.IsPublic && !t.IsNestedPublic) continue;

                var builtIns = t.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => t.IsAssignableFrom(f.FieldType))
                    .Select(f => f.Name)
                    .ToList();
                builtIns.AddRange(t.GetProperties(BindingFlags.Public | BindingFlags.Static)
                    .Where(p => t.IsAssignableFrom(p.PropertyType))
                    .Select(p => p.Name));

                bool hasCustom = t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Any(m => m.Name == "Custom" && m.ReturnType == t);

                result.Add(new VariantInfo
                {
                    Name = t.Name,
                    Namespace = t.Namespace ?? "",
                    BuiltInValues = builtIns.Distinct().OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList(),
                    HasCustomFactory = hasCustom,
                });
            }
        }
        return result.OrderBy(v => v.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    // ------------------------------------------------------------------
    //  Icons
    // ------------------------------------------------------------------

    static List<IconCatalog> DiscoverIconCatalogs()
    {
        var result = new List<IconCatalog>();
        if (_iconKeyType == null) return result;

        // BOBIconKeys is partial; nested public classes are the catalogs.
        var iconKeysType = _coreAsm.GetType("BlazOrbit.Components.BOBIconKeys");
        if (iconKeysType == null) iconKeysType = _mainAsm.GetType("BlazOrbit.Components.BOBIconKeys");
        if (iconKeysType == null) return result;

        foreach (var nested in iconKeysType.GetNestedTypes(BindingFlags.Public))
        {
            var entries = nested.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => _iconKeyType.IsAssignableFrom(f.FieldType))
                .Select(f => f.Name)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (entries.Count == 0) continue;
            result.Add(new IconCatalog
            {
                Name = nested.Name,
                Path = "BOBIconKeys." + nested.Name,
                Entries = entries,
            });
        }
        return result.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    // ------------------------------------------------------------------
    //  Markdown writers
    // ------------------------------------------------------------------

    static void WriteComponentsMd(List<ComponentInfo> components, string refPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Component Catalog");
        sb.AppendLine();
        sb.AppendLine("Auto-generated from compiled assemblies. Lists every public component derived");
        sb.AppendLine("from `Microsoft.AspNetCore.Components.ComponentBase` and **all** of its");
        sb.AppendLine("`[Parameter]` properties — including those inherited from `BOBComponentBase`,");
        sb.AppendLine("`BOBVariantComponentBase<,>`, `BOBInputComponentBase<,,>`,");
        sb.AppendLine("`BOBDataCollectionBase<,,>`, and `Microsoft.AspNetCore.Components.Forms.InputBase<>`.");
        sb.AppendLine();
        sb.AppendLine("> Inherited parameters (e.g. `Items`, `Hoverable`, `Disabled`, `ReadOnly`,");
        sb.AppendLine("> `Required`, `Variant`, `Value`, `ValueChanged`, `ValueExpression`) are listed");
        sb.AppendLine("> alongside the component's own. The `Declared on` column tells you which");
        sb.AppendLine("> class introduces each parameter so you can locate the contract.");
        sb.AppendLine();

        foreach (var comp in components)
        {
            sb.Append("## `").Append(comp.Name).AppendLine("`");
            sb.AppendLine();
            sb.Append("- **Namespace**: `").Append(comp.Namespace).AppendLine("`");
            sb.Append("- **Base**: `").Append(comp.BaseClass).AppendLine("`");
            if (comp.Implements.Any())
            {
                sb.Append("- **Implements**: ");
                sb.AppendLine(string.Join(", ", comp.Implements.Select(i => "`" + i + "`")));
            }
            sb.AppendLine();

            if (comp.Parameters.Any())
            {
                sb.AppendLine("### Parameters");
                sb.AppendLine();
                sb.AppendLine("| Parameter | Type | Declared on |");
                sb.AppendLine("|-----------|------|-------------|");
                foreach (var p in comp.Parameters)
                {
                    sb.Append("| `").Append(p.Name).Append("` | `").Append(EscapeMd(p.Type)).Append("` | `").Append(p.DeclaredOn).AppendLine("` |");
                }
                sb.AppendLine();
            }
        }

        File.WriteAllText(Path.Combine(refPath, "components.md"), sb.ToString(), Encoding.UTF8);
        Console.WriteLine("Generated: components.md");
    }

    static void WriteVariantsMd(List<VariantInfo> variants, string refPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Variants");
        sb.AppendLine();
        sb.AppendLine("Auto-generated. Each variant is a sealed subclass of");
        sb.AppendLine("`BlazOrbit.Components.Variant` exposing its built-in instances as static fields");
        sb.AppendLine("and (when `Custom(string)` is defined) a factory for user-named variants.");
        sb.AppendLine();

        foreach (var v in variants)
        {
            sb.Append("## `").Append(v.Name).AppendLine("`");
            sb.AppendLine();
            sb.Append("- **Namespace**: `").Append(v.Namespace).AppendLine("`");
            sb.AppendLine();
            if (v.BuiltInValues.Any())
            {
                sb.AppendLine("### Built-in values");
                sb.AppendLine();
                foreach (var name in v.BuiltInValues)
                    sb.Append("- `").Append(v.Name).Append('.').Append(name).AppendLine("`");
                sb.AppendLine();
            }
            if (v.HasCustomFactory)
            {
                sb.AppendLine("### Custom factory");
                sb.AppendLine();
                sb.AppendLine("```csharp");
                sb.Append(v.Name).AppendLine(".Custom(string name)");
                sb.AppendLine("```");
                sb.AppendLine();
            }
        }

        sb.AppendLine("## Registration");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine("builder.Services.AddBlazOrbitVariants(b =>");
        sb.AppendLine("{");
        sb.AppendLine("    b.ForComponent<BOBButton>()");
        sb.AppendLine("     .AddVariant(BOBButtonVariant.Custom(\"MyVariant\"), MyTemplates.MyVariant);");
        sb.AppendLine("});");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("`MyTemplates.MyVariant` is a `RenderFragment<TComponent>` declared inside a");
        sb.AppendLine("`.razor` file (Razor markup is not legal inside `.cs`). See `patterns.md` and");
        sb.AppendLine("`recipes.md` for the canonical pattern.");

        File.WriteAllText(Path.Combine(refPath, "variants.md"), sb.ToString(), Encoding.UTF8);
        Console.WriteLine("Generated: variants.md");
    }

    static void WriteIconsMd(List<IconCatalog> catalogs, string refPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Icons");
        sb.AppendLine();
        sb.AppendLine("Auto-generated catalog of every `IconKey` exposed under");
        sb.AppendLine("`BlazOrbit.Components.BOBIconKeys`. Always pass `IconKey?` parameters via");
        sb.AppendLine("these statics — `IconKey` has no public constructor consumers should call.");
        sb.AppendLine();
        sb.AppendLine("```razor");
        sb.AppendLine("<BOBButton Text=\"Save\" LeadingIcon=\"@BOBIconKeys.UI.Save\" />");
        sb.AppendLine("<BOBSvgIcon ChildContent=\"@BOBIconKeys.MaterialIconsRound.i_palette\" />");
        sb.AppendLine("```");
        sb.AppendLine();

        // Compact-list catalogs in full (UI, Brands, FileFormats, Materials).
        // Material* catalogs exceed 2000 entries each; truncate them with a
        // sample + a note that names follow the Material Icons kebab-cased
        // identifier prefixed by `i_`.
        const int materialPreviewCount = 80;
        foreach (var cat in catalogs)
        {
            sb.Append("## `").Append(cat.Path).AppendLine("`");
            sb.AppendLine();
            sb.Append("Total: **").Append(cat.Entries.Count).AppendLine(" icons**");
            sb.AppendLine();

            var isHugeCatalog = cat.Name.StartsWith("MaterialIcons", StringComparison.Ordinal);
            var entries = isHugeCatalog
                ? cat.Entries.Take(materialPreviewCount).ToList()
                : cat.Entries;

            if (isHugeCatalog)
            {
                sb.Append("Showing first **").Append(entries.Count).AppendLine(" of the full set**.");
                sb.Append("Field names follow `i_<material-icon-name>` (Material Icons kebab-cased, ");
                sb.AppendLine("with hyphens replaced by `_`). Resolve any name from the official");
                sb.Append("Material Icons catalog: https://fonts.google.com/icons — e.g. ");
                sb.AppendLine("`palette` → `BOBIconKeys.MaterialIconsRound.i_palette`.");
                sb.AppendLine();
            }

            sb.AppendLine("```");
            const int columns = 4;
            for (int i = 0; i < entries.Count; i += columns)
            {
                var slice = entries.Skip(i).Take(columns);
                sb.AppendLine(string.Join(", ", slice));
            }
            if (isHugeCatalog) sb.AppendLine("...");
            sb.AppendLine("```");
            sb.AppendLine();
        }

        File.WriteAllText(Path.Combine(refPath, "icons.md"), sb.ToString(), Encoding.UTF8);
        Console.WriteLine("Generated: icons.md");
    }

    // ------------------------------------------------------------------
    //  Helpers
    // ------------------------------------------------------------------

    static bool HasAttribute(MemberInfo m, Type attrType)
    {
        foreach (var a in m.GetCustomAttributes(false))
            if (attrType.IsInstanceOfType(a)) return true;
        return false;
    }

    static string StripGenericArity(string name)
    {
        var idx = name.IndexOf('`');
        return idx >= 0 ? name.Substring(0, idx) : name;
    }

    static string FriendlyTypeName(Type t, bool includeNamespace, bool openGenerics)
    {
        if (t.IsByRef) t = t.GetElementType();
        if (t == typeof(void)) return "void";

        var underlying = Nullable.GetUnderlyingType(t);
        if (underlying != null) return FriendlyTypeName(underlying, includeNamespace, openGenerics) + "?";

        if (t.IsArray) return FriendlyTypeName(t.GetElementType(), includeNamespace, openGenerics) + "[]";

        var alias = BclAlias(t);
        if (alias != null) return alias;

        if (!t.IsGenericType)
        {
            return includeNamespace ? (t.Namespace + "." + t.Name) : t.Name;
        }

        var genericName = StripGenericArity(t.Name);

        var args = t.GetGenericArguments();
        var argText = string.Join(", ",
            args.Select(a => a.IsGenericParameter && openGenerics
                ? a.Name
                : FriendlyTypeName(a, false, openGenerics)));

        var name = genericName + "<" + argText + ">";
        return includeNamespace ? (t.Namespace + "." + name) : name;
    }

    static string BclAlias(Type t)
    {
        if (t == typeof(string)) return "string";
        if (t == typeof(bool)) return "bool";
        if (t == typeof(int)) return "int";
        if (t == typeof(long)) return "long";
        if (t == typeof(short)) return "short";
        if (t == typeof(byte)) return "byte";
        if (t == typeof(decimal)) return "decimal";
        if (t == typeof(double)) return "double";
        if (t == typeof(float)) return "float";
        if (t == typeof(char)) return "char";
        if (t == typeof(object)) return "object";
        return null;
    }

    static string EscapeMd(string s) { return s.Replace("|", "\\|"); }

    // ------------------------------------------------------------------
    //  DTOs
    // ------------------------------------------------------------------

    class ComponentInfo
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string BaseClass { get; set; }
        public List<string> Implements { get; set; }
        public List<ParamInfo> Parameters { get; set; }

        public ComponentInfo()
        {
            Implements = new List<string>();
            Parameters = new List<ParamInfo>();
            Name = "";
            Namespace = "";
            BaseClass = "";
        }
    }

    class ParamInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string DeclaredOn { get; set; }
        public ParamInfo() { Name = ""; Type = ""; DeclaredOn = ""; }
    }

    class VariantInfo
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<string> BuiltInValues { get; set; }
        public bool HasCustomFactory { get; set; }
        public VariantInfo() { Name = ""; Namespace = ""; BuiltInValues = new List<string>(); }
    }

    class IconCatalog
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<string> Entries { get; set; }
        public IconCatalog() { Name = ""; Path = ""; Entries = new List<string>(); }
    }
}
