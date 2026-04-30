using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Snapshots", "BOBDatePicker")]
public class BOBDatePickerSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "With_Value_June_15_2024", Builder = (Action<ComponentParameterCollectionBuilder<BOBDatePicker>>)(p => p
                .Add(c => c.Value, new DateOnly(2024, 6, 15))) },

            new { Name = "With_Value_January_1_2020", Builder = (Action<ComponentParameterCollectionBuilder<BOBDatePicker>>)(p => p
                .Add(c => c.Value, new DateOnly(2020, 1, 1))) },

            new { Name = "Large_Size", Builder = (Action<ComponentParameterCollectionBuilder<BOBDatePicker>>)(p => p
                .Add(c => c.Value, new DateOnly(2024, 6, 15))
                .Add(c => c.Size, BOBSize.Large)) },

            new { Name = "Compact_Density", Builder = (Action<ComponentParameterCollectionBuilder<BOBDatePicker>>)(p => p
                .Add(c => c.Value, new DateOnly(2024, 6, 15))
                .Add(c => c.Density, BOBDensity.Compact)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBDatePicker> cut = ctx.Render<BOBDatePicker>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
