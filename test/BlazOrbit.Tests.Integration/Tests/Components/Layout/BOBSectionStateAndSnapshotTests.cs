using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Layout;

[Trait("Component State", "BOBSection")]
public class BOBSectionStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSection> cut = ctx.Render<BOBSection>(p => p
            .Add(c => c.Size, BOBContainerSize.Small));

        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("small");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_Large_Size(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSection> cut = ctx.Render<BOBSection>();

        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
    }
}

[Trait("Component Snapshots", "BOBSection")]
public class BOBSectionSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, Action<ComponentParameterCollectionBuilder<BOBSection>> Builder)[] testCases =
        [
            ("Default", p => p
                .Add(c => c.ChildContent, b => b.AddContent(0, "Body"))),
            ("With_Title", p => p
                .Add(c => c.Title, "Features")
                .Add(c => c.ChildContent, b => b.AddContent(0, "Body"))),
            ("With_Title_And_Lead", p => p
                .Add(c => c.Title, "Features")
                .Add(c => c.Lead, b => b.AddContent(0, "Learn more about our product"))
                .Add(c => c.ChildContent, b => b.AddContent(0, "Body"))),
            ("Small_Size", p => p
                .Add(c => c.Size, BOBContainerSize.Small)
                .Add(c => c.ChildContent, b => b.AddContent(0, "Body"))),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBSection> cut = ctx.Render<BOBSection>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verify(results).UseParameters(scenario.Name);
    }
}
