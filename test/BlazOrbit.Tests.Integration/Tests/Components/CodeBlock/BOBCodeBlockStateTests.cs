using BlazOrbit.Components;
using BlazOrbit.SyntaxHighlight;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.CodeBlock;

[Trait("Component State", "BOBCodeBlock")]
public class BOBCodeBlockStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Content_When_Code_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "Hello"));

        cut.Find(".bob-code-block__content").InnerHtml.Should().Contain("Hello");

        // Act
        cut.Render(p => p.Add(c => c.Code, "World"));

        // Assert
        cut.Find(".bob-code-block__content").InnerHtml.Should().Contain("World");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Title_When_Language_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Language, SyntaxHighlightLanguage.CSharp));

        cut.Find(".bob-code-block__title").TextContent.Should().Be("CSHARP");

        // Act
        cut.Render(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Language, SyntaxHighlightLanguage.Json));

        // Assert
        cut.Find(".bob-code-block__title").TextContent.Should().Be("JSON");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Size_Attribute_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Size, BOBSize.Small));

        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("small");

        // Act
        cut.Render(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Size, BOBSize.Large));

        // Assert
        cut.Find("bob-component").GetAttribute("data-bob-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Custom_Title_Override_Language_Name(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Language, SyntaxHighlightLanguage.TypeScript));

        cut.Find(".bob-code-block__title").TextContent.Should().Be("TYPESCRIPT");

        // Act
        cut.Render(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Language, SyntaxHighlightLanguage.TypeScript)
            .Add(c => c.Title, "My Script"));

        // Assert
        cut.Find(".bob-code-block__title").TextContent.Should().Be("My Script");
    }
}
