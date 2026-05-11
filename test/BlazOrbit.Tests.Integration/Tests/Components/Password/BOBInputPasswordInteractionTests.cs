using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Password;

[Trait("Component Interaction", "BOBInputPassword")]
public class BOBInputPasswordInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Input_Type_On_Eye_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>();

        cut.Find("input.bob-input__field").GetAttribute("type").Should().Be("password");
        cut.Find(".bob-password__toggle").GetAttribute("aria-pressed").Should().Be("false");

        cut.Find(".bob-password__toggle").Click();

        cut.Find("input.bob-input__field").GetAttribute("type").Should().Be("text");
        cut.Find(".bob-password__toggle").GetAttribute("aria-pressed").Should().Be("true");

        cut.Find(".bob-password__toggle").Click();

        cut.Find("input.bob-input__field").GetAttribute("type").Should().Be("password");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Toggle_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".bob-password__toggle").Click();

        cut.Find("input.bob-input__field").GetAttribute("type").Should().Be("password");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Value_When_UpdateOnInput(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? captured = null;
        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.UpdateOnInput, true)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find("input.bob-input__field").Input("hunter2");

        captured.Should().Be("hunter2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Strength_On_Input_Even_Without_UpdateOnInput(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.ShowStrengthMeter, true)
            .Add(c => c.MinLength, 4));

        // Default flags require Upper + Lower + Digit + Symbol + MeetsLength. "Abcd" passes
        // length + upper + lower but fails Digit, which puts it in the Fair tier per the
        // cascading rules in BOBInputPassword.ComputeStrength.
        cut.Find("input.bob-input__field").Input("Abcd");

        cut.Find(".bob-password__strength").GetAttribute("data-bob-strength").Should().Be("fair");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnStrengthChanged_When_Score_Crosses_Threshold(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBPasswordStrength reported = BOBPasswordStrength.None;
        int fireCount = 0;
        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.MinLength, 4)
            .Add(c => c.OnStrengthChanged, s => { reported = s; fireCount++; }));

        cut.Find("input.bob-input__field").Input("Abcd");
        reported.Should().Be(BOBPasswordStrength.Fair);
        fireCount.Should().Be(1);

        cut.Find("input.bob-input__field").Input("Abcd1234!");
        reported.Should().Be(BOBPasswordStrength.Strong);
        fireCount.Should().Be(2);

        // Same strength on next input should not re-fire.
        cut.Find("input.bob-input__field").Input("Abcd1234!@");
        fireCount.Should().Be(2);
    }
}
