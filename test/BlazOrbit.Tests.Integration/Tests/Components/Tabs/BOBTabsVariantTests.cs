using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Tabs;

[Trait("Component Variants", "BOBTabs")]
public class BOBTabsVariantTests
{
    private static RenderFragment OneTab => b =>
    {
        b.OpenComponent<BOBTab>(0);
        b.AddAttribute(1, "Id", "t1");
        b.AddAttribute(2, "Label", "T1");
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Pills_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, OneTab)
            .Add(c => c.Variant, BOBTabsVariant.Pills));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("pills");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Enclosed_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, OneTab)
            .Add(c => c.Variant, BOBTabsVariant.Enclosed));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("enclosed");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBTabsVariant custom = BOBTabsVariant.Custom("segmented");
        ctx.Services.AddBlazOrbitVariants(b => b
            .ForComponent<BOBTabs>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "bob-component");
                builder.AddAttribute(1, "class", "custom-segmented-tabs");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BOBTabs> cut = ctx.Render<BOBTabs>(p => p
            .Add(c => c.ChildContent, OneTab)
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find(".custom-segmented-tabs").Should().NotBeNull();
    }
}
