using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Radio;

[Trait("Component Rendering", "BOBInputRadio")]
public class BOBInputRadioRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.Label, "Pick one"));

        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be("input-radio");
        root.GetAttribute("data-bob-variant").Should().Be("default");
        root.GetAttribute("data-bob-size").Should().Be("medium");
        root.GetAttribute("data-bob-disabled").Should().Be("false");
        root.GetAttribute("data-bob-error").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Orientation_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.Orientation, RadioOrientation.Horizontal));

        cut.Find("bob-component").GetAttribute("data-bob-orientation").Should().Be("horizontal");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Three_Options(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>();

        cut.FindAll(".bob-radio__option").Should().HaveCount(3);
        cut.FindAll(".bob-radio__option").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.Label, "Choose")
            .Add(c => c.HelperText, "Select one option."));

        cut.Find(".bob-radio__label").TextContent.Should().Contain("Choose");
        cut.Find(".bob-field-helper").TextContent.Should().Contain("Select one option.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_Marker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.Label, "Choice")
            .Add(c => c.Required, true));

        cut.Find("bob-component").GetAttribute("data-bob-required").Should().Be("true");
        cut.Find(".bob-field__required").TextContent.Should().Contain("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Radiogroup_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>();

        cut.Find("[role='radiogroup']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Selected_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>(p => p
            .Add(c => c.SelectedValue, "opt2"));

        IReadOnlyList<IElement> options = cut.FindAll(".bob-radio__option");
        options[0].GetAttribute("aria-checked").Should().Be("false");
        options[1].GetAttribute("aria-checked").Should().Be("true");
        options[2].GetAttribute("aria-checked").Should().Be("false");
    }
}
