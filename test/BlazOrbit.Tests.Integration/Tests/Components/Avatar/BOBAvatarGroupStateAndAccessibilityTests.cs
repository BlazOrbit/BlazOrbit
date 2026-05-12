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

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Auto_Count_Overflow_From_Registered_Children(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // 4 nested avatars + MaxVisible=2 => +2 overflow without TotalOverride.
        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>(p => p
            .Add(c => c.MaxVisible, 2)
            .AddChildContent<BOBAvatar>(a => a.Add(b => b.Label, "Alice"))
            .AddChildContent<BOBAvatar>(a => a.Add(b => b.Label, "Bob"))
            .AddChildContent<BOBAvatar>(a => a.Add(b => b.Label, "Carol"))
            .AddChildContent<BOBAvatar>(a => a.Add(b => b.Label, "Dave")));

        cut.Find(".bob-avatar-group__overflow").TextContent.Should().Be("+2");
        cut.Instance.RegisteredCount.Should().Be(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Overflow_When_Below_MaxVisible(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // 2 children, MaxVisible=5 => no overflow badge.
        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>(p => p
            .Add(c => c.MaxVisible, 5)
            .AddChildContent<BOBAvatar>(a => a.Add(b => b.Label, "Alice"))
            .AddChildContent<BOBAvatar>(a => a.Add(b => b.Label, "Bob")));

        cut.FindAll(".bob-avatar-group__overflow").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Prefer_TotalOverride_Over_Auto_Count(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // 2 visible children but the data source claims 10 total => +8 overflow honours
        // the explicit override even though only 2 avatars are registered.
        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>(p => p
            .Add(c => c.MaxVisible, 2)
            .Add(c => c.TotalOverride, 10)
            .AddChildContent<BOBAvatar>(a => a.Add(b => b.Label, "Alice"))
            .AddChildContent<BOBAvatar>(a => a.Add(b => b.Label, "Bob")));

        cut.Find(".bob-avatar-group__overflow").TextContent.Should().Be("+8");
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