using AngleSharp.Dom;
using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Card;

[Trait("Component Accessibility", "BOBCard")]
public class BOBCardAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Root_With_Clickable_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.Clickable, true)
            .Add(c => c.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => { }))
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));

        // Assert — SR/CSS can key off this attribute
        cut.Find("bob-component").GetAttribute("data-bob-clickable").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Report_NonClickable_State_On_Root(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-clickable").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Forward_Role_And_Tabindex_From_AdditionalAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — BOBCard is semantically-neutral; consumers opt in via
        // AdditionalAttributes. Verify they reach the root element.
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.Clickable, true)
            .Add(c => c.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => { }))
            .AddUnmatched("role", "button")
            .AddUnmatched("tabindex", "0")
            .AddUnmatched("aria-label", "Open details")
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));

        // Assert
        IElement root = cut.Find("bob-component");
        root.GetAttribute("role").Should().Be("button");
        root.GetAttribute("tabindex").Should().Be("0");
        root.GetAttribute("aria-label").Should().Be("Open details");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_Focusable_Semantics_When_Not_Clickable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));

        // Assert — framework emits none by default; presentational by default
        IElement root = cut.Find("bob-component");
        root.HasAttribute("role").Should().BeFalse();
        root.HasAttribute("tabindex").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_Heading_Semantics_In_Header_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.Header, b => b.AddMarkupContent(0, "<h3 class='card-title'>Title</h3>"))
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));

        // Assert — heading lives inside the card header slot (document outline preserved)
        cut.Find(".bob-card__header h3.card-title").TextContent.Should().Be("Title");
    }
}
