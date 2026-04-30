using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Loading;

[Trait("Component Snapshots", "BOBLoadingIndicator")]
public class BOBLoadingIndicatorSnapshotTests
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
                Name = "Spinner_Default",
                Html = ctx.Render<BOBLoadingIndicator>().GetNormalizedMarkup()
            },
            new
            {
                Name = "Dots",
                Html = ctx.Render<BOBLoadingIndicator>(p => p
                    .Add(c => c.Variant, BOBLoadingIndicatorVariant.Dots)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Bars",
                Html = ctx.Render<BOBLoadingIndicator>(p => p
                    .Add(c => c.Variant, BOBLoadingIndicatorVariant.Bars)).GetNormalizedMarkup()
            },
            new
            {
                Name = "LinearIndeterminate",
                Html = ctx.Render<BOBLoadingIndicator>(p => p
                    .Add(c => c.Variant, BOBLoadingIndicatorVariant.LinearIndeterminate)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Spinner_Large_Custom_Label",
                Html = ctx.Render<BOBLoadingIndicator>(p => p
                    .Add(c => c.Size, BOBSize.Large)
                    .Add(c => c.AriaLabel, "Uploading file")).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
