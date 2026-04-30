using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Badge;

[Trait("Component Variants", "BOBNotificationBadge")]
public class BOBNotificationBadgeVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>();

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBBadgeVariant custom = BOBBadgeVariant.Custom("alert-pulse");
        ctx.Services.AddBlazOrbitVariants(b => b
            .ForComponent<BOBNotificationBadge>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "custom-notification-pulse");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BOBNotificationBadge> cut = ctx.Render<BOBNotificationBadge>(p => p
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find("span.custom-notification-pulse").Should().NotBeNull();
    }
}
