using BlazOrbit.Docs.CodeGeneration.Tests.Infrastructure;

namespace BlazOrbit.Docs.CodeGeneration.Tests.Tests;

[Trait("Generator Snapshots", "DocDemoGenerator")]
public class DocDemoGeneratorTests
{
    [Fact]
    public async Task Should_Skip_When_No_DocDemo()
    {
        string razor = """
            @namespace Docs.Pages

            <h1>Nothing to see</h1>
            """;

        string output = GeneratorTestHarness.Run(
            new DocDemoGenerator(),
            additionalTexts: [("/docs/Pages/Empty.razor", razor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Generate_For_Basic_DocDemo()
    {
        string razor = """
            @namespace Docs.Pages

            <DocDemo Key="default">
                <BOBButton Text="Hi" />
            </DocDemo>

            <DocDemo Key="disabled">
                <BOBButton Text="Hi" Disabled="true" />
            </DocDemo>
            """;

        string output = GeneratorTestHarness.Run(
            new DocDemoGenerator(),
            additionalTexts: [("/docs/Pages/ButtonPage.razor", razor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Ignore_SelfClosing_And_KeyLess_DocDemos()
    {
        string razor = """
            @namespace Docs.Pages

            <DocDemo Key="kept">
                <span>A</span>
            </DocDemo>

            <DocDemo Key="selfClose" />

            <DocDemo>
                <span>unnamed</span>
            </DocDemo>
            """;

        string output = GeneratorTestHarness.Run(
            new DocDemoGenerator(),
            additionalTexts: [("/docs/Pages/MixedPage.razor", razor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Only_Capture_TopLevel_When_Nested()
    {
        string razor = """
            @namespace Docs.Pages

            <DocDemo Key="outer">
                <div>
                    <DocDemo Key="inner">
                        <span>nested</span>
                    </DocDemo>
                </div>
            </DocDemo>
            """;

        string output = GeneratorTestHarness.Run(
            new DocDemoGenerator(),
            additionalTexts: [("/docs/Pages/NestedPage.razor", razor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Transform_Loc_Directives()
    {
        string razor = """
            @namespace Docs.Pages

            <DocDemo Key="withLoc">
                <h1>@Loc["title"]</h1>
                <p>@Loc["body"]</p>
            </DocDemo>
            """;

        string output = GeneratorTestHarness.Run(
            new DocDemoGenerator(),
            additionalTexts: [("/docs/Pages/LocPage.razor", razor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Resolve_Namespace_From_ProjectDir_Fallback()
    {
        string razor = """
            <DocDemo Key="only">
                <span>A</span>
            </DocDemo>
            """;

        Dictionary<string, string> options = new()
        {
            ["build_property.rootnamespace"] = "Docs.Root",
            ["build_property.projectdir"] = "/docs/",
        };

        string output = GeneratorTestHarness.Run(
            new DocDemoGenerator(),
            additionalTexts: [("/docs/Pages/Components/ButtonPage.razor", razor)],
            globalOptions: options);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Skip_When_No_Namespace_And_No_ProjectDir()
    {
        string razor = """
            <DocDemo Key="orphan">
                <span>A</span>
            </DocDemo>
            """;

        string output = GeneratorTestHarness.Run(
            new DocDemoGenerator(),
            additionalTexts: [("/docs/Pages/Components/Orphan.razor", razor)]);

        await Verify(output);
    }
}
