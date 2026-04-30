using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Grid;

[Trait("Component Snapshots", "BOBGrid")]
public class BOBGridSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Grid_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, Action<ComponentParameterCollectionBuilder<BOBGrid>> Builder)[] testCases =
        [
            ("Default", p => p
                .Add(c => c.ChildContent, b => b.AddContent(0, "content"))),
            ("With3Columns", p => p
                .Add(c => c.Columns, 3)
                .Add(c => c.Gap, "1rem")),
            ("ColumnDirection", p => p
                .Add(c => c.Direction, GridDirection.Column)
                .Add(c => c.Gap, "8px")),
            ("WithMaxWidth", p => p
                .Add(c => c.MaxWidth, "1200px")
                .Add(c => c.Columns, 12)),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBGrid> cut = ctx.Render<BOBGrid>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verifier.Verify(results).UseParameters(scenario.Name);
    }
}
