using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazOrbit.Tests.Integration.Tests.Components.FileUpload;

[Trait("Component Accessibility", "BOBInputFile")]
public class BOBInputFileAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Provide_Tabindex_On_Drop_Zone(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>();

        cut.Find(".bob-file-upload__zone").GetAttribute("tabindex").Should().Be("0");

        cut.Render(p => p.Add(c => c.Disabled, true));
        cut.Find(".bob-file-upload__zone").GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Remove_Button_Aria_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(p => p
            .Add(c => c.GeneratePreviews, false)
            .Add(c => c.RemoveText, "Quitar archivo"));

        InputFileContent file = InputFileContent.CreateFromText("x", "x.txt");
        cut.FindComponent<InputFile>().UploadFiles(file);

        cut.Find(".bob-file-upload__remove").GetAttribute("aria-label").Should().Be("Quitar archivo");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Errors_As_Alert_Region(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(p => p
            .Add(c => c.MaxSize, 1)
            .Add(c => c.GeneratePreviews, false));

        InputFileContent file = InputFileContent.CreateFromText("hello", "hello.txt");
        cut.FindComponent<InputFile>().UploadFiles(file);

        cut.Find(".bob-file-upload__errors").GetAttribute("role").Should().Be("alert");
    }
}
