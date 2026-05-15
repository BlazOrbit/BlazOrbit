using BlazOrbit.Components.Forms;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.FileUpload;

[Trait("Component Rendering", "BOBInputFile")]
public class BOBInputFileRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>();

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("input-file");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Drop_Zone_With_Native_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>();

        cut.Find(".bob-file-upload__zone").Should().NotBeNull();
        cut.Find("input[type='file']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Drop_Zone_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>();

        cut.Find(".bob-file-upload__title").TextContent.Should().Contain("Drag");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Drop_Zone_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(p => p
            .AddChildContent("<span class='custom-zone'>Drop here!</span>"));

        cut.Find(".custom-zone").TextContent.Should().Be("Drop here!");
        cut.FindAll(".bob-file-upload__title").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Multiple_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(p => p
            .Add(c => c.Multiple, true));

        cut.Find("input[type='file']").HasAttribute("multiple").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Accept_Filter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(p => p
            .Add(c => c.Accept, "image/*,.pdf"));

        cut.Find("input[type='file']").GetAttribute("accept").Should().Be("image/*,.pdf");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBInputFile> cut = ctx.Render<BOBInputFile>(p => p
            .Add(c => c.Label, "Attachments")
            .Add(c => c.HelperText, "Max 10 MB."));

        cut.Find(".bob-file-upload__label").TextContent.Should().Contain("Attachments");
        cut.Find(".bob-file-upload__helper").TextContent.Should().Contain("Max 10 MB.");
    }
}
