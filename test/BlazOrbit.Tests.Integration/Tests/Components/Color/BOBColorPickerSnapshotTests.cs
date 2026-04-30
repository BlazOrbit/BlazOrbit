using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Color;

[Trait("Component Snapshots", "BOBColorPicker")]
public class BOBColorPickerSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default", Builder = (Action<ComponentParameterCollectionBuilder<BOBColorPicker>>)(p => { }) },

            new { Name = "With_Hex_Value", Builder = (Action<ComponentParameterCollectionBuilder<BOBColorPicker>>)(p => p
                .Add(c => c.Value, new CssColor("#ff0000"))
                .Add(c => c.OutputFormat, ColorOutputFormats.Hex)) },

            new { Name = "With_Rgba_Value", Builder = (Action<ComponentParameterCollectionBuilder<BOBColorPicker>>)(p => p
                .Add(c => c.Value, new CssColor("#00ff00"))
                .Add(c => c.OutputFormat, ColorOutputFormats.Rgba)) },

            new { Name = "Hidden_Actions", Builder = (Action<ComponentParameterCollectionBuilder<BOBColorPicker>>)(p => p
                .Add(c => c.ShowActions, false)) },

            new { Name = "Custom_Revert_Text", Builder = (Action<ComponentParameterCollectionBuilder<BOBColorPicker>>)(p => p
                .Add(c => c.ShowActions, true)
                .Add(c => c.RevertText, "Undo")) },

            new { Name = "Small_Selection", Builder = (Action<ComponentParameterCollectionBuilder<BOBColorPicker>>)(p => p
                .Add(c => c.SelectionWidth, 100)
                .Add(c => c.SelectionHeight, 75)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBColorPicker> cut = ctx.Render<BOBColorPicker>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
