using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Select;

[Trait("Component Snapshots", "BOBSelect")]
public class BOBSelectSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, Action<ComponentParameterCollectionBuilder<BOBSelect<string>>> Builder)[] testCases =
        [
            ("Default", p => p
                .Add(c => c.Value, "a")
                .Add(c => c.ChildContent, b => b.AddMarkupContent(0,
                    "<option value='a'>A</option><option value='b'>B</option>"))),
            ("With_Label_And_Helper", p => p
                .Add(c => c.Value, "a")
                .Add(c => c.Label, "Country")
                .Add(c => c.HelperText, "Pick your country")
                .Add(c => c.ChildContent, b => b.AddMarkupContent(0,
                    "<option value='a'>A</option>"))),
            ("Disabled", p => p
                .Add(c => c.Value, "a")
                .Add(c => c.Disabled, true)
                .Add(c => c.ChildContent, b => b.AddMarkupContent(0,
                    "<option value='a'>A</option>"))),
            ("Required", p => p
                .Add(c => c.Value, "a")
                .Add(c => c.Required, true)
                .Add(c => c.Label, "Required field")
                .Add(c => c.ChildContent, b => b.AddMarkupContent(0,
                    "<option value='a'>A</option>"))),
            ("FullWidth", p => p
                .Add(c => c.Value, "a")
                .Add(c => c.FullWidth, true)
                .Add(c => c.ChildContent, b => b.AddMarkupContent(0,
                    "<option value='a'>A</option>")))
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verify(results).UseParameters(scenario.Name);
    }
}