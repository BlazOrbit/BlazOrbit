using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Snapshots", "BOBInputPrefix")]
public class BOBInputPrefixSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Representative_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "TextOnly", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputPrefix>>)(p => p
                .Add(c => c.PrefixText, "$")
                .Add(c => c.Size, BOBSize.Medium)) },

            new { Name = "IconOnly", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputPrefix>>)(p => p
                .Add(c => c.PrefixIcon, BOBIconKeys.MaterialIconsOutlined.i_check)
                .Add(c => c.Size, BOBSize.Medium)) },

            new { Name = "TextAndIcon", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputPrefix>>)(p => p
                .Add(c => c.PrefixText, "USD")
                .Add(c => c.PrefixIcon, BOBIconKeys.MaterialIconsOutlined.i_check)
                .Add(c => c.Size, BOBSize.Medium)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputPrefix> cut = ctx.Render<BOBInputPrefix>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
