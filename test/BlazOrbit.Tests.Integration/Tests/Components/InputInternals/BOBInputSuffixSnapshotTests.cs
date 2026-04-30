using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Snapshots", "BOBInputSuffix")]
public class BOBInputSuffixSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Representative_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "TextOnly", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputSuffix>>)(p => p
                .Add(c => c.SuffixText, "kg")
                .Add(c => c.Size, BOBSize.Medium)) },

            new { Name = "IconOnly", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputSuffix>>)(p => p
                .Add(c => c.SuffixIcon, BOBIconKeys.MaterialIconsOutlined.i_check)
                .Add(c => c.Size, BOBSize.Medium)) },

            new { Name = "TextAndIcon", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputSuffix>>)(p => p
                .Add(c => c.SuffixText, "EUR")
                .Add(c => c.SuffixIcon, BOBIconKeys.MaterialIconsOutlined.i_check)
                .Add(c => c.Size, BOBSize.Medium)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputSuffix> cut = ctx.Render<BOBInputSuffix>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
