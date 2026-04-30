using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Button;

[Trait("Component Snapshots", "BOBButton")]
public class BOBButtonSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default", Builder = (Action<ComponentParameterCollectionBuilder<BOBButton>>)(p => p
                .Add(c => c.Text, "Default Button")) },

            new { Name = "WithIcon", Builder = (Action<ComponentParameterCollectionBuilder<BOBButton>>)(p => p
                .Add(c => c.Text, "Icon Button")
                .Add(c => c.LeadingIcon, BOBIconKeys.MaterialIconsOutlined.i_check)) },

            new { Name = "Loading", Builder = (Action<ComponentParameterCollectionBuilder<BOBButton>>)(p => p
                .Add(c => c.Text, "Loading")
                .Add(c => c.Loading, true)
                .Add(c => c.LoadingIndicatorVariant, BOBLoadingIndicatorVariant.Spinner)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BOBButton>>)(p => p
                .Add(c => c.Text, "Disabled")
                .Add(c => c.Disabled, true)) },

            new { Name = "Elevated", Builder = (Action<ComponentParameterCollectionBuilder<BOBButton>>)(p => p
                .Add(c => c.Text, "Elevated")
                .Add(c => c.Shadow, BOBShadowPresets.Elevation(8))
                .Add(c => c.Transitions, BOBTransitionPresets.HoverLift)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name); ;
    }
}