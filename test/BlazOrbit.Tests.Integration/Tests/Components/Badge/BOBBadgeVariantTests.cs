using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Badge;

[Trait("Component Variants", "BOBBadge")]
public class BOBBadgeVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "1")));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBBadgeVariant custom = BOBBadgeVariant.Custom("pill");
        ctx.Services.AddBlazOrbitVariants(b => b
            .ForComponent<BOBBadge>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "custom-pill-badge");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BOBBadge> cut = ctx.Render<BOBBadge>(p => p
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find("span.custom-pill-badge").Should().NotBeNull();
    }
}
