using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Number;

[Trait("Component Snapshots", "BOBInputNumber")]
public class BOBInputNumberSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default_Empty", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumber<int?>>>)(p => p
                .Add(c => c.Label, "Qty")) },

            new { Name = "With_Value", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumber<int?>>>)(p => p
                .Add(c => c.Label, "Qty")
                .Add(c => c.Value, 42)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumber<int?>>>)(p => p
                .Add(c => c.Label, "Qty")
                .Add(c => c.Disabled, true)) },

            new { Name = "Loading", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumber<int?>>>)(p => p
                .Add(c => c.Label, "Qty")
                .Add(c => c.Loading, true)) },

            new { Name = "No_Step_Buttons", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumber<int?>>>)(p => p
                .Add(c => c.Label, "Qty")
                .Add(c => c.ShowStepButtons, false)) },

            new { Name = "Left_Buttons", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumber<int?>>>)(p => p
                .Add(c => c.Label, "Qty")
                .Add(c => c.ButtonPlacement, StepButtonPlacement.Left)) },

            new { Name = "With_Prefix_Suffix", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputNumber<int?>>>)(p => p
                .Add(c => c.Label, "Price")
                .Add(c => c.PrefixText, "$")
                .Add(c => c.SuffixText, "USD")) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputNumber<int?>> cut = ctx.Render<BOBInputNumber<int?>>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
