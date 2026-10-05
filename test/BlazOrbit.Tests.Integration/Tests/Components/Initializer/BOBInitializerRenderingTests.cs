using BlazOrbit.Components;
using BlazOrbit.Themes;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.Initializer;

[Trait("Component Rendering", "BOBInitializer")]
public class BOBInitializerRenderingTests
{
    internal static readonly Dictionary<string, string> FullPalette = new()
    {
        ["--palette-background"] = "#121212",
        ["--palette-background-contrast"] = "#FFFFFF",
        ["--palette-error"] = "#CF6679",
        ["--palette-error-contrast"] = "#000000",
        ["--palette-info"] = "#64B5F6",
        ["--palette-info-contrast"] = "#000000",
        ["--palette-primary"] = "#8AB4F8",
        ["--palette-primary-contrast"] = "#000000",
        ["--palette-secondary"] = "#B39DDB",
        ["--palette-secondary-contrast"] = "#000000",
        ["--palette-shadow"] = "#000000",
        ["--palette-success"] = "#81C995",
        ["--palette-success-contrast"] = "#000000",
        ["--palette-surface"] = "#1E1E1E",
        ["--palette-surface-contrast"] = "#FFFFFF",
        ["--palette-warning"] = "#FFD54F",
        ["--palette-warning-contrast"] = "#000000"
    };

    internal static IThemeJsInterop RegisterFakeTheme(BlazorTestContextBase ctx)
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(new ValueTask<Dictionary<string, string>>(FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddScoped(_ => fake);
        return fake;
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ChildContent_After_First_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>(p => p
            .AddChildContent("<div class='test-child'>Hello</div>"));

        // Assert
        cut.FindAll(".test-child").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_For_Null_ChildContent(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>();

        // Assert - no child divs (only HeadContent + CascadingValue shell)
        cut.FindAll("div").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Call_InitializeAsync_On_First_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>(p => p
            .Add(c => c.DefaultTheme, "light"));

        // Assert
        await fake.Received(1).InitializeAsync("light");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Call_GetPaletteAsync_On_First_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>();

        // Assert
        await fake.Received(1).GetPaletteAsync();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ChildContent_When_Palette_Has_Not_Resolved(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterPendingTheme(ctx);

        // Arrange & Act - palette never resolves, as under static SSR / prerender
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>(p => p
            .AddChildContent("<div class='test-child'>Hello</div>"));

        // Assert
        cut.FindAll(".test-child").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Cascade_Null_Palette_Until_Resolved(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TaskCompletionSource<Dictionary<string, string>> pending = RegisterPendingTheme(ctx);

        // Arrange
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>(p => p
            .AddChildContent<PaletteProbe>());

        IRenderedComponent<PaletteProbe> probe = cut.FindComponent<PaletteProbe>();
        probe.Instance.Palette.Should().BeNull();

        // Act
        pending.SetResult(FullPalette);

        // Assert
        probe.WaitForState(() => probe.Instance.Palette is not null, TimeSpan.FromSeconds(1));
        probe.Instance.Palette!.Primary.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ChildContent_When_Palette_Is_Incomplete(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        // ThemeJsInterop returns an empty dictionary when the JS module fails to load.
        fake.GetPaletteAsync().Returns(new ValueTask<Dictionary<string, string>>([]));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddScoped(_ => fake);

        // Arrange & Act
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>(p => p
            .AddChildContent<PaletteProbe>());

        // Assert
        cut.FindComponent<PaletteProbe>().Instance.Palette.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_DefaultTheme_To_AntiFlash_Script(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>(p => p
            .Add(c => c.DefaultTheme, "light"));

        // Assert
        AntiFlashScripts(ctx, cut).Should().ContainSingle()
            .Which.GetAttribute("data-default-theme").Should().Be("light");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AntiFlash_Script_When_Page_Declares_HeadContent(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act - HeadOutlet only renders the last <HeadContent>; a page's own
        // <HeadContent> (title, meta description...) must not drop the theme bootstrap.
        IRenderedComponent<BOBInitializer> cut = ctx.Render<BOBInitializer>(p => p
            .AddChildContent<PageWithHeadContent>());

        // Assert
        AntiFlashScripts(ctx, cut).Should().ContainSingle();
    }

    private static IReadOnlyList<AngleSharp.Dom.IElement> AntiFlashScripts(
        BlazorTestContextBase ctx, IRenderedComponent<BOBInitializer> cut)
    {
        const string selector = "script[src='_content/BlazOrbit/anti-flash.js']";
        IRenderedComponent<HeadOutlet> head = ctx.Render<HeadOutlet>();
        return [.. head.FindAll(selector), .. cut.FindAll(selector)];
    }

    private sealed class PageWithHeadContent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<HeadContent>(0);
            builder.AddComponentParameter(1, nameof(HeadContent.ChildContent), (RenderFragment)(b =>
            {
                b.OpenElement(0, "meta");
                b.AddAttribute(1, "name", "description");
                b.AddAttribute(2, "content", "page");
                b.CloseElement();
            }));
            builder.CloseComponent();
        }
    }

    private static TaskCompletionSource<Dictionary<string, string>> RegisterPendingTheme(BlazorTestContextBase ctx)
    {
        TaskCompletionSource<Dictionary<string, string>> pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(_ => new ValueTask<Dictionary<string, string>>(pending.Task));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddScoped(_ => fake);
        return pending;
    }

    private sealed class PaletteProbe : ComponentBase
    {
        [CascadingParameter] public BOBPalette? Palette { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "palette-probe");
            builder.CloseElement();
        }
    }
}
