using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Number;

[Trait("Component Rendering", "BOBInputNumber")]
public class BOBInputNumberRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int?>> cut = ctx.Render<BOBInputNumber<int?>>(p => p
            .Add(c => c.Label, "Quantity"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-number");
        root.GetAttribute("data-bob-input-base").Should().NotBeNull();
        root.GetAttribute("data-bob-variant").Should().Be("outlined");
        root.GetAttribute("data-bob-size").Should().Be("medium");
        root.GetAttribute("data-bob-density").Should().Be("standard");
        root.GetAttribute("data-bob-disabled").Should().Be("false");
        root.GetAttribute("data-bob-floated").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Step_Buttons_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Label, "Count"));

        cut.Find(".bob-input__step-buttons").Should().NotBeNull();
        cut.Find("bob-component").GetAttribute("data-bob-button-placement").Should().Be("right");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Step_Buttons_When_ShowStepButtons_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.ShowStepButtons, false));

        cut.FindAll(".bob-input__step-buttons").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Label, "Amount")
            .Add(c => c.HelperText, "Enter a number."));

        cut.Find("label.bob-input__label").TextContent.Should().Contain("Amount");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("Enter a number.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Prefix_And_Suffix(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<decimal>> cut = ctx.Render<BOBInputNumber<decimal>>(p => p
            .Add(c => c.PrefixText, "$")
            .Add(c => c.SuffixText, "USD"));

        cut.Find(".bob-input__addon--prefix").TextContent.Should().Contain("$");
        cut.Find(".bob-input__addon--suffix").TextContent.Should().Contain("USD");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Floated_When_HasValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Label, "Qty")
            .Add(c => c.Value, 42));

        cut.Find("bob-component").GetAttribute("data-bob-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ButtonPlacement_Left(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.ButtonPlacement, StepButtonPlacement.Left));

        cut.Find("bob-component").GetAttribute("data-bob-button-placement").Should().Be("left");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Design_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Size, BOBSize.Large)
            .Add(c => c.Density, BOBDensity.Compact)
            .Add(c => c.Color, "rgba(10,20,30,1)"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-size").Should().Be("large");
        root.GetAttribute("data-bob-density").Should().Be("compact");
        root.GetAttribute("style").Should().Contain("--bob-inline-color: rgba(10,20,30,1)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Disable_Increment_When_No_Max(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Value, 10));

        // Assert
        IElement incrementButton = cut.Find(".bob-input__step-buttons button[aria-label='Increment']");
        incrementButton.HasAttribute("disabled").Should().BeFalse();
    }
}
