using System.Collections.Immutable;
using BlazOrbit.Localization.CodeGeneration;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace BlazOrbit.Tests.Integration.Tests.Localization;

/// <summary>
/// Drives <see cref="BobLocalizationGenerator"/> through Roslyn's
/// <see cref="CSharpGeneratorDriver"/> so we can assert on the emitted code without spinning
/// up a separate consumer project.
/// </summary>
[Trait("Component Integration", "BobLocalizationGenerator")]
public class BobLocalizationGeneratorTests
{
    private const string MinimalAttributeSource = """
                                                  namespace BlazOrbit.Localization
                                                  {
                                                      [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
                                                      public sealed class BobLocalizationBundleAttribute : System.Attribute
                                                      {
                                                          public System.Type ResourceType { get; }
                                                          public string DefaultCulture { get; set; } = "en-US";
                                                          public string TranslationsFolder { get; set; } = "Translations";
                                                          public BobLocalizationBundleAttribute(System.Type resourceType) => ResourceType = resourceType;
                                                      }
                                                  }
                                                  """;

    [Fact]
    public void Should_Emit_Bootstrap_When_Bundle_Attribute_Present()
    {
        string consumer = """
                          [assembly: BlazOrbit.Localization.BobLocalizationBundleAttribute(typeof(MyApp.MyAppResources))]

                          namespace MyApp
                          {
                              public sealed class MyAppResources { }
                          }
                          """;

        GeneratorDriverRunResult result = RunGenerator(consumer, []);

        result.Diagnostics.Should().BeEmpty();
        // One file per bundle. Hint name embeds the marker's simple type name so two markers
        // sharing a simple name across namespaces still produce distinct files.
        result.GeneratedTrees.Should().ContainSingle(t =>
            t.FilePath.EndsWith("BobLocalize.Bundle.MyAppResources.g.cs"));

        string source = result.GeneratedTrees.Single().ToString();
        source.Should().Contain("_BobLocalizeBootstrap");
        source.Should().Contain("[ModuleInitializer]");
        // Generator emits the verbatim typeof argument from the consumer's source
        // — `MyApp.MyAppResources` in this test. Once Razor SDK and other generators
        // have finished, the C# compiler resolves the reference identically to how it
        // resolved the original attribute application.
        source.Should().Contain("typeof(MyApp.MyAppResources)");
        source.Should().Contain("BobLocalize.RegisterBundle");
    }

