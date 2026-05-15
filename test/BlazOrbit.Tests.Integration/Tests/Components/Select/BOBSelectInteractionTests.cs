using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Select;

[Trait("Component Interaction", "BOBSelect")]
public class BOBSelectInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Invoke_ValueChanged_On_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? changedValue = null;
        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Value, "a")
            .Add(c => c.ValueChanged, v => changedValue = v)
            .Add(c => c.ChildContent, b => b.AddMarkupContent(0,
                "<option value='a'>A</option><option value='b'>B</option>")));

        cut.Find("select").Change("b");

        changedValue.Should().Be("b");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Invoke_ValueChanged_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool fired = false;
        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Value, "a")
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, _ => fired = true))
            .Add(c => c.ChildContent, b => b.AddMarkupContent(0,
                "<option value='a'>A</option><option value='b'>B</option>")));

        cut.Find("select").Change("b");

        fired.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Invoke_ValueChanged_When_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool fired = false;
        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Value, "a")
            .Add(c => c.ReadOnly, true)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, _ => fired = true))
            .Add(c => c.ChildContent, b => b.AddMarkupContent(0,
                "<option value='a'>A</option><option value='b'>B</option>")));

        cut.Find("select").Change("b");

        fired.Should().BeFalse();
    }
}
