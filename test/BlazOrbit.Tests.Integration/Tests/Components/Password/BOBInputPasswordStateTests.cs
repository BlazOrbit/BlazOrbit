using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Password;

[Trait("Component State", "BOBInputPassword")]
public class BOBInputPasswordStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>();

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-disabled").Should().BeNull();

        cut.Render(p => p.Add(c => c.Disabled, true));

        root.GetAttribute("data-bob-disabled").Should().Be("true");
        cut.Find("input.bob-input__field").HasAttribute("disabled").Should().BeTrue();
        cut.Find(".bob-password__toggle").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>();

        cut.Render(p => p.Add(c => c.ReadOnly, true));

        cut.Find("bob-component").GetAttribute("data-bob-readonly").Should().Be("true");
        cut.Find("input.bob-input__field").HasAttribute("readonly").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>();

        cut.Render(p => p.Add(c => c.Error, true));

        cut.Find("bob-component").GetAttribute("data-bob-error").Should().Be("true");
        cut.Find("input.bob-input__field").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Compute_Strength_From_Initial_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.ShowStrengthMeter, true)
            .Add(c => c.MinLength, 8)
            .Add(c => c.Value, "Aa1!aaaa1234"));

        cut.Find(".bob-password__strength").GetAttribute("data-bob-strength").Should().Be("strong");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Score_Weak_When_Below_MinLength(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.ShowStrengthMeter, true)
            .Add(c => c.MinLength, 8)
            .Add(c => c.Value, "abc"));

        cut.Find(".bob-password__strength").GetAttribute("data-bob-strength").Should().Be("weak");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Score_Weak_When_Required_Class_Missing(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputPassword> cut = ctx.Render<BOBInputPassword>(p => p
            .Add(c => c.ShowStrengthMeter, true)
            .Add(c => c.MinLength, 8)
            .Add(c => c.Value, "abcdefghi"));

        cut.Find(".bob-password__strength").GetAttribute("data-bob-strength").Should().Be("weak");
    }
}