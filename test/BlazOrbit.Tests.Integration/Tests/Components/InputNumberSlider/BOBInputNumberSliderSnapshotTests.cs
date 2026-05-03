using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputNumberSlider;

[Trait("Component Snapshots", "BOBInputNumberSlider")]
public class BOBInputNumberSliderSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumberSlider<int>>>)(p => p
                .Add(c => c.Label, "Volume")) },

            new { Name = "With_Value", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumberSlider<int>>>)(p => p
                .Add(c => c.Label, "Volume")
                .Add(c => c.Value, 42)) },

            new { Name = "Vertical", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumberSlider<int>>>)(p => p
                .Add(c => c.Label, "Bass")
                .Add(c => c.Orientation, BOBSliderOrientation.Vertical)
                .Add(c => c.Value, 30)) },

            new { Name = "With_Ticks", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumberSlider<int>>>)(p => p
                .Add(c => c.Min, 0)
                .Add(c => c.Max, 10)
                .Add(c => c.Step, 1)
                .Add(c => c.ShowTicks, true)
                .Add(c => c.Value, 5)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumberSlider<int>>>)(p => p
                .Add(c => c.Label, "Volume")
                .Add(c => c.Disabled, true)) },

            new { Name = "ReadOnly", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumberSlider<int>>>)(p => p
                .Add(c => c.Label, "Volume")
                .Add(c => c.ReadOnly, true)) },

            new { Name = "Required", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumberSlider<int>>>)(p => p
                .Add(c => c.Label, "Volume")
                .Add(c => c.Required, true)) },

            new { Name = "Error", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumberSlider<int>>>)(p => p
                .Add(c => c.Label, "Volume")
                .Add(c => c.Error, true)
                .Add(c => c.HelperText, "Out of range")) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputNumberSlider<int>> cut = ctx.Render<BOBInputNumberSlider<int>>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
