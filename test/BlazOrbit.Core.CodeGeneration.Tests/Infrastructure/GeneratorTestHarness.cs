using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BlazOrbit.Core.CodeGeneration.Tests.Infrastructure;

public static class GeneratorTestHarness
{
    public static string Run(
        IIncrementalGenerator generator,
        IEnumerable<string>? sources = null,
        IEnumerable<(string Path, string Content)>? additionalTexts = null,
        IReadOnlyDictionary<string, string>? globalOptions = null,
        IEnumerable<MetadataReference>? extraReferences = null)
    {
        IEnumerable<SyntaxTree> trees = (sources ?? [])
            .Select(s => CSharpSyntaxTree.ParseText(s, new CSharpParseOptions(LanguageVersion.Latest)));

        List<MetadataReference> refs =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.IsExternalInit).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location)
        ];
        string runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        refs.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")));
        refs.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "netstandard.dll")));
        if (extraReferences is not null)
        {
            refs.AddRange(extraReferences);
        }

        CSharpCompilation compilation = CSharpCompilation.Create(
            "GeneratorTests",
            trees,
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        ImmutableArray<AdditionalText> texts = [
            ..(additionalTexts ?? [])
            .Select(t => (AdditionalText)new InMemoryAdditionalText(t.Path, t.Content))
        ];

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [generator.AsSourceGenerator()],
            texts,
            compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
            ?? new CSharpParseOptions(LanguageVersion.Latest),
            globalOptions is null ? null : new FakeAnalyzerConfigOptionsProvider(globalOptions));

        driver = driver.RunGenerators(compilation);
        GeneratorDriverRunResult result = driver.GetRunResult();

        return Format(result);
    }

    private static string Format(GeneratorDriverRunResult result)
    {
        StringBuilder sb = new();
        foreach (GeneratorRunResult gen in result.Results)
        {
            if (!gen.Diagnostics.IsDefaultOrEmpty)
            {
                foreach (Diagnostic d in gen.Diagnostics.OrderBy(x => x.Id).ThenBy(x => x.GetMessage()))
                {
                    sb.Append("// Diagnostic: ").Append(d.Id).Append(' ').AppendLine(d.GetMessage());
                }
            }

            foreach (GeneratedSourceResult src in gen.GeneratedSources.OrderBy(s => s.HintName, StringComparer.Ordinal))
            {
                sb.Append("// ==== ").Append(src.HintName).AppendLine(" ====");
                sb.AppendLine(src.SourceText.ToString().ReplaceLineEndings("\n").TrimEnd());
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd() + "\n";
    }
}

public sealed class InMemoryAdditionalText(string path, string content) : AdditionalText
{
    private readonly SourceText _text = SourceText.From(content, Encoding.UTF8);
    public override string Path { get; } = path;
    public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
}

public sealed class FakeAnalyzerConfigOptionsProvider(IReadOnlyDictionary<string, string> globals)
    : AnalyzerConfigOptionsProvider
{
    private readonly FakeOptions _global = new(globals);
    public override AnalyzerConfigOptions GlobalOptions => _global;
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _global;
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _global;

    private sealed class FakeOptions(IReadOnlyDictionary<string, string> data) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
            => data.TryGetValue(key, out value!);
    }
}