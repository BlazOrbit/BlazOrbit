using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Otp;

[Trait("Component State", "BOBInputOtp")]
public class BOBInputOtpStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Disabled, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-disabled").Should().BeNull();

        cut.Render(p => p.Add(c => c.Disabled, true));

        root.GetAttribute("data-bob-disabled").Should().Be("true");
        cut.FindAll(".bob-otp__slot")[0].HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.ReadOnly, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-readonly").Should().BeNull();

        cut.Render(p => p.Add(c => c.ReadOnly, true));

        root.GetAttribute("data-bob-readonly").Should().Be("true");
        cut.FindAll(".bob-otp__slot")[0].HasAttribute("readonly").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Error, false));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-error").Should().BeNull();

        cut.Render(p => p.Add(c => c.Error, true));

        root.GetAttribute("data-bob-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Resize_Buffer_When_Length_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4)
            .Add(c => c.Value, "1234"));

        cut.FindAll(".bob-otp__slot").Should().HaveCount(4);

        cut.Render(p => p
            .Add(c => c.Length, 6)
            .Add(c => c.Value, "1234"));

        IReadOnlyList<IElement> slots = cut.FindAll(".bob-otp__slot");
        slots.Should().HaveCount(6);
        slots[0].GetAttribute("value").Should().Be("1");
        slots[3].GetAttribute("value").Should().Be("4");
        slots[4].GetAttribute("value").Should().BeEmpty();
        slots[5].GetAttribute("value").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Truncate_Initial_Value_To_Length(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4)
            .Add(c => c.Value, "12345678"));

        IReadOnlyList<IElement> slots = cut.FindAll(".bob-otp__slot");
        slots.Should().HaveCount(4);
        slots[0].GetAttribute("value").Should().Be("1");
        slots[3].GetAttribute("value").Should().Be("4");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Label, "Old")
            .Add(c => c.HelperText, "Old help"));

        cut.Find(".bob-otp__label").TextContent.Should().Contain("Old");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("Old help");

        cut.Render(p => p
            .Add(c => c.Label, "New")
            .Add(c => c.HelperText, "New help"));

        cut.Find(".bob-otp__label").TextContent.Should().Contain("New");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("New help");
    }
}
