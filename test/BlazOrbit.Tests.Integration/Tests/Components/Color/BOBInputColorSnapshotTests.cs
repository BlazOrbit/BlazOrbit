using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Color;

[Trait("Component Snapshots", "BOBInputColor")]
public class BOBInputColorSnapshotTests
{
    private class Model { public CssColor? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();

        var testCases = new[]
        {
            new { Name = "Default_Empty", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputColor>>)(p => p
                .Add(c => c.Label, "Color")
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "With_Hex_Value", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputColor>>)(p => p
                .Add(c => c.Label, "Color")
                .Add(c => c.Value, new CssColor("#ff0000"))
                .Add(c => c.OutputFormat, ColorOutputFormats.Hex)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "With_Rgba_Value", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputColor>>)(p => p
                .Add(c => c.Label, "Color")
                .Add(c => c.Value, new CssColor("#00ff00"))
                .Add(c => c.OutputFormat, ColorOutputFormats.Rgba)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputColor>>)(p => p
                .Add(c => c.Label, "Color")
                .Add(c => c.Disabled, true)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Error_State", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputColor>>)(p => p
                .Add(c => c.Label, "Color")
                .Add(c => c.Error, true)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "With_Helper", Builder = (Action<ComponentParameterCollectionBuilder<BOBInputColor>>)(p => p
                .Add(c => c.Label, "Color")
                .Add(c => c.HelperText, "Pick any color")
                .Add(c => c.ValueExpression, () => model.Value)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputColor> cut = ctx.Render<BOBInputColor>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
