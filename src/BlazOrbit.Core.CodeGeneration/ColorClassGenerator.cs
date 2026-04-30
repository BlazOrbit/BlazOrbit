// BlazOrbit.Core\SourceGenerators\ColorClassGenerator.cs
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace BlazOrbit.Core.CodeGeneration;

/// <summary>
/// Roslyn incremental source generator that expands <c>[AutogenerateCssColors]</c>-attributed
/// classes into per-color nested partial classes at compile time.
/// </summary>
[Generator]
public class ColorClassGenerator : IIncrementalGenerator
{
    internal static readonly DiagnosticDescriptor MustBePartialStaticRule = new(
        id: "BOBGEN010",
        title: "AutogenerateCssColorsAttribute requires a partial static class",
        messageFormat: "Class '{0}' is decorated with [AutogenerateCssColors] but is not declared as 'public static partial'. Add the missing modifier(s).",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The ColorClassGenerator emits per-color nested classes as a partial extension of the target type; the target must therefore be declared 'static partial' so the generated file can merge with the user-authored source.");

    private readonly record struct NamedColor(string Name, byte R, byte G, byte B, byte A);

    /// <summary>Stable, cache-friendly carrier for a <see cref="Location"/>.</summary>
    internal readonly record struct LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
    {
        public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);
    }

    private static readonly NamedColor[] _colors = typeof(Color)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(p => p.PropertyType == typeof(Color))
        .Select(p =>
        {
            Color c = (Color)p.GetValue(null)!;
            return new NamedColor(p.Name, c.R, c.G, c.B, c.A);
        })
        .ToArray();

    /// <summary>
    /// Fully-qualified metadata name of the <c>[AutogenerateCssColors]</c> attribute.
    /// </summary>
    public const string AttributeMetadataName = "BlazOrbit.Components.AutogenerateCssColorsAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassToGenerate> classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeMetadataName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => GetSemanticTargetFromAttribute(ctx, ct))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value);

        context.RegisterSourceOutput(classDeclarations.Collect(), static (spc, source) => Execute(source, spc));
    }

    private readonly record struct ClassToGenerate(
        string NamespaceName,
        string ClassName,
        int VariantLevels,
        bool IsStatic,
        bool IsPartial,
        LocationInfo Location);

    private static ClassToGenerate? GetSemanticTargetFromAttribute(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.TargetNode is not ClassDeclarationSyntax classDeclaration)
        {
            return null;
        }

        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        // ForAttributeWithMetadataName guarantees Attributes contains at least one match.
        AttributeData attribute = context.Attributes[0];

        int variantLevels = 5;
        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is int levels &&
            levels > 0)
        {
            variantLevels = levels;
        }

        bool isStatic = classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword);
        bool isPartial = classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);

        SyntaxTree tree = classDeclaration.SyntaxTree;
        TextSpan span = classDeclaration.Identifier.Span;
        LocationInfo location = new(
            tree.FilePath,
            span,
            tree.GetLineSpan(span).Span);

        return new ClassToGenerate(
            classSymbol.ContainingNamespace.ToDisplayString(),
            classSymbol.Name,
            variantLevels,
            isStatic,
            isPartial,
            location);
    }

    private static void Execute(ImmutableArray<ClassToGenerate> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        CancellationToken ct = context.CancellationToken;

        foreach (ClassToGenerate classToGenerate in classes.Distinct())
        {
            ct.ThrowIfCancellationRequested();

            if (!classToGenerate.IsStatic || !classToGenerate.IsPartial)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MustBePartialStaticRule,
                    classToGenerate.Location.ToLocation(),
                    classToGenerate.ClassName));
                continue;
            }

            string sourceCode = BuildSource(classToGenerate, ct);
            context.AddSource($"{classToGenerate.ClassName}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private static string BuildSource(ClassToGenerate classToGenerate, CancellationToken ct)
    {
        int variantLevels = classToGenerate.VariantLevels;
        string namespaceName = classToGenerate.NamespaceName;
        string className = classToGenerate.ClassName;

        StringBuilder sb = new(capacity: 64 * 1024);
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using BlazOrbit.Components;");
        sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine("[ExcludeFromCodeCoverage]\r\n");
        sb.AppendLine($"public static partial class {className}");
        sb.AppendLine("{\r\n");

        bool first = true;
        foreach (NamedColor color in _colors)
        {
            ct.ThrowIfCancellationRequested();

            if (!first)
            {
                sb.Append("\r\n");
            }

            first = false;

            sb.Append("    public static class ").Append(color.Name).Append("\r\n");
            sb.Append("    {\r\n");
            AppendProperty(sb, "Default", color, variant: null);
            for (int i = 1; i <= variantLevels; i++)
            {
                AppendProperty(sb, $"Darken{i}", color, variant: $"CssColorVariant.Darken({i})");
            }

            for (int i = 1; i <= variantLevels; i++)
            {
                AppendProperty(sb, $"Lighten{i}", color, variant: $"CssColorVariant.Lighten({i})");
            }

            sb.Append("    }\r\n");
        }

        sb.Append("}\r\n");
        return sb.ToString();
    }

    private static void AppendProperty(StringBuilder sb, string name, NamedColor color, string? variant)
    {
        sb.Append("            public static CssColor ").Append(name).Append(" { get; } = new CssColor(")
            .Append(color.R).Append(", ")
            .Append(color.G).Append(", ")
            .Append(color.B).Append(", ")
            .Append(color.A);
        if (variant != null)
        {
            sb.Append(", ").Append(variant);
        }

        sb.Append(");\r\n");
    }
}