    [Fact]
    public void Should_Not_Emit_Anything_When_No_Bundle_Attribute()
    {
        string consumer = """
                          namespace MyApp
                          {
                              public sealed class Untagged { }
                          }
                          """;

        GeneratorDriverRunResult result = RunGenerator(consumer, []);
        result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Should_Bake_Translations_From_Matching_Tn_File()
    {
        string consumer = """
                          [assembly: BlazOrbit.Localization.BobLocalizationBundleAttribute(typeof(MyApp.SampleResources))]

                          namespace MyApp
                          {
                              public sealed class SampleResources { }
                          }
                          """;

        string tn = """
                    # Hello
                    Hola

                    # Goodbye
                    Adiós
                    """;

        GeneratorDriverRunResult result = RunGenerator(
            consumer,
            [("Translations/SampleResources.es-ES.tn", tn)]);

        string source = ConcatAll(result);
        source.Should().Contain("\"Hola\"");
        source.Should().Contain("\"Adiós\"");
        source.Should().Contain("\"es-ES\"");
    }

    [Fact]
    public void Should_Honor_DefaultCulture_Argument_For_Tn_Without_Culture_Suffix()
    {
        string consumer = """
                          [assembly: BlazOrbit.Localization.BobLocalizationBundleAttribute(
                              typeof(MyApp.BareResources),
                              DefaultCulture = "fr-FR")]

                          namespace MyApp
                          {
                              public sealed class BareResources { }
                          }
                          """;

        string tn = """
                    # Hello
                    Bonjour
                    """;

        GeneratorDriverRunResult result = RunGenerator(
            consumer,
            [("Translations/BareResources.tn", tn)]);

        string source = ConcatAll(result);
        source.Should().Contain("\"fr-FR\"");
        source.Should().Contain("\"Bonjour\"");
    }

    [Fact]
    public void Should_Skip_Tn_File_Not_Matching_Bundle_Name()
    {
        string consumer = """
                          [assembly: BlazOrbit.Localization.BobLocalizationBundleAttribute(typeof(MyApp.AppResources))]

                          namespace MyApp { public sealed class AppResources { } }
                          """;

        string unrelatedTn = """
                             # Hello
                             Hola
                             """;

        GeneratorDriverRunResult result = RunGenerator(
            consumer,
            [("Translations/UnrelatedResources.es-ES.tn", unrelatedTn)]);

        string source = ConcatAll(result);
        source.Should().NotContain("\"Hola\"");
    }

    [Fact]
    public void Should_Surface_Parse_Warning_As_Diagnostic()
    {
        string consumer = """
                          [assembly: BlazOrbit.Localization.BobLocalizationBundleAttribute(typeof(MyApp.WarnResources))]

                          namespace MyApp { public sealed class WarnResources { } }
                          """;

        // Duplicate key triggers BL0001.
        string tn = """
                    # Hello
                    First

                    # Hello
                    Second
                    """;

        GeneratorDriverRunResult result = RunGenerator(
            consumer,
            [("Translations/WarnResources.es-ES.tn", tn)]);

        result.Diagnostics.Should().ContainSingle(d => d.Id == "BL0001");
    }

    [Fact]
    public void Should_Match_Hierarchical_Folder_Layout()
    {
        // Translations/Layout/NavMenu.es-ES.tn — folder hierarchy mirrors namespace.
        string consumer = """
                          [assembly: BlazOrbit.Localization.BobLocalizationBundleAttribute(typeof(MyApp.Layout.NavMenu))]

                          namespace MyApp.Layout { public sealed class NavMenu { } }
                          """;

        GeneratorDriverRunResult result = RunGenerator(
            consumer,
            [("Translations/Layout/NavMenu.es-ES.tn", "# Hello\nHola")]);

        string source = ConcatAll(result);
        source.Should().Contain("\"Hola\"");
        source.Should().Contain("\"es-ES\"");
    }

    [Fact]
    public void Should_Match_Dot_Separated_Filename_Layout()
    {
        // Translations/Layout.NavMenu.es-ES.tn — single folder, dotted filename.
        string consumer = """
                          [assembly: BlazOrbit.Localization.BobLocalizationBundleAttribute(typeof(MyApp.Layout.NavMenu))]

                          namespace MyApp.Layout { public sealed class NavMenu { } }
                          """;

        GeneratorDriverRunResult result = RunGenerator(
            consumer,
            [("Translations/Layout.NavMenu.es-ES.tn", "# Hello\nHola")]);

        string source = ConcatAll(result);
        source.Should().Contain("\"Hola\"");
    }

    [Fact]
    public void Should_Honor_Custom_TranslationsFolder()
    {
        string consumer = """
                          [assembly: BlazOrbit.Localization.BobLocalizationBundleAttribute(
                              typeof(MyApp.Custom),
                              TranslationsFolder = "I18n")]

                          namespace MyApp { public sealed class Custom { } }
                          """;

        GeneratorDriverRunResult result = RunGenerator(
            consumer,
            [
                ("Translations/Custom.es-ES.tn", "# Hello\nWRONG"),
                ("I18n/Custom.es-ES.tn", "# Hello\nHola")
            ]);

        string source = ConcatAll(result);
        source.Should().Contain("\"Hola\"");
        source.Should().NotContain("\"WRONG\"");
    }

    [Fact]
    public void Should_Disambiguate_Same_Type_Name_In_Different_Namespaces()
    {
        // Two NavMenu types in different namespaces — each gets its own .tn file.
        string consumer = """
                          [assembly: BlazOrbit.Localization.BobLocalizationBundleAttribute(typeof(MyApp.Layout.NavMenu))]
                          [assembly: BlazOrbit.Localization.BobLocalizationBundleAttribute(typeof(MyApp.Admin.NavMenu))]

                          namespace MyApp.Layout { public sealed class NavMenu { } }
                          namespace MyApp.Admin  { public sealed class NavMenu { } }
                          """;

        GeneratorDriverRunResult result = RunGenerator(
            consumer,
            [
                ("Translations/Layout/NavMenu.es-ES.tn", "# Hello\nHola Layout"),
                ("Translations/Admin/NavMenu.es-ES.tn", "# Hello\nHola Admin")
            ]);

        string source = ConcatAll(result);
        source.Should().Contain("\"Hola Layout\"");
        source.Should().Contain("\"Hola Admin\"");
        // Two distinct files even though both markers share the simple name `NavMenu`.
        result.GeneratedTrees.Should().HaveCount(2);
    }

    [Fact]
    public void Should_Aggregate_Multiple_Cultures_For_Same_Bundle()
    {
        string consumer = """
                          [assembly: BlazOrbit.Localization.BobLocalizationBundleAttribute(typeof(MyApp.MultiResources))]
                          namespace MyApp { public sealed class MultiResources { } }
                          """;

        GeneratorDriverRunResult result = RunGenerator(
            consumer,
            [
                ("Translations/MultiResources.es-ES.tn", "# Hello\nHola"),
                ("Translations/MultiResources.fr-FR.tn", "# Hello\nBonjour"),
                ("Translations/MultiResources.ja-JP.tn", "# Hello\nこんにちは")
            ]);

        string source = ConcatAll(result);
        source.Should().Contain("\"es-ES\"");
        source.Should().Contain("\"fr-FR\"");
        source.Should().Contain("\"ja-JP\"");
        source.Should().Contain("\"Hola\"");
        source.Should().Contain("\"Bonjour\"");
        // Unicode either passes through literal or escaped — Quote() emits literal for >= U+0020.
        source.Should().Contain("こんにちは");
    }

    /// <summary>Concatenates every generated tree's source — useful when the generator splits its
    /// output across one-file-per-bundle and the test wants a single haystack to search.</summary>
    private static string ConcatAll(GeneratorDriverRunResult result)
        => string.Join("\n", result.GeneratedTrees.Select(t => t.ToString()));

    private static GeneratorDriverRunResult RunGenerator(
        string consumerSource,
        IReadOnlyList<(string Name, string Content)> additionalFiles)
    {
        SyntaxTree attrTree = CSharpSyntaxTree.ParseText(MinimalAttributeSource);
        SyntaxTree consumerTree = CSharpSyntaxTree.ParseText(consumerSource);

        ImmutableArray<MetadataReference> refs = [
            ..AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .Cast<MetadataReference>()
        ];

        Compilation compilation = CSharpCompilation.Create(
            "TestConsumer",
            [attrTree, consumerTree],
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        ImmutableArray<AdditionalText> additionalTexts = [
            ..additionalFiles
                .Select(f => (AdditionalText)new InMemoryAdditionalText(f.Name, f.Content))
        ];

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new BobLocalizationGenerator().AsSourceGenerator()],
            additionalTexts);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        return driver.GetRunResult();
    }

    private sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly string _content;

        public InMemoryAdditionalText(string path, string content)
        {
            Path = path;
            _content = content;
        }

        public override string Path { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default)
            => SourceText.From(_content);
    }
}