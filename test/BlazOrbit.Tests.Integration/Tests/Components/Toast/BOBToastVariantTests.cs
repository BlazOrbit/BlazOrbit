using BlazOrbit.Components.Layout;
using BlazOrbit.Components.Layout.Services;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Toast;

[Trait("Component Variants", "BOBToast")]
public class BOBToastVariantTests
{
    private static ToastState DefaultState() => new()
    {
        Content = b => b.AddContent(0, "msg"),
        Options = ToastOptions.Default
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, DefaultState()));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBToastVariant custom = BOBToastVariant.Custom("Minimal");
        ctx.Services.AddBlazOrbitVariants(b => b
            .ForComponent<BOBToast>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-toast-minimal");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BOBToast> cut = ctx.Render<BOBToast>(p => p
            .Add(c => c.State, DefaultState())
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find(".custom-toast-minimal").Should().NotBeNull();
    }
}
