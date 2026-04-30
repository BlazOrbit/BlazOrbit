using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Snapshots", "BOBInputLoading")]
public class BOBInputLoadingSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Representative_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Idle", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputLoading>>)(p => p
                .Add(c => c.Loading, false)) },

            new { Name = "Loading", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputLoading>>)(p => p
                .Add(c => c.Loading, true)
                .Add(c => c.LoadingIndicatorVariant, BOBLoadingIndicatorVariant.CircularProgress)
                .Add(c => c.Size, BOBSize.Medium)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputLoading> cut = ctx.Render<BOBInputLoading>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
