using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.AutoComplete;

[Trait("Component Snapshots", "BOBAutoComplete")]
public class BOBAutoCompleteSnapshotTests
{
    private static Func<string, CancellationToken, Task<IEnumerable<string>>> StaticSource(params string[] items) =>
        (q, _) => Task.FromResult(items.AsEnumerable());

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Variants_And_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default_Empty", Builder = (Action<ComponentParameterCollectionBuilder<BOBAutoComplete<string>>>)(p => p
                .Add(c => c.SearchAsync, StaticSource())
                .Add(c => c.ItemText, s => s)
                .Add(c => c.Label, "Search")) },

            new { Name = "Filled_Variant", Builder = (Action<ComponentParameterCollectionBuilder<BOBAutoComplete<string>>>)(p => p
                .Add(c => c.SearchAsync, StaticSource())
                .Add(c => c.ItemText, s => s)
                .Add(c => c.Variant, BOBInputVariant.Filled)
                .Add(c => c.Label, "Search")) },

            new { Name = "Hydrated_Value", Builder = (Action<ComponentParameterCollectionBuilder<BOBAutoComplete<string>>>)(p => p
                .Add(c => c.SearchAsync, StaticSource())
                .Add(c => c.ItemText, s => s)
                .Add(c => c.Value, "Spain")
                .Add(c => c.Label, "Country")) },

            new { Name = "Required_Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BOBAutoComplete<string>>>)(p => p
                .Add(c => c.SearchAsync, StaticSource())
                .Add(c => c.ItemText, s => s)
                .Add(c => c.Required, true)
                .Add(c => c.Disabled, true)
                .Add(c => c.Label, "Search")) },

            new { Name = "Error_With_Helper", Builder = (Action<ComponentParameterCollectionBuilder<BOBAutoComplete<string>>>)(p => p
                .Add(c => c.SearchAsync, StaticSource())
                .Add(c => c.ItemText, s => s)
                .Add(c => c.Error, true)
                .Add(c => c.HelperText, "Pick something.")) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBAutoComplete<string>> cut = ctx.Render<BOBAutoComplete<string>>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
