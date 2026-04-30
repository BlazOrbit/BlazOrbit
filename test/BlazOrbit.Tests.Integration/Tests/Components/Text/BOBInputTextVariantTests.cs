using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Text;

[Trait("Component Variants", "BOBInputText")]
public class BOBInputTextVariantTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_Outlined_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("outlined");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Filled_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
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
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Variant, BOBInputVariant.Standard));

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("standard");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBInputVariant customVariant = BOBInputVariant.Custom("GlassInput");

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<BOBInputText>()
                   .AddVariant(
                       customVariant,
                       input => builder =>
                       {
                           builder.OpenElement(0, "bob-component");
                           builder.AddAttribute(1, "class", "glass-input");
                           builder.AddContent(2, input.Label);
                           builder.CloseElement();
                       }));

        Model model = new();

        // Act
        IRenderedComponent<BOBInputText> cut = ctx.Render<BOBInputText>(p => p
            .Add(c => c.Label, "My Label")
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Variant, customVariant));

        // Assert
        cut.Find(".glass-input").Should().NotBeNull();
        cut.Markup.Should().Contain("My Label");
    }
}
