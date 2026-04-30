using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Checkbox;

[Trait("Component Variants", "BOBInputCheckbox")]
public class BOBInputCheckboxVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>();

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BOBInputCheckboxVariant customVariant = BOBInputCheckboxVariant.Custom("Toggle");

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<BOBInputCheckbox<bool>>()
                   .AddVariant(
                       customVariant,
                       cb => builder =>
                       {
                           builder.OpenElement(0, "bob-component");
                           builder.AddAttribute(1, "class", "toggle-checkbox");
                           builder.AddContent(2, cb.Label);
                           builder.CloseElement();
                       }));

        // Act
        IRenderedComponent<BOBInputCheckbox<bool>> cut = ctx.Render<BOBInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Custom Toggle")
            .Add(c => c.Variant, customVariant));

        // Assert
        cut.Find(".toggle-checkbox").Should().NotBeNull();
        cut.Markup.Should().Contain("Custom Toggle");
    }
}
