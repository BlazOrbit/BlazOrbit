using BlazOrbit.Components;
using BlazOrbit.SyntaxHighlight;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.CodeBlock;

[Trait("Component Rendering", "BOBCodeBlock")]
public class BOBCodeBlockRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("code-block");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Header(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert
        cut.Find(".bob-code-block__header").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Content_Area(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert
        cut.Find(".bob-code-block__content").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Default_Language_As_Title(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;")
            .Add(c => c.Language, SyntaxHighlightLanguage.CSharp));

        // Assert — default title = Language.ToString().ToUpperInvariant()
        cut.Find(".bob-code-block__title").TextContent.Should().Be("CSHARP");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Custom_Title_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;")
            .Add(c => c.Title, "My Snippet"));

        // Assert
        cut.Find(".bob-code-block__title").TextContent.Should().Be("My Snippet");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Copy_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert — _BOBBtn renders a <button>
        cut.Find("button").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;")
            .Add(c => c.Size, BOBSize.Large));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Code_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "Hello World"));

        // Assert — highlighted content contains the code text
        cut.Find(".bob-code-block__content").InnerHtml.Should().Contain("Hello World");
    }
}
