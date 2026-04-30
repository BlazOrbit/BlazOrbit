using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Snapshots", "BOBThemePreview")]
public class BOBThemePreviewSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Section_Structure_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBThemePreview> cut = ctx.Render<BOBThemePreview>();

        // Snapshot the section headings + preview structure only — the full markup is
        // too large and brittle. This protects the demo surface from silent breakage
        // while leaving room for internal component evolution.
        string[] headings = cut.FindAll(".bob-theme-preview__section > h5")
                               .Select(h => h.TextContent.Trim())
                               .ToArray();

        int sections = cut.FindAll(".bob-theme-preview__section").Count;
        int rows = cut.FindAll(".bob-theme-preview__row").Count;

        await Verify(new { headings, sections, rows }).UseParameters(scenario.Name);
    }
}
