using BlazOrbit.Components;
using BlazOrbit.SyntaxHighlight;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.CodeBlock;

[Trait("Component Snapshots", "BOBCodeBlock")]
public class BOBCodeBlockSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "CSharp_Default",
                Html = ctx.Render<BOBCodeBlock>(p => p
                    .Add(c => c.Code, "var x = 1;")
                    .Add(c => c.Language, SyntaxHighlightLanguage.CSharp)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Json_With_Custom_Title",
                Html = ctx.Render<BOBCodeBlock>(p => p
                    .Add(c => c.Code, "{ \"key\": \"value\" }")
                    .Add(c => c.Language, SyntaxHighlightLanguage.Json)
                    .Add(c => c.Title, "Config")).GetNormalizedMarkup()
            },
            new
            {
                Name = "Large_Size",
                Html = ctx.Render<BOBCodeBlock>(p => p
                    .Add(c => c.Code, "var x = 1;")
                    .Add(c => c.Size, BOBSize.Large)).GetNormalizedMarkup()
            },
            new
            {
                Name = "TypeScript",
                Html = ctx.Render<BOBCodeBlock>(p => p
                    .Add(c => c.Code, "const x: number = 1;")
                    .Add(c => c.Language, SyntaxHighlightLanguage.TypeScript)).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
