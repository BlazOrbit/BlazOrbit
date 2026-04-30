using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Number;

[Trait("Component Variants", "BOBInputNumber")]
public class BOBInputNumberVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_Outlined_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>();

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("outlined");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Filled_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Variant, BOBInputVariant.Filled));

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("filled");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Standard_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Variant, BOBInputVariant.Standard));

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("standard");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBInputVariant customVariant = BOBInputVariant.Custom("NeonNumber");

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<BOBInputNumber<int>>()
                   .AddVariant(
                       customVariant,
                       input => builder =>
                       {
                           builder.OpenElement(0, "bob-component");
                           builder.AddAttribute(1, "class", "neon-number");
                           builder.AddContent(2, input.Label);
                           builder.CloseElement();
                       }));

        // Act
        IRenderedComponent<BOBInputNumber<int>> cut = ctx.Render<BOBInputNumber<int>>(p => p
            .Add(c => c.Label, "Qty")
            .Add(c => c.Variant, customVariant));

        // Assert
        cut.Find(".neon-number").Should().NotBeNull();
        cut.Markup.Should().Contain("Qty");
    }
}
