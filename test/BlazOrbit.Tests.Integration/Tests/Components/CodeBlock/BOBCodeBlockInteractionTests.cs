using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.CodeBlock;

[Trait("Component Interaction", "BOBCodeBlock")]
public class BOBCodeBlockInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Call_CopyTextAsync_When_Copy_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IClipboardJsInterop clipboard = Substitute.For<IClipboardJsInterop>();
        clipboard.CopyTextAsync(Arg.Any<string>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddSingleton(clipboard);

        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Act
        cut.Find("button").Click();

        // Assert
        await clipboard.Received(1).CopyTextAsync(Arg.Is<string>(s => s.Contains("var x = 1")));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Normalized_Code_To_Clipboard(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IClipboardJsInterop clipboard = Substitute.For<IClipboardJsInterop>();
        clipboard.CopyTextAsync(Arg.Any<string>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddSingleton(clipboard);

        const string indentedCode = "    var x = 1;\n    var y = 2;";

        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, indentedCode));

        // Act
        cut.Find("button").Click();

        // Assert — NormalizeIndentation strips leading 4 spaces
        await clipboard.Received(1).CopyTextAsync(Arg.Is<string>(s => s.StartsWith("var x")));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Copy_Button_After_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IClipboardJsInterop clipboard = Substitute.For<IClipboardJsInterop>();
        clipboard.CopyTextAsync(Arg.Any<string>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddSingleton(clipboard);

        IRenderedComponent<BOBCodeBlock> cut = ctx.Render<BOBCodeBlock>(p => p
            .Add(c => c.Code, "x"));

        cut.Find("button").HasAttribute("disabled").Should().BeFalse();

        // Act
        cut.Find("button").Click();

        // Assert — _copied=true disables button
        cut.Find("button").HasAttribute("disabled").Should().BeTrue();
    }
}
