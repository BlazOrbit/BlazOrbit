using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Svg;

[Trait("Component Security", "BOBSvgIcon")]
public class BOBSvgIconSecurityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Strip_Script_Tags_From_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, new IconKey("xss1") { SvgContent = "<path d=\"M1 1h22\"/><script>alert(1)</script>" }));

        // Assert — script stripped by SvgMarkupSanitizer
        cut.Find("svg").InnerHtml.Should().NotContain("<script>");
        cut.Find("svg").InnerHtml.Should().NotContain("alert(1)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Strip_Event_Handler_Attributes_From_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, new IconKey("xss2") { SvgContent = "<path d=\"M1 1\" onclick=\"evil()\"/>" }));

        // Assert — event handler stripped
        cut.Find("svg").InnerHtml.Should().NotContain("onclick");
        cut.Find("svg").InnerHtml.Should().NotContain("evil()");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Strip_ForeignObject_From_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, new IconKey("xss3") { SvgContent = "<path d=\"M1 1\"/><foreignObject><div>XSS</div></foreignObject>" }));

        // Assert — foreignObject stripped
        cut.Find("svg").InnerHtml.Should().NotContain("foreignObject");
        cut.Find("svg").InnerHtml.Should().NotContain("XSS");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Strip_Javascript_Uri_From_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBSvgIcon> cut = ctx.Render<BOBSvgIcon>(p => p
            .Add(c => c.Icon, new IconKey("xss4") { SvgContent = "<a href=\"javascript:alert(1)\"><path d=\"M1 1\"/></a>" }));

        // Assert — javascript: URI stripped
        cut.Find("svg").InnerHtml.Should().NotContain("javascript:");
    }
}
