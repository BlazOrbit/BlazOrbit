using BlazOrbit.Components;
using BlazOrbit.SyntaxHighlight;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.CodeBlock;

[Trait("Component Accessibility", "BOBCodeBlock")]
public class BOBCodeBlockAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Label_On_Copy_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert
        cut.Find("button").GetAttribute("aria-label").Should().Be("Copy code");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Copy_Button_Have_Type_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert — type=button prevents accidental form submission
        cut.Find("button").GetAttribute("type").Should().Be("button");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Additional_Attributes_To_Root(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;")
            .AddUnmatched("aria-label", "Code snippet"));

        // Assert
        cut.Find("bob-component").GetAttribute("aria-label").Should().Be("Code snippet");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Title_Span_Reflect_Language_For_Screen_Readers(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "{}")
            .Add(c => c.Language, SyntaxHighlightLanguage.Json));

        // Assert — visible title helps screen reader context
        cut.Find(".bob-code-block__title").TextContent.Should().Be("JSON");
    }
}
