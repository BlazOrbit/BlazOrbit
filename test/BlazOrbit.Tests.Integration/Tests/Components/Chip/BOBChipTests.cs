using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Chip;

[Trait("Component Rendering", "BOBChip")]
public class BOBChipRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Static_Chip_As_Span(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBChip> cut = ctx.Render<BOBChip>(p => p
            .Add(c => c.Label, "tag"));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("chip");
        cut.Find(".bob-chip").TagName.Should().Be("SPAN");
        cut.Find(".bob-chip__label").TextContent.Should().Be("tag");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Selectable_Chip_As_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBChip> cut = ctx.Render<BOBChip>(p => p
            .Add(c => c.Label, "filter")
            .Add(c => c.Selectable, true));

        cut.Find(".bob-chip").TagName.Should().Be("BUTTON");
        cut.Find(".bob-chip").GetAttribute("aria-pressed").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Count_Badge(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBChip> cut = ctx.Render<BOBChip>(p => p
            .Add(c => c.Label, "Inbox")
            .Add(c => c.Count, 5));

        cut.Find(".bob-chip__count").TextContent.Should().Be("5");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Remove_Button_When_Removable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBChip> cut = ctx.Render<BOBChip>(p => p
            .Add(c => c.Label, "x")
            .Add(c => c.Removable, true));

        cut.FindAll(".bob-chip__remove").Should().HaveCount(1);
    }
}

[Trait("Component Interaction", "BOBChip")]
public class BOBChipInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Selected_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool selected = false;
        IRenderedComponent<BOBChip> cut = ctx.Render<BOBChip>(p => p
            .Add(c => c.Label, "f")
            .Add(c => c.Selectable, true)
            .Add(c => c.SelectedChanged, v => selected = v));

        cut.Find(".bob-chip").Click();
        selected.Should().BeTrue();
        cut.Find(".bob-chip").GetAttribute("aria-pressed").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnClick_For_Action_Chip(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int clicks = 0;
        IRenderedComponent<BOBChip> cut = ctx.Render<BOBChip>(p => p
            .Add(c => c.Label, "go")
            .Add(c => c.Clickable, true)
            .Add(c => c.OnClick, () => clicks++));

        cut.Find(".bob-chip").Click();
        clicks.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnRemoved_On_X_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool removed = false;
        IRenderedComponent<BOBChip> cut = ctx.Render<BOBChip>(p => p
            .Add(c => c.Label, "x")
            .Add(c => c.Removable, true)
            .Add(c => c.OnRemoved, () => removed = true));

        cut.Find(".bob-chip__remove").Click();
        removed.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool fired = false;
        IRenderedComponent<BOBChip> cut = ctx.Render<BOBChip>(p => p
            .Add(c => c.Label, "x")
            .Add(c => c.Selectable, true)
            .Add(c => c.Disabled, true)
            .Add(c => c.SelectedChanged, _ => fired = true));

        cut.Find(".bob-chip").Click();
        fired.Should().BeFalse();
    }
}

[Trait("Component Snapshots", "BOBChip")]
public class BOBChipSnapshotTests
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
                Name = "Static",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBChip>>)(p => p
                    .Add(c => c.Label, "tag"))
            },
            new
            {
                Name = "With_Icon_And_Count",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBChip>>)(p => p
                    .Add(c => c.Label, "Inbox")
                    .Add(c => c.LeadingIcon, BOBIconKeys.MaterialIconsOutlined.i_label)
                    .Add(c => c.Count, 12))
            },
            new
            {
                Name = "Selectable_On",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBChip>>)(p => p
                    .Add(c => c.Label, "On")
                    .Add(c => c.Selectable, true)
                    .Add(c => c.Selected, true))
            },
            new
            {
                Name = "Removable",
                Builder = (Action<ComponentParameterCollectionBuilder<BOBChip>>)(p => p
                    .Add(c => c.Label, "alpha")
                    .Add(c => c.Removable, true))
            }
        };

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BOBChip> cut = ctx.Render<BOBChip>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToList();

        await Verify(results).UseParameters(scenario.Name);
    }
}
