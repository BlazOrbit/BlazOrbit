using BlazOrbit.Components.Display;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Rating;

[Trait("Component Snapshots", "BOBRating")]
public class BOBRatingSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Empty", Builder = (Action<ComponentParameterCollectionBuilder<BOBRating>>)(p => p
                .Add(c => c.Label, "Rate me")) },

            new { Name = "Three_Stars", Builder = (Action<ComponentParameterCollectionBuilder<BOBRating>>)(p => p
                .Add(c => c.Value, 3)) },

            new { Name = "Half_Step", Builder = (Action<ComponentParameterCollectionBuilder<BOBRating>>)(p => p
                .Add(c => c.AllowHalf, true)
                .Add(c => c.Value, 2.5)) },

            new { Name = "ReadOnly_Four", Builder = (Action<ComponentParameterCollectionBuilder<BOBRating>>)(p => p
                .Add(c => c.Value, 4)
                .Add(c => c.ReadOnly, true)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BOBRating>>)(p => p
                .Add(c => c.Value, 2)
                .Add(c => c.Disabled, true)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBRating> cut = ctx.Render<BOBRating>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
