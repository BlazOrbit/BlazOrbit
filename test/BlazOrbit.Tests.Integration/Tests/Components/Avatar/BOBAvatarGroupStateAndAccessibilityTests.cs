using BlazOrbit.Components;
using BlazOrbit.Components.Display;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Avatar;

[Trait("Component State", "BOBAvatarGroup")]
public class BOBAvatarGroupStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Aria_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>(p => p
            .Add(c => c.AriaLabel, "Team members"));

        cut.Find("[role='group']").GetAttribute("aria-label").Should().Be("Team members");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_MaxVisible_Style(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>(p => p
            .Add(c => c.MaxVisible, 2));

        cut.Find(".bob-avatar-group").GetAttribute("style").Should().Contain("--_group-max-visible: 2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>(p => p
            .Add(c => c.Size, BOBSize.Large));

        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
    }
}

[Trait("Component Accessibility", "BOBAvatarGroup")]
public class BOBAvatarGroupAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Role_Group(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>();

        cut.Find("[role='group']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Label_On_Group(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>(p => p
            .Add(c => c.AriaLabel, "Contributors"));

        cut.Find("[role='group']").GetAttribute("aria-label").Should().Be("Contributors");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Label_On_Overflow(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>(p => p
            .Add(c => c.MaxVisible, 2)
            .Add(c => c.TotalOverride, 5));

        cut.Find(".bob-avatar-group__overflow").GetAttribute("aria-label").Should().Be("+3 more");
    }
}
