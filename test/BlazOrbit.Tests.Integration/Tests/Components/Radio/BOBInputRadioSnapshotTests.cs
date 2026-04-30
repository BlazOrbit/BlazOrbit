using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Radio;

[Trait("Component Snapshots", "BOBInputRadio")]
public class BOBInputRadioSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Vertical_NoSelection", Builder = (Action<ComponentParameterCollectionBuilder<TestBOBInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")) },

            new { Name = "Vertical_Selected", Builder = (Action<ComponentParameterCollectionBuilder<TestBOBInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")
                .Add(c => c.SelectedValue, "opt2")) },

            new { Name = "Horizontal", Builder = (Action<ComponentParameterCollectionBuilder<TestBOBInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")
                .Add(c => c.Orientation, RadioOrientation.Horizontal)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<TestBOBInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")
                .Add(c => c.Disabled, true)) },

            new { Name = "Error", Builder = (Action<ComponentParameterCollectionBuilder<TestBOBInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")
                .Add(c => c.Error, true)) },

            new { Name = "Required_With_Helper", Builder = (Action<ComponentParameterCollectionBuilder<TestBOBInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")
                .Add(c => c.Required, true)
                .Add(c => c.HelperText, "You must pick one.")) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
