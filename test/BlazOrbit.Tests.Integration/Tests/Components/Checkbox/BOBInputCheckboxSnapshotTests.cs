using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Checkbox;

[Trait("Component Snapshots", "BOBInputCheckbox")]
public class BOBInputCheckboxSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Bool_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Unchecked", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputCheckbox<bool>>>)(p => p
                .Add(c => c.Label, "Accept")
                .Add(c => c.Value, false)) },

            new { Name = "Checked", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputCheckbox<bool>>>)(p => p
                .Add(c => c.Label, "Accept")
                .Add(c => c.Value, true)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputCheckbox<bool>>>)(p => p
                .Add(c => c.Label, "Disabled")
                .Add(c => c.Disabled, true)) },

            new { Name = "Error", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputCheckbox<bool>>>)(p => p
                .Add(c => c.Label, "Error")
                .Add(c => c.Error, true)) },

            new { Name = "Required_With_Helper", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputCheckbox<bool>>>)(p => p
                .Add(c => c.Label, "Terms")
                .Add(c => c.Required, true)
                .Add(c => c.HelperText, "Must accept.")) },

            new { Name = "Large_Colored", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputCheckbox<bool>>>)(p => p
                .Add(c => c.Label, "Styled")
                .Add(c => c.Value, true)
                .Add(c => c.Size, BOBSize.Large)
                .Add(c => c.Color, "rgba(255,0,0,1)")) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Indeterminate(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool?>> cut = ctx.Render<BOBInputCheckbox<bool?>>(p => p
            .Add(c => c.Label, "Indeterminate")
            .Add(c => c.Value, (bool?)null));

        await Verify(cut.GetNormalizedMarkup()).UseParameters(scenario.Name);
    }
}
