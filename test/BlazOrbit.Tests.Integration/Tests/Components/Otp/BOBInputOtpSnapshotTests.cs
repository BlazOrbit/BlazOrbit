using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Otp;

[Trait("Component Snapshots", "BOBInputOtp")]
public class BOBInputOtpSnapshotTests
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
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputOtp>>)(p => p
                    .Add(c => c.Length, 4)
                    .Add(c => c.Label, "Code"))
            },
            new
            {
                Name = "Default_Filled",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputOtp>>)(p => p
                    .Add(c => c.Length, 4)
                    .Add(c => c.Value, "1234"))
            },
            new
            {
                Name = "Underlined_Variant",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputOtp>>)(p => p
                    .Add(c => c.Length, 4)
                    .Add(c => c.Variant, BOBInputOtpVariant.Underlined)
                    .Add(c => c.Label, "PIN"))
            },
            new
            {
                Name = "Masked",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputOtp>>)(p => p
                    .Add(c => c.Length, 4)
                    .Add(c => c.Mask, true)
                    .Add(c => c.Value, "1234"))
            },
            new
            {
                Name = "Alphanumeric",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputOtp>>)(p => p
                    .Add(c => c.Length, 4)
                    .Add(c => c.Numeric, false)
                    .Add(c => c.Value, "ABCD"))
            },
            new
            {
                Name = "Disabled",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputOtp>>)(p => p
                    .Add(c => c.Length, 4)
                    .Add(c => c.Disabled, true))
            },
            new
            {
                Name = "Error_With_Helper",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputOtp>>)(p => p
                    .Add(c => c.Length, 4)
                    .Add(c => c.Error, true)
                    .Add(c => c.HelperText, "Invalid code."))
            },
            new
            {
                Name = "Required_Large",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBInputOtp>>)(p => p
                    .Add(c => c.Length, 4)
                    .Add(c => c.Required, true)
                    .Add(c => c.Size, BOBSize.Large)
                    .Add(c => c.Label, "Code"))
            }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(testCase.Builder);
            return new { testCase.Name, Html = cut.GetNormalizedMarkup() };
        }).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}
