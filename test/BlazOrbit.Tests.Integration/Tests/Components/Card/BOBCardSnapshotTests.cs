using BlazOrbit.Components;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;

namespace BlazOrbit.Tests.Integration.Tests.Components.Card;

[Trait("Component Snapshots", "BOBCard")]
public class BOBCardSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Card_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, Action<ComponentParameterCollectionBuilder<BOBCard>> Builder)[] testCases =
        [
            ("Default", p => p
                .Add(c => c.ChildContent, b => b.AddContent(0, "Card body"))),
            ("WithHeader", p => p
                .Add(c => c.Header, b => b.AddContent(0, "Title"))
                .Add(c => c.ChildContent, b => b.AddContent(0, "Body"))),
            ("WithMedia", p => p
                .Add(c => c.Media, b => b.AddContent(0, "img"))
                .Add(c => c.MediaHeight, "200px")
                .Add(c => c.ChildContent, b => b.AddContent(0, "Body"))),
            ("WithShadow", p => p
                .Add(c => c.Shadow, ShadowStyle.Create(4, 8))
                .Add(c => c.ChildContent, b => b.AddContent(0, "Body"))),
            ("WithActions", p => p
                .Add(c => c.Actions, b => b.AddContent(0, "Save"))
                .Add(c => c.ChildContent, b => b.AddContent(0, "Body"))),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verifier.Verify(results).UseParameters(scenario.Name);
    }
}
