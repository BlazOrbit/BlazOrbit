using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Card;

[Trait("Component Variants", "BOBCard")]
public class BOBCardVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBCardVariant custom = BOBCardVariant.Custom("Outlined");
        ctx.Services.AddBlazOrbitVariants(b => b
            .ForComponent<BOBCard>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-card-outlined");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BOBCard> cut = ctx.Render<BOBCard>(p => p
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find(".custom-card-outlined").Should().NotBeNull();
    }
}
