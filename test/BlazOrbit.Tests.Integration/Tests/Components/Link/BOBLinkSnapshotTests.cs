using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Link;

[Trait("Component Snapshots", "BOBLink")]
public class BOBLinkSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Text",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBLink>>)(p => p
                    .Add(c => c.Href, "/docs")
                    .Add(c => c.Text, "Read the docs"))
            },
            new
            {
                Name = "Button",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBLink>>)(p => p
                    .Add(c => c.Href, "/signup")
                    .Add(c => c.Text, "Sign up")
                    .Add(c => c.Variant, BOBLinkVariant.Button)
                    .Add(c => c.LeadingIcon, BOBIconKeys.MaterialIconsOutlined.i_check))
            },
            new
            {
                Name = "External",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBLink>>)(p => p
                    .Add(c => c.Href, "https://example.com")
                    .Add(c => c.Text, "Example")
                    .Add(c => c.Target, "_blank")
                    .Add(c => c.TrailingIcon, BOBIconKeys.MaterialIconsOutlined.i_open_in_new))
            },
            new
            {
                Name = "Elevated",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBLink>>)(p => p
                    .Add(c => c.Href, "/pricing")
                    .Add(c => c.Text, "Pricing")
                    .Add(c => c.Variant, BOBLinkVariant.Button)
                    .Add(c => c.Shadow, BOBShadowPresets.Elevation(8))
                    .Add(c => c.Transitions, BOBTransitionPresets.HoverLift))
            }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(testCase.Builder);
            return new { testCase.Name, Html = cut.GetNormalizedMarkup() };
        }).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}
