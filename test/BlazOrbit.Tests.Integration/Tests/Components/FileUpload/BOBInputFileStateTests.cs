using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.FileUpload;

[Trait("Component State", "BOBInputFile")]
public class BOBInputFileStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Disabled_On_Zone_And_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>();

        cut.Find(".bob-file-upload__zone").GetAttribute("data-bob-disabled").Should().BeNull();
        cut.Find("input[type='file']").HasAttribute("disabled").Should().BeFalse();

        cut.Render(p => p.Add(c => c.Disabled, true));

        cut.Find(".bob-file-upload__zone").GetAttribute("data-bob-disabled").Should().Be("true");
        cut.Find("input[type='file']").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_DropZone_Texts(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(p => p
            .Add(c => c.DropZoneText, "Drop docs")
            .Add(c => c.DropZoneHint, "PDF only"));

        cut.Find(".bob-file-upload__title").TextContent.Should().Be("Drop docs");
        cut.Find(".bob-file-upload__hint").TextContent.Should().Be("PDF only");
    }

    [Fact]
    public void BOBFileValidationError_Equality_Compares_All_Fields()
    {
        BOBFileValidationError a = new("a.png", 100, BOBFileValidationErrorKind.TooLarge);
        BOBFileValidationError b = new("a.png", 100, BOBFileValidationErrorKind.TooLarge);
        BOBFileValidationError c = new("a.png", 200, BOBFileValidationErrorKind.TooLarge);

        a.Should().Be(b);
        a.Should().NotBe(c);
    }
}