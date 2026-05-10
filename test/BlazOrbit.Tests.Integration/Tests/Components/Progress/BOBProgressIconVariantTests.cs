using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Progress;

[Trait("Component Variants", "BOBProgressIcon")]
public class BOBProgressIconVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dots_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBProgressIcon> cut = ctx.Render<BOBProgressIcon>(p => p
            .Add(c => c.Variant, BOBProgressIconVariant.Dots));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("dots");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBProgressIconVariant custom = BOBProgressIconVariant.Custom("pulse");
        ctx.Services.AddBlazOrbitVariants(b => b
            .ForComponent<BOBProgressIcon>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "bob-component");
                builder.AddAttribute(1, "class", "custom-pulse-loader");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BOBProgressIcon> cut = ctx.Render<BOBProgressIcon>(p => p
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find(".custom-pulse-loader").Should().NotBeNull();
    }
}
