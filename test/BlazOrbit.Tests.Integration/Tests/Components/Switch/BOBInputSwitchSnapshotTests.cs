using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Switch;

[Trait("Component Snapshots", "BOBInputSwitch")]
public class BOBInputSwitchSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Off", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputSwitch>>)(p => p
                .Add(c => c.Label, "Toggle")
                .Add(c => c.Value, false)) },

            new { Name = "On", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputSwitch>>)(p => p
                .Add(c => c.Label, "Toggle")
                .Add(c => c.Value, true)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputSwitch>>)(p => p
                .Add(c => c.Label, "Disabled")
                .Add(c => c.Disabled, true)) },

            new { Name = "Error", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputSwitch>>)(p => p
                .Add(c => c.Label, "Error")
                .Add(c => c.Error, true)) },

            new { Name = "With_Helper", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputSwitch>>)(p => p
                .Add(c => c.Label, "Notify")
                .Add(c => c.HelperText, "Send email alerts.")) },

            new { Name = "Large_Compact", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputSwitch>>)(p => p
                .Add(c => c.Label, "Big")
                .Add(c => c.Size, BOBSize.Large)
                .Add(c => c.Density, BOBDensity.Compact)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
