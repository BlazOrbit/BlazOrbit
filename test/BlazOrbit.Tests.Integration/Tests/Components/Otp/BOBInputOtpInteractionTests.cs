using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Otp;

[Trait("Component Interaction", "BOBInputOtp")]
public class BOBInputOtpInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Value_When_Slot_Receives_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? captured = null;
        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-otp__slot")[0].Input("1");
        cut.FindAll(".bob-otp__slot")[1].Input("2");

        captured.Should().Be("12");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Filter_Non_Digit_When_Numeric(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? captured = null;
        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-otp__slot")[0].Input("a");

        captured.Should().BeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Accept_Letters_When_Numeric_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? captured = null;
        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4)
            .Add(c => c.Numeric, false)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-otp__slot")[0].Input("a");

        captured.Should().Be("a");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Distribute_Pasted_Code_Across_Slots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? captured = null;
        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 6)
            .Add(c => c.ValueChanged, v => captured = v));

        // Paste lands on the first slot as a multi-char input event.
        cut.FindAll(".bob-otp__slot")[0].Input("123456");

        captured.Should().Be("123456");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Drop_Non_Digit_Chars_From_Pasted_Code_When_Numeric(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? captured = null;
        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 6)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-otp__slot")[0].Input("12-3456");

        captured.Should().Be("123456");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Truncate_Pasted_Code_Beyond_Length(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? captured = null;
        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-otp__slot")[0].Input("123456789");

        captured.Should().Be("1234");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Slot_On_Backspace(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? captured = null;
        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4)
            .Add(c => c.Value, "12")
            .Add(c => c.ValueChanged, v => captured = v));

        cut.FindAll(".bob-otp__slot")[1].KeyDown("Backspace");

        captured.Should().Be("1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Previous_Slot_On_Backspace_When_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? captured = null;
        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4)
            .Add(c => c.Value, "12")
            .Add(c => c.ValueChanged, v => captured = v));

        // Slot 2 is empty (Value="12" fills slots 0 and 1). Backspace there hops back to slot 1.
        cut.FindAll(".bob-otp__slot")[2].KeyDown("Backspace");

        captured.Should().Be("1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnComplete_When_All_Slots_Filled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string? completed = null;
        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 3)
            .Add(c => c.OnComplete, code => completed = code));

        cut.FindAll(".bob-otp__slot")[0].Input("1");
        cut.FindAll(".bob-otp__slot")[1].Input("2");
        cut.FindAll(".bob-otp__slot")[2].Input("3");

        completed.Should().Be("123");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnComplete_Once_Per_Fill_Cycle(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        int fireCount = 0;
        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 3)
            .Add(c => c.OnComplete, _ => fireCount++));

        cut.FindAll(".bob-otp__slot")[0].Input("1");
        cut.FindAll(".bob-otp__slot")[1].Input("2");
        cut.FindAll(".bob-otp__slot")[2].Input("3");

        fireCount.Should().Be(1);

        // Backspace then refill should re-fire.
        cut.FindAll(".bob-otp__slot")[2].KeyDown("Backspace");
        cut.FindAll(".bob-otp__slot")[2].Input("4");

        fireCount.Should().Be(2);
    }
}