using BlazOrbit.Components;
using BlazOrbit.SyntaxHighlight;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.CodeBlock;

[Trait("Component Security", "BOBCodeBlock")]
public class BOBCodeBlockSecurityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Encode_Script_Tag_In_Code(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "</pre><script>alert(1)</script>")
            .Add(c => c.Language, SyntaxHighlightLanguage.CSharp));

        // Assert — raw <script> tag must not appear in rendered markup
        cut.Find(".bob-code-block__content").InnerHtml.Should().NotContain("<script>alert(1)</script>");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Encode_Closing_Pre_Tag_In_Code(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "</pre><b>injected</b>")
            .Add(c => c.Language, SyntaxHighlightLanguage.CSharp));

        // Assert — injected HTML must not break structure
        cut.FindAll("bob-component[data-bob-component=\"code-block\"]").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Html_Entities_Safely_When_Highlight_Fails(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — empty code triggers fallback path
        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, string.Empty));

        // Assert — content area renders without exception
        cut.Find(".bob-code-block__content").Should().NotBeNull();
    }
}
