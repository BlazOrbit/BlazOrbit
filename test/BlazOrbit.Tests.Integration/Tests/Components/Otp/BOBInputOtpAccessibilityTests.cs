using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Otp;

[Trait("Component Accessibility", "BOBInputOtp")]
public class BOBInputOtpAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Slot_Group_With_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>();

        cut.Find(".bob-otp__slots").GetAttribute("role").Should().Be("group");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_Group_To_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Label, "Code"));

        IElement group = cut.Find(".bob-otp__slots");
        IElement label = cut.Find(".bob-otp__label");

        string? labelledBy = group.GetAttribute("aria-labelledby");
        labelledBy.Should().NotBeNullOrEmpty();
        label.GetAttribute("id").Should().Be(labelledBy);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Number_Slots_Per_Aria_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4));

        IReadOnlyList<IElement> slots = cut.FindAll(".bob-otp__slot");
        slots[0].GetAttribute("aria-label").Should().Be("Digit 1 of 4");
        slots[3].GetAttribute("aria-label").Should().Be("Digit 4 of 4");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Required_When_Required(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Required, true));

        cut.FindAll(".bob-otp__slot")[0].GetAttribute("aria-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Aria_Invalid_When_Error(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Error, true));

        cut.FindAll(".bob-otp__slot")[0].GetAttribute("aria-invalid").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_One_Time_Code_Autocomplete_On_First_Slot_Only(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputOtp> cut = ctx.Render<BOBInputOtp>(p => p
            .Add(c => c.Length, 4));

        IReadOnlyList<IElement> slots = cut.FindAll(".bob-otp__slot");
        slots[0].GetAttribute("autocomplete").Should().Be("one-time-code");
        slots[1].GetAttribute("autocomplete").Should().Be("off");
    }
}
