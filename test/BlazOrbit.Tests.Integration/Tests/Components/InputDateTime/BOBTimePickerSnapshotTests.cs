using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Snapshots", "BOBTimePicker")]
public class BOBTimePickerSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default", Builder = (Action<ComponentParameterCollectionBuilder<BOBTimePicker>>)(p => { }) },

            new { Name = "With_Value_14_35", Builder = (Action<ComponentParameterCollectionBuilder<BOBTimePicker>>)(p => p
                .Add(c => c.Value, new TimeOnly(14, 35))) },

            new { Name = "With_Value_09_05", Builder = (Action<ComponentParameterCollectionBuilder<BOBTimePicker>>)(p => p
                .Add(c => c.Value, new TimeOnly(9, 5))) },

            new { Name = "Midnight", Builder = (Action<ComponentParameterCollectionBuilder<BOBTimePicker>>)(p => p
                .Add(c => c.Value, new TimeOnly(0, 0))) },

            new { Name = "Large_Size", Builder = (Action<ComponentParameterCollectionBuilder<BOBTimePicker>>)(p => p
                .Add(c => c.Value, new TimeOnly(14, 35))
                .Add(c => c.Size, BOBSize.Large)) },

            new { Name = "Compact_Density", Builder = (Action<ComponentParameterCollectionBuilder<BOBTimePicker>>)(p => p
                .Add(c => c.Value, new TimeOnly(14, 35))
                .Add(c => c.Density, BOBDensity.Compact)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBTimePicker> cut = ctx.Render<BOBTimePicker>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
