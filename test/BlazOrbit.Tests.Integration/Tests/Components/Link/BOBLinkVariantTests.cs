using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Components.Link;

[Trait("Component Variants", "BOBLink")]
public class BOBLinkVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Button_Variant_As_Anchor(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "/signup")
            .Add(c => c.Text, "Sign up")
            .Add(c => c.Variant, BOBLinkVariant.Button));

        // Assert - looks like a button, keeps link semantics
        cut.Find("bob-component").GetAttribute("data-bob-variant").Should().Be("button");
        cut.Find("a").GetAttribute("href").Should().Be("/signup");
        cut.FindAll("button").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BOBLinkVariant customVariant = BOBLinkVariant.Custom("Pill");

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<BOBLink>()
                .AddVariant(
                    customVariant,
                    link => builder =>
                    {
                        builder.OpenElement(0, "a");
                        builder.AddAttribute(1, "class", "pill-link");
                        builder.AddAttribute(2, "href", link.Href);
                        builder.AddContent(3, link.Text);
                        builder.CloseElement();
                    }));

        // Act
        IRenderedComponent<BOBLink> cut = ctx.Render<BOBLink>(p => p
            .Add(c => c.Href, "/pill")
            .Add(c => c.Text, "Pill")
            .Add(c => c.Variant, customVariant));

        // Assert
        cut.Find(".pill-link").GetAttribute("href").Should().Be("/pill");
    }
}
