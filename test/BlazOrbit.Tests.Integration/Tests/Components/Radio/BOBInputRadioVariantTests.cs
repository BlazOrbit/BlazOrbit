using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components.Consumers;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Radio;

[Trait("Component Variants", "BOBInputRadio")]
public class BOBInputRadioVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBOBInputRadioConsumer> cut = ctx.Render<TestBOBInputRadioConsumer>();

        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBInputRadioVariant custom = BOBInputRadioVariant.Custom("segmented");
        ctx.Services.AddBlazOrbitVariants(b => b
            .ForComponent<BOBInputRadio<string?>>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "fieldset");
                builder.AddAttribute(1, "class", "custom-radio-segmented");
                builder.CloseElement();
            }));

        // Act — set Variant on consumer; consumer doesn't pass it directly, so render via direct component
        IRenderedComponent<BOBInputRadio<string?>> cut = ctx.Render<BOBInputRadio<string?>>(p => p
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find("fieldset.custom-radio-segmented").Should().NotBeNull();
    }
}
