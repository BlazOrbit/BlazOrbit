using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Otp;

[Trait("Component Rendering", "BOBInputOtp")]
public class BOBInputOtpRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Label, "Code"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-otp");
        root.GetAttribute("data-bob-variant").Should().Be("boxed");
        root.GetAttribute("data-bob-size").Should().Be("medium");
        root.GetAttribute("data-bob-disabled").Should().BeNull();
        root.GetAttribute("data-bob-error").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Length_Of_Six_Slots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>();

        cut.FindAll(".bob-otp__slot").Should().HaveCount(6);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Length(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4));

        cut.FindAll(".bob-otp__slot").Should().HaveCount(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clamp_Length_To_Sane_Range(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> tooSmall = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 1));
        tooSmall.FindAll(".bob-otp__slot").Should().HaveCount(2);

        IRenderedComponent<BOBInputOtp> tooBig = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 25));
        tooBig.FindAll(".bob-otp__slot").Should().HaveCount(12);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Label, "Verification code")
            .Add(c => c.HelperText, "Sent via SMS."));

        cut.Find(".bob-otp__label").TextContent.Should().Contain("Verification code");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("Sent via SMS.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_Marker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Label, "Code")
            .Add(c => c.Required, true));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-required").Should().Be("true");
        cut.Find(".bob-field__required").TextContent.Should().Contain("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hydrate_Slots_From_Initial_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4)
            .Add(c => c.Value, "12"));

        IReadOnlyList<IElement> slots = cut.FindAll(".bob-otp__slot");
        slots[0].GetAttribute("value").Should().Be("1");
        slots[1].GetAttribute("value").Should().Be("2");
        slots[2].GetAttribute("value").Should().BeEmpty();
        slots[3].GetAttribute("value").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Mask_As_Bullet_When_Mask_Enabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 3)
            .Add(c => c.Mask, true)
            .Add(c => c.Value, "123"));

        IReadOnlyList<IElement> slots = cut.FindAll(".bob-otp__slot");
        slots[0].GetAttribute("type").Should().Be("password");
        slots[0].GetAttribute("value").Should().Be("•");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Inputmode_Numeric_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>();

        IElement first = cut.FindAll(".bob-otp__slot")[0];
        first.GetAttribute("inputmode").Should().Be("numeric");
        first.GetAttribute("pattern").Should().Be("[0-9]*");
        first.GetAttribute("autocomplete").Should().Be("one-time-code");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Drop_Numeric_Pattern_When_Numeric_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Numeric, false));

        IElement first = cut.FindAll(".bob-otp__slot")[0];
        first.GetAttribute("inputmode").Should().Be("text");
        first.GetAttribute("pattern").Should().BeNull();
    }
}