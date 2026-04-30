using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Button;

[Trait("Component Variants", "BOBButton")]
public class BOBButtonVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBButtonVariant customVariant = BOBButtonVariant.Custom("GlassButton");

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<BOBButton>()
                   .AddVariant(
                       customVariant,
                       button => builder =>
                       {
                           builder.OpenElement(0, "bob-component");
                           builder.AddAttribute(1, "class", "glass-button");
                           builder.AddContent(2, button.Text);
                           builder.CloseElement();
                       }));

        // Act
        IRenderedComponent<BOBButton> cut = ctx.Render<BOBButton>(p => p
            .Add(c => c.Text, "Glass Button")
            .Add(c => c.Variant, customVariant));

        // Assert
        cut.Find(".glass-button").Should().NotBeNull();
        cut.Markup.Should().Contain("Glass Button");
    }
}