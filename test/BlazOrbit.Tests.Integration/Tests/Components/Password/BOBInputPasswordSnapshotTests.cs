using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Password;

[Trait("Component Snapshots", "BOBInputPassword")]
public class BOBInputPasswordSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_Variants_And_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Default_Empty",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputPassword>>)(p => p
                    .Add(c => c.Label, "Password"))
            },
            new
            {
                Name = "Toggle_Disabled",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputPassword>>)(p => p
                    .Add(c => c.ShowVisibilityToggle, false)
                    .Add(c => c.Label, "Password"))
            },
            new
            {
                Name = "Filled_Variant",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputPassword>>)(p => p
                    .Add(c => c.Variant, BOBInputVariant.Filled)
                    .Add(c => c.Label, "Password"))
            },
            new
            {
                Name = "Strong_With_Meter",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputPassword>>)(p => p
                    .Add(c => c.ShowStrengthMeter, true)
                    .Add(c => c.MinLength, 8)
                    .Add(c => c.Value, "Aa1!aaaa1234"))
            },
            new
            {
                Name = "Weak_With_Meter",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputPassword>>)(p => p
                    .Add(c => c.ShowStrengthMeter, true)
                    .Add(c => c.Value, "abc"))
            },
            new
            {
                Name = "Required_Disabled",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputPassword>>)(p => p
                    .Add(c => c.Required, true)
                    .Add(c => c.Disabled, true)
                    .Add(c => c.Label, "Password"))
            },
            new
            {
                Name = "Error_With_Helper",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputPassword>>)(p => p
                    .Add(c => c.Error, true)
                    .Add(c => c.Label, "Password")
                    .Add(c => c.HelperText, "Too short."))
            }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(testCase.Builder);
            return new { testCase.Name, Html = cut.GetNormalizedMarkup() };
        }).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}
