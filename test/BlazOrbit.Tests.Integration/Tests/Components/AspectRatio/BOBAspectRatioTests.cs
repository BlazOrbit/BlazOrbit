using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.AspectRatio;

[Trait("Component Rendering", "BOBAspectRatio")]
public class BOBAspectRatioRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Ratio(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAspectRatio> cut = ctx.Render<BOBAspectRatio>(p => p
            .AddChildContent("<span>fill</span>"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("aspect-ratio");
        root.GetAttribute("style").Should().Contain("--bob-inline-aspect-ratio:16 / 9");
        cut.Find(".bob-aspect-ratio__inner").TextContent.Should().Be("fill");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Ratio(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAspectRatio> cut = ctx.Render<BOBAspectRatio>(p => p
            .Add(c => c.Ratio, "1 / 1"));

        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-aspect-ratio:1 / 1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fall_Back_When_Ratio_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAspectRatio> cut = ctx.Render<BOBAspectRatio>(p => p
            .Add(c => c.Ratio, ""));

        cut.Find("bob-component").GetAttribute("style").Should().Contain("--bob-inline-aspect-ratio:16 / 9");
    }
}

[Trait("Component Snapshots", "BOBAspectRatio")]
public class BOBAspectRatioSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default", Builder = (Action<ComponentParameterCollectionBuilder<BOBAspectRatio>>)(p => p
                .AddChildContent("<div>x</div>")) },
            new { Name = "Square", Builder = (Action<ComponentParameterCollectionBuilder<BOBAspectRatio>>)(p => p
                .Add(c => c.Ratio, "1 / 1")
                .AddChildContent("<div>x</div>")) },
        };

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBAspectRatio> cut = ctx.Render<BOBAspectRatio>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
