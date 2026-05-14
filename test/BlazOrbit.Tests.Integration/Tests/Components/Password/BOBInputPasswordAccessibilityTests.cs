using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Password;

[Trait("Component Accessibility", "BOBInputPassword")]
public class BOBInputPasswordAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Required_When_Required(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.Required, true));

        cut.Find("input.bob-input__field").GetAttribute("aria-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Aria_Pressed_On_Reveal(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>();

        cut.Find(".bob-password__toggle").GetAttribute("aria-pressed").Should().Be("false");
        cut.Find(".bob-password__toggle").GetAttribute("aria-label").Should().Be("Show password");

        cut.Find(".bob-password__toggle").Click();

        cut.Find(".bob-password__toggle").GetAttribute("aria-pressed").Should().Be("true");
        cut.Find(".bob-password__toggle").GetAttribute("aria-label").Should().Be("Hide password");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Toggle_From_Tab_Order(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>();

        cut.Find(".bob-password__toggle").GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reference_Strength_From_Aria_Describedby(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.ShowStrengthMeter, true));

        string? describedBy = cut.Find("input.bob-input__field").GetAttribute("aria-describedby");
        string? strengthId = cut.Find(".bob-password__strength").GetAttribute("id");

        describedBy.Should().NotBeNullOrEmpty();
        strengthId.Should().NotBeNullOrEmpty();
        describedBy.Should().Contain(strengthId!);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Strength_Live_Region_Polite(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.ShowStrengthMeter, true));

        cut.Find(".bob-password__strength").GetAttribute("aria-live").Should().Be("polite");
    }
}