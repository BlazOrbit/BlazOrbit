using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Button;

[Trait("Component Accessibility", "BOBButton")]
public class BOBButtonAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_On_Inner_Button_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Click me")
            .Add(c => c.Disabled, true));

        // Assert — HTML disabled attribute on the inner <button>
        cut.Find("button").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Data_Bob_Disabled_On_Root_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Click me")
            .Add(c => c.Disabled, true));

        // Assert — data-bob-disabled on root element
        cut.Find("bob-component").GetAttribute("data-bob-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Data_Bob_Loading_On_Root_When_Loading(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Loading")
            .Add(c => c.Loading, true));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Inner_Button_When_Loading(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — Loading also disables the button
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Loading")
            .Add(c => c.Loading, true));

        // Assert — loading makes button disabled
        cut.Find("button").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Aria_Label_To_Root_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — aria-label passes through AdditionalAttributes to bob-component root
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Icon button")
            .AddUnmatched("aria-label", "Close dialog"));

        // Assert
        cut.Find("bob-component").GetAttribute("aria-label").Should().Be("Close dialog");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Aria_Label_On_Inner_Button_When_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — AriaLabel parameter forwards to the focusable <button>,
        // not just the bob-component wrapper, so screen readers pick it up.
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.LeadingIcon, BOBIconKeys.UI.Close)
            .Add(c => c.AriaLabel, "Close dialog"));

        // Assert
        cut.Find("button").GetAttribute("aria-label").Should().Be("Close dialog");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_Aria_Label_When_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Save"));

        // Assert — Razor omits the attribute when the bound value is null
        cut.Find("button").HasAttribute("aria-label").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Aria_Busy_And_Live_When_Loading(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — WCAG 4.1.2: busy state of the control must be programmatically
        // determinable; aria-live announces the loading transition to screen readers.
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Saving")
            .Add(c => c.Loading, true));

        // Assert
        IElement button = cut.Find("button");
        button.GetAttribute("aria-busy").Should().Be("true");
        button.GetAttribute("aria-live").Should().Be("polite");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_Aria_Busy_When_Not_Loading(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Save"));

        // Assert — `aria-busy="false"` is technically valid but adds noise; we omit it
        // entirely when not loading and let assistive tech default to "not busy".
        IElement button = cut.Find("button");
        button.HasAttribute("aria-busy").Should().BeFalse();
        button.HasAttribute("aria-live").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Aria_Busy_When_Loading_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — start not loading
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Save"));
        cut.Find("button").HasAttribute("aria-busy").Should().BeFalse();

        // Act — flip to loading
        cut.Render(p => p
            .Add(c => c.Text, "Save")
            .Add(c => c.Loading, true));

        // Assert
        cut.Find("button").GetAttribute("aria-busy").Should().Be("true");

        // Act — flip back
        cut.Render(p => p
            .Add(c => c.Text, "Save")
            .Add(c => c.Loading, false));

        // Assert
        cut.Find("button").HasAttribute("aria-busy").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_Click_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int clickCount = 0;
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Disabled")
            .Add(c => c.Disabled, true)
            .Add(c => c.OnClick, _ => clickCount++));

        // Act — click on a disabled button should not fire the callback
        try { cut.Find("button").Click(); } catch { /* bunit may throw for disabled */ }

        // Assert
        clickCount.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Button_With_Type_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Submit"));

        // Assert — type="button" prevents accidental form submission
        cut.Find("button").GetAttribute("type").Should().Be("button");
    }
}
