using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Svg;

[Trait("Component Variants", "BOBSvgIcon")]
public class BOBSvgIconVariantTests
{
    private static readonly IconKey SimpleIcon = new("SimpleIcon") { SvgContent = "<path d=\"M12 2L2 22h20L12 2z\"/>" };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, SimpleIcon));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBSvgIconVariant custom = BOBSvgIconVariant.Custom("outlined");
        ctx.Services.AddBlazOrbitVariants(b => b
            .ForComponent<BOBSvgIcon>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "custom-outlined-icon");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, SimpleIcon)
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find(".custom-outlined-icon").Should().NotBeNull();
    }
}
