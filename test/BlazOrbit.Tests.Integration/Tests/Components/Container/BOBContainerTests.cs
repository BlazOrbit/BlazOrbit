using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Container;

[Trait("Component Rendering", "BOBContainer")]
public class BOBContainerRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Children(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBContainer> cut = ctx.Render<BOBContainer>(p => p
            .AddChildContent("<span class='ok'>ok</span>"));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("container");
        cut.Find(".ok").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Size_Token_For_Each_Tier(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Walk every tier in a single render scope so a regression in the enum→token
        // mapping is caught regardless of which value is "first".
        (BOBContainerSize size, string token)[] tiers =
        [
            (BOBContainerSize.Small, "small"),
            (BOBContainerSize.Medium, "medium"),
            (BOBContainerSize.Large, "large"),
            (BOBContainerSize.Wide, "wide"),
            (BOBContainerSize.Full, "full")
        ];

        foreach ((BOBContainerSize size, string token) in tiers)
        {
            IRenderedComponent<BOBContainer> cut = ctx.Render<BOBContainer>(p => p
                .Add(c => c.Size, size));
            cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be(token);
        }
    }
}

[Trait("Component Rendering", "BOBSection")]
public class BOBSectionRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Title_As_H2(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSection> cut = ctx.Render<BOBSection>(p => p
            .Add(c => c.Title, "Pricing"));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("section");
        cut.Find("h2.bob-section__title").TextContent.Should().Be("Pricing");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Body_Wrapper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSection> cut = ctx.Render<BOBSection>(p => p
            .AddChildContent("<p class='b'>x</p>"));

        cut.Find(".bob-section__body .b").TextContent.Should().Be("x");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Title_When_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSection> cut = ctx.Render<BOBSection>(p => p
            .AddChildContent("<p>x</p>"));

        cut.FindAll("h2.bob-section__title").Should().BeEmpty();
    }
}

[Trait("Component Snapshots", "BOBContainer")]
public class BOBContainerSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "ContainerLarge",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBContainer>>)(p => p
                    .Add(c => c.Size, BOBContainerSize.Large)
                    .AddChildContent("<p>body</p>"))
            },
            new
            {
                Name = "ContainerFull",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBContainer>>)(p => p
                    .Add(c => c.Size, BOBContainerSize.Full)
                    .AddChildContent("<p>body</p>"))
            }
        };

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBContainer> cut = ctx.Render<BOBContainer>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}
