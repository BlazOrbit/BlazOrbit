using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputRangeSlider;

[Trait("Component Snapshots", "BOBInputRangeSlider")]
public class BOBInputRangeSliderSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputRangeSlider<int>>>)(p => p
                .Add(c => c.Label, "Range")) },

            new { Name = "With_Value", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputRangeSlider<int>>>)(p => p
                .Add(c => c.Label, "Range")
                .Add(c => c.Value, new BOBNumericRange<int>(20, 80))) },

            new { Name = "Vertical", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputRangeSlider<int>>>)(p => p
                .Add(c => c.Label, "Hours")
                .Add(c => c.Orientation, BOBSliderOrientation.Vertical)
                .Add(c => c.Value, new BOBNumericRange<int>(8, 17))) },

            new { Name = "With_Ticks", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputRangeSlider<int>>>)(p => p
                .Add(c => c.Min, 0)
                .Add(c => c.Max, 10)
                .Add(c => c.Step, 1)
                .Add(c => c.ShowTicks, true)
                .Add(c => c.Value, new BOBNumericRange<int>(2, 7))) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputRangeSlider<int>>>)(p => p
                .Add(c => c.Label, "Range")
                .Add(c => c.Disabled, true)) },

            new { Name = "ReadOnly", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputRangeSlider<int>>>)(p => p
                .Add(c => c.Label, "Range")
                .Add(c => c.ReadOnly, true)) },

            new { Name = "Required", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputRangeSlider<int>>>)(p => p
                .Add(c => c.Label, "Range")
                .Add(c => c.Required, true)) },

            new { Name = "Custom_AriaLabels", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputRangeSlider<int>>>)(p => p
                .Add(c => c.AriaLabelMin, "From")
                .Add(c => c.AriaLabelMax, "To")
                .Add(c => c.Value, new BOBNumericRange<int>(10, 90))) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputRangeSlider<int>> cut = ctx.Render<BOBInputRangeSlider<int>>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
