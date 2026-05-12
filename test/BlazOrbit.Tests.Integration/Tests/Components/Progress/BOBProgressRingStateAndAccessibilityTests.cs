using BlazOrbit.Components;
using BlazOrbit.Components.Display;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Progress;

[Trait("Component State", "BOBProgressRing")]
public class BOBProgressRingStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_Value_To_Max(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>(p => p
            .Add(c => c.Value, 150)
            .Add(c => c.ShowPercentage, true));

        cut.Find(".bob-progress-ring__label").TextContent.Should().Be("100%");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_Value_To_Min(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>(p => p
            .Add(c => c.Value, -20)
            .Add(c => c.ShowPercentage, true));

        cut.Find(".bob-progress-ring__label").TextContent.Should().Be("0%");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Max_Equals_Min(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>(p => p
            .Add(c => c.Value, 50)
            .Add(c => c.Min, 100)
            .Add(c => c.Max, 100)
            .Add(c => c.ShowPercentage, true));

        cut.Find(".bob-progress-ring__label").TextContent.Should().Be("0%");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>(p => p
            .Add(c => c.Size, BOBSize.Small));

        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("small");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_ValueNow(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>(p => p
            .Add(c => c.Value, 42.7));

        cut.Find("[role='progressbar']").GetAttribute("aria-valuenow").Should().Be("43");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Aria_ValueNow_When_Indeterminate(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>(p => p
            .Add(c => c.Indeterminate, true));

        cut.Find("[role='progressbar']").GetAttribute("aria-valuenow").Should().BeNull();
    }
}

[Trait("Component Accessibility", "BOBProgressRing")]
public class BOBProgressRingAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Default_Aria_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>();

        cut.Find("[role='progressbar']").GetAttribute("aria-label").Should().Be("Progress");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Custom_Aria_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBProgressRing> cut = ctx.Render<BOBProgressRing>(p => p
            .Add(c => c.AriaLabel, "Uploading file"));

        cut.Find("[role='progressbar']").GetAttribute("aria-label").Should().Be("Uploading file");
    }
}