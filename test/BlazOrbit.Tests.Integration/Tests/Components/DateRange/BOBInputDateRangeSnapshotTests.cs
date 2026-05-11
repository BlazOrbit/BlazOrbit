using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.DateRange;

[Trait("Component Snapshots", "BOBInputDateRange")]
public class BOBInputDateRangeSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Variants_And_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default_Empty", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputDateRange>>)(_ => { }) },

            new { Name = "Hydrated_Range", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputDateRange>>)(p => p
                .Add(c => c.Value, new global::BlazOrbit.Components.DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)))
                .Add(c => c.Label, "Period")) },

            new { Name = "Without_Presets", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputDateRange>>)(p => p
                .Add(c => c.ShowPresets, false)
                .Add(c => c.Label, "Range")) },

            new { Name = "Without_Summary", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputDateRange>>)(p => p
                .Add(c => c.Value, new global::BlazOrbit.Components.DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 7)))
                .Add(c => c.ShowSummary, false)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputDateRange> cut = ctx.Render<BOBInputDateRange>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
