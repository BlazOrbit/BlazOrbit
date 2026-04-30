using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Snapshots", "BOBInputOutline")]
public class BOBInputOutlineSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Representative_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "NoLabel", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputOutline>>)(p => p
                .Add(c => c.For, "input-1")) },

            new { Name = "WithLabel", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputOutline>>)(p => p
                .Add(c => c.Label, "Full Name")
                .Add(c => c.For, "input-1")) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputOutline> cut = ctx.Render<BOBInputOutline>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
