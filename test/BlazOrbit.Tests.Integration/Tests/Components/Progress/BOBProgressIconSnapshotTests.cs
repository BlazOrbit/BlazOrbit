using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Progress;

[Trait("Component Snapshots", "BOBProgressIcon")]
public class BOBProgressIconSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Spinner_Default", Html = ctx.Render<BOBProgressIcon>().GetNormalizedMarkup() }, new
            {
                Name = "Dots",
                Html = ctx.Render<BOBProgressIcon>(p => p
                    .Add(c => c.Variant, BOBProgressIconVariant.Dots)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Bars",
                Html = ctx.Render<BOBProgressIcon>(p => p
                    .Add(c => c.Variant, BOBProgressIconVariant.Bars)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Spinner_Large_Custom_Label",
                Html = ctx.Render<BOBProgressIcon>(p => p
                    .Add(c => c.Size, BOBSize.Large)
                    .Add(c => c.AriaLabel, "Uploading file")).GetNormalizedMarkup()
            }
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}