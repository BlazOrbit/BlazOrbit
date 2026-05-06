using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Button;

[Trait("Component State", "BOBButton")]
public class BOBButtonStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Color_State_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string initialColor = "rgba(255,0,0,1)";
        string updatedColor = "rgba(0,255,0,1)";

        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Color Button")
            .Add(c => c.Color, initialColor));

        // Act
        cut.Render(p => p
            .Add(c => c.Color, updatedColor)
            .Add(c => c.BackgroundColor, "rgba(0,0,255,1)"));

        // Assert
        IElement component = cut.Find("bob-component");
        string style = component.GetAttribute("style");
        style.Should().Contain("--bob-inline-color: rgba(0,255,0,1)");
        style.Should().Contain("--bob-inline-background: rgba(0,0,255,1)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Multiple_State_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Multi-State")
            .Add(c => c.Size, BOBSize.Medium)
            .Add(c => c.Variant, BOBButtonVariant.Default));

        IElement component = cut.Find("bob-component");

        // Act & Assert - Change multiple properties
        cut.Render(p => p
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.Shadow, BOBShadowPresets.Elevation(4)));

        component.GetAttribute("data-bob-size").Should().Be("large");
        component.GetAttribute("data-bob-variant").Should().Be("default");
        component.GetAttribute("data-bob-shadow").Should().Be("true");
        component.GetAttribute("style").Should().Contain("--bob-inline-shadow:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Rapid_State_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int clickCount = 0;
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Rapid Change")
            .Add(c => c.OnClick, _ => clickCount++));

        // Act - Rapid clicks and state changes
        cut.Find("button").Click();
        cut.Render(p => p.Add(c => c.Disabled, true));
        cut.Find("button").Click(); // Should not increment
        cut.Render(p => p.Add(c => c.Disabled, false));
        cut.Find("button").Click();

        // Assert
        clickCount.Should().Be(2); // Only 2 clicks when enabled
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Ripple_Configuration_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Ripple Button")
            .Add(c => c.DisableRipple, false)
            .Add(c => c.RippleColor, "rgba(255,128,0,1)"));

        // Assert â€” ripple color flows into --bob-inline-ripple-color when enabled
        IElement component = cut.Find("bob-component");
        component.GetAttribute("style").Should().Contain("--bob-inline-ripple-color");

        // Act â€” disable ripple; the inline variable goes away
        cut.Render(p => p
            .Add(c => c.DisableRipple, true));

        // Assert
        component.GetAttribute("style")?.Contains("--bob-inline-ripple-color").Should().NotBe(true);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_User_Attributes_Through_State_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Dictionary<string, object> customAttributes = new()
        {
            { "data-testid", "my-button" },
            { "class", "custom-class" },
            { "style", "margin: 10px;" }
        };

        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Button")
            .Add(c => c.AdditionalAttributes, customAttributes));

        // Act - Change other properties
        cut.Render(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.Size, BOBSize.Small));

        // Assert - Custom attributes preserved
        IElement component = cut.Find("bob-component");
        component.GetAttribute("data-testid").Should().Be("my-button");
        component.ClassList.Should().Contain("custom-class");
        component.GetAttribute("style").Should().Contain("margin: 10px;");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Loading_State_Correctly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Submit")
            .Add(c => c.Loading, false));

        IElement component = cut.Find("bob-component");

        // Assert initial state
        component.GetAttribute("data-bob-loading").Should().BeNull();
        cut.Find("button").GetAttribute("disabled").Should().BeNull();
        cut.FindComponents<BOBLoadingIndicator>().Should().BeEmpty();

        // Act - Set loading
        cut.Render(p => p
            .Add(c => c.Loading, true)
            .Add(c => c.LoadingIndicatorVariant, BOBLoadingIndicatorVariant.Dots));

        // Assert loading state
        component.GetAttribute("data-bob-loading").Should().Be("true");
        cut.Find("button").GetAttribute("disabled").Should().NotBeNull();
        cut.FindComponent<BOBLoadingIndicator>().Should().NotBeNull();

        // Act - Clear loading
        cut.Render(p => p
            .Add(c => c.Loading, false));

        // Assert final state
        component.GetAttribute("data-bob-loading").Should().BeNull();
        cut.Find("button").GetAttribute("disabled").Should().BeNull();
        cut.FindComponents<BOBLoadingIndicator>().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Icon_States_Correctly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange - Start with no icons
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Icon Test"));

        // Assert no icons
        cut.FindAll(".bob-button__icon").Should().BeEmpty();

        // Act - Add leading icon
        cut.Render(p => p
            .Add(c => c.LeadingIcon, BOBIconKeys.MaterialIconsOutlined.i_home));

        // Assert leading icon
        cut.FindAll(".bob-button__icon--leading").Should().HaveCount(1);

        // Act - Add trailing icon
        cut.Render(p => p
            .Add(c => c.TrailingIcon, BOBIconKeys.MaterialIconsOutlined.i_arrow_forward));

        // Assert both icons
        cut.FindAll(".bob-button__icon").Should().HaveCount(2);

        // Act - Remove leading icon
        cut.Render(p => p
            .Add(c => c.LeadingIcon, null));

        // Assert only trailing icon
        cut.FindAll(".bob-button__icon--trailing").Should().HaveCount(1);
        cut.FindAll(".bob-button__icon--leading").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_State_When_Parameters_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Initial Text")
            .Add(c => c.Disabled, false));

        // Act - Update text
        cut.Render(p => p
            .Add(c => c.Text, "Updated Text"));

        // Assert
        cut.Find("button").TextContent.Should().Be("Updated Text");

        // Act - Disable button
        cut.Render(p => p
            .Add(c => c.Disabled, true));

        // Assert
        cut.Find("button").GetAttribute("disabled").Should().NotBeNull();
    }
}