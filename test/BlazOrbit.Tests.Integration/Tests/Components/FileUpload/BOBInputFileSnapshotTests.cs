using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.FileUpload;

[Trait("Component Snapshots", "BOBInputFile")]
public class BOBInputFileSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Variants_And_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputFile>>)(_ => { }) },
            new
            {
                Name = "Multiple_With_Accept",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputFile>>)(p => p
                    .Add(c => c.Multiple, true)
                    .Add(c => c.Accept, "image/*")
                    .Add(c => c.Label, "Images"))
            },
            new
            {
                Name = "Disabled",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputFile>>)(p => p
                    .Add(c => c.Disabled, true)
                    .Add(c => c.Label, "Disabled"))
            },
            new
            {
                Name = "Custom_Texts",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputFile>>)(p => p
                    .Add(c => c.DropZoneText, "Suelta aquí")
                    .Add(c => c.DropZoneHint, "Hasta 5 MB")
                    .Add(c => c.HelperText, "Documentos PDF"))
            }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(testCase.Builder);
            return new { testCase.Name, Html = cut.GetNormalizedMarkup() };
        }).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}
