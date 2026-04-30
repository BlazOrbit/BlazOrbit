using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Svg;

[Trait("Component Interaction", "BOBSvgIcon")]
public class BOBSvgIconInteractionTests
{
    private static readonly IconKey SimpleIcon = new("SimpleIcon") { SvgContent = "<path d=\"M12 2L2 22h20L12 2z\"/>" };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Invoke_OnClick_Callback(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int clickCount = 0;
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, SimpleIcon)
            .Add(c => c.OnClick, _ => clickCount++));

        // Act
        cut.Find("bob-component").Click();

        // Assert
        clickCount.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_MouseEventArgs_To_Handler(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        MouseEventArgs? captured = null;
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, SimpleIcon)
            .Add(c => c.OnClick, e => captured = e));

        // Act
        cut.Find("bob-component").Click(new MouseEventArgs { ClientX = 42, ClientY = 13 });

        // Assert
        captured.Should().NotBeNull();
        captured!.ClientX.Should().Be(42);
        captured.ClientY.Should().Be(13);
    }
}
