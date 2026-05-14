using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Select;

[Trait("Component Accessibility", "BOBSelect")]
public class BOBSelectAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Required_On_Select(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Required, true));

        cut.Find("select").GetAttribute("aria-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Required_False_When_Not_Required(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Required, false));

        cut.Find("select").GetAttribute("aria-required").Should().Be("false");
    }
}