using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.TextArea;

[Trait("Component Variants", "BOBInputTextArea")]
public class BOBInputTextAreaVariantTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_Outlined_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("outlined");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Filled_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Variant, BOBInputVariant.Filled));

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("filled");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Standard_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Variant, BOBInputVariant.Standard));

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("standard");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBInputVariant customVariant = BOBInputVariant.Custom("NeonArea");

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<BOBInputTextArea>()
                   .AddVariant(
                       customVariant,
                       input => builder =>
                       {
                           builder.OpenElement(0, "bob-component");
                           builder.AddAttribute(1, "class", "neon-area");
                           builder.AddContent(2, input.Label);
                           builder.CloseElement();
                       }));

        Model model = new();

        IRenderedComponent<BOBInputTextArea> cut = ctx.Render<BOBInputTextArea>(p => p
            .Add(c => c.Label, "Notes")
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Variant, customVariant));

        cut.Find(".neon-area").Should().NotBeNull();
        cut.Markup.Should().Contain("Notes");
    }
}
