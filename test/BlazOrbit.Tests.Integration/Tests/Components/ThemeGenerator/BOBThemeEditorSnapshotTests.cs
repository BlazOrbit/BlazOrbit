using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Snapshots", "BOBThemeEditor")]
public class BOBThemeEditorSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Status_Palette_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>
            {
                ["Error"] = new("#B00020"),
                ["ErrorContrast"] = new("#FFFFFF"),
                ["Success"] = new("#2E7D32"),
                ["SuccessContrast"] = new("#FFFFFF"),
            }));

        await Verify(cut.GetNormalizedMarkup()).UseParameters(scenario.Name);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Empty_Palette_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, []));

        await Verify(cut.GetNormalizedMarkup()).UseParameters(scenario.Name);
    }
}
