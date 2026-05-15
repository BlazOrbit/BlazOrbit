using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazOrbit.Tests.Integration.Tests.Components.FileUpload;

[Trait("Component Interaction", "BOBInputFile")]
public class BOBInputFileInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Add_File_To_List_On_Selection(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<IBrowserFile>? captured = null;
        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(p => p
            .Add(c => c.GeneratePreviews, false)
            .Add(c => c.OnFilesSelected, files => captured = files));

        InputFileContent file = InputFileContent.CreateFromText("hello", "hello.txt");
        cut.FindComponent<InputFile>().UploadFiles(file);

        captured.Should().NotBeNull();
        captured!.Should().HaveCount(1);
        captured[0].Name.Should().Be("hello.txt");
        cut.FindAll(".bob-file-upload__entry").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reject_File_Beyond_MaxSize(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<BOBFileValidationError>? errors = null;
        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(p => p
            .Add(c => c.MaxSize, 4)
            .Add(c => c.GeneratePreviews, false)
            .Add(c => c.OnInvalid, e => errors = e));

        InputFileContent oversized = InputFileContent.CreateFromText("hello world", "big.txt");
        cut.FindComponent<InputFile>().UploadFiles(oversized);

        errors.Should().NotBeNull();
        errors!.Should().ContainSingle().Which.Kind.Should().Be(BOBFileValidationErrorKind.TooLarge);
        cut.FindAll(".bob-file-upload__entry").Should().BeEmpty();
        cut.Find(".bob-file-upload__errors").TextContent.Should().Contain("big.txt");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reject_File_Of_Disallowed_Type(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IReadOnlyList<BOBFileValidationError>? errors = null;
        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(p => p
            .Add(c => c.Accept, "image/*")
            .Add(c => c.GeneratePreviews, false)
            .Add(c => c.OnInvalid, e => errors = e));

        InputFileContent textFile = InputFileContent.CreateFromText("hello", "doc.txt");
        cut.FindComponent<InputFile>().UploadFiles(textFile);

        errors.Should().NotBeNull();
        errors!.Should().ContainSingle().Which.Kind.Should().Be(BOBFileValidationErrorKind.DisallowedType);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Remove_File_On_Remove_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IBrowserFile? removed = null;
        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(p => p
            .Add(c => c.GeneratePreviews, false)
            .Add(c => c.OnRemoved, f => removed = f));

        InputFileContent file = InputFileContent.CreateFromText("hi", "x.txt");
        cut.FindComponent<InputFile>().UploadFiles(file);

        cut.FindAll(".bob-file-upload__entry").Should().HaveCount(1);

        cut.Find(".bob-file-upload__entry ._bob-btn[data-bob-variant='ghost']").Click();

        cut.FindAll(".bob-file-upload__entry").Should().BeEmpty();
        removed.Should().NotBeNull();
        removed!.Name.Should().Be("x.txt");
    }
}
