using AngleSharp.Dom;
using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component State", "BOBInputOutline")]
public class BOBInputOutlineStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Label_On_Reparameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputOutline> cut = ctx.Render<BOBInputOutline>(p => p
            .Add(c => c.Label, "Name")
            .Add(c => c.For, "input-1"));

        cut.Find("label.bob-input__label").TextContent.Should().Be("Name");

        // Act
        cut.Render(p => p
            .Add(c => c.Label, null)
            .Add(c => c.For, "input-1"));

        // Assert
        cut.FindAll("label.bob-input__label").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_For_Attribute_On_Reparameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBInputOutline> cut = ctx.Render<BOBInputOutline>(p => p
            .Add(c => c.Label, "Email")
            .Add(c => c.For, "email-1"));

        cut.Find("label.bob-input__label").GetAttribute("for").Should().Be("email-1");

        // Act
        cut.Render(p => p
            .Add(c => c.Label, "Email")
            .Add(c => c.For, "email-2"));

        // Assert
        cut.Find("label.bob-input__label").GetAttribute("for").Should().Be("email-2");
    }
}
