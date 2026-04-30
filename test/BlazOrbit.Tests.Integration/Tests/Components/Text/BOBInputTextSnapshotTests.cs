using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Text;

[Trait("Component Snapshots", "BOBInputText")]
public class BOBInputTextSnapshotTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();

        var testCases = new[]
        {
            new { Name = "Default_Outlined", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputText>>)(p => p
                .Add(c => c.Label, "Name")
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Filled_With_Value", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputText>>)(p => p
                .Add(c => c.Variant, BOBInputVariant.Filled)
                .Add(c => c.Label, "Name")
                .Add(c => c.Value, "filled value")
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Standard_With_Helper", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputText>>)(p => p
                .Add(c => c.Variant, BOBInputVariant.Standard)
                .Add(c => c.Label, "Email")
                .Add(c => c.HelperText, "Your primary email address.")
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Required", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputText>>)(p => p
                .Add(c => c.Label, "Required Field")
                .Add(c => c.Required, true)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputText>>)(p => p
                .Add(c => c.Label, "Disabled")
                .Add(c => c.Disabled, true)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Loading", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputText>>)(p => p
                .Add(c => c.Label, "Loading")
                .Add(c => c.Loading, true)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Error_State", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputText>>)(p => p
                .Add(c => c.Label, "Error")
                .Add(c => c.Error, true)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "With_Prefix_Suffix", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputText>>)(p => p
                .Add(c => c.Label, "Website")
                .Add(c => c.PrefixText, "https://")
                .Add(c => c.SuffixText, ".com")
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Sized_Large_Compact", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputText>>)(p => p
                .Add(c => c.Label, "Styled")
                .Add(c => c.Size, BOBSize.Large)
                .Add(c => c.Density, BOBDensity.Compact)
                .Add(c => c.ValueExpression, () => model.Value)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
