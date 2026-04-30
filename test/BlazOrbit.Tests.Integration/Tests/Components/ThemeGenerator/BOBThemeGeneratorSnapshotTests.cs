using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Snapshots", "BOBThemeGenerator")]
public class BOBThemeGeneratorSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_ThemeEditor_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBThemeEditor> cut = ctx.Render<BOBThemeEditor>(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>
            {
                ["Primary"] = new("#1A73E8"),
                ["PrimaryContrast"] = new("#FFFFFF"),
                ["Background"] = new("#121212"),
                ["BackgroundContrast"] = new("#FFFFFF"),
            }));

        await Verifier.Verify(cut.GetNormalizedMarkup()).UseParameters(scenario.Name);
    }
}
