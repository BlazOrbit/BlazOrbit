using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Switch;

[Trait("Component Variants", "BOBInputSwitch")]
public class BOBInputSwitchVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>();

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBInputSwitchVariant customVariant = BOBInputSwitchVariant.Custom("Pill");

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<BOBInputSwitch>()
                   .AddVariant(
                       customVariant,
                       sw => builder =>
                       {
                           builder.OpenElement(0, "bob-component");
                           builder.AddAttribute(1, "class", "pill-switch");
                           builder.AddContent(2, sw.Label);
                           builder.CloseElement();
                       }));

        // Act
        IRenderedComponent<BOBInputSwitch> cut = ctx.Render<BOBInputSwitch>(p => p
            .Add(c => c.Label, "Pill Toggle")
            .Add(c => c.Variant, customVariant));

        // Assert
        cut.Find(".pill-switch").Should().NotBeNull();
        cut.Markup.Should().Contain("Pill Toggle");
    }
}
