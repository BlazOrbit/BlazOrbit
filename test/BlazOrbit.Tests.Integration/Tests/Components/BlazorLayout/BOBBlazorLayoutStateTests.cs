using BlazOrbit.Components.Layout;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Tests.Components.Initializer;
using BlazOrbit.Themes;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Components.BlazorLayout;

[Trait("Component State", "BOBBlazorLayout")]
public class BOBBlazorLayoutStateTests
{
    private static readonly Dictionary<string, string> LightPalette = new()
    {
        ["--palette-background"] = "#FFFFFF",
        ["--palette-background-contrast"] = "#000000",
        ["--palette-error"] = "#B00020",
        ["--palette-error-contrast"] = "#FFFFFF",
        ["--palette-info"] = "#1976D2",
        ["--palette-info-contrast"] = "#FFFFFF",
        ["--palette-primary"] = "#6200EE",
        ["--palette-primary-contrast"] = "#FFFFFF",
        ["--palette-secondary"] = "#03DAC6",
        ["--palette-secondary-contrast"] = "#000000",
        ["--palette-shadow"] = "#000000",
        ["--palette-success"] = "#4CAF50",
        ["--palette-success-contrast"] = "#FFFFFF",
        ["--palette-surface"] = "#F5F5F5",
        ["--palette-surface-contrast"] = "#000000",
        ["--palette-warning"] = "#FB8C00",
        ["--palette-warning-contrast"] = "#000000",
    };

    private static IThemeJsInterop RegisterFakeTheme(
        BlazorTestContextBase ctx,
        Action<Action<string>>? captureHandler = null)
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(
            new ValueTask<Dictionary<string, string>>(BOBInitializerRenderingTests.FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        if (captureHandler is not null)
        {
            fake.OnThemeChanged += Arg.Do<Action<string>>(h => captureHandler(h));
        }

        ctx.Services.AddScoped(_ => fake);
        return fake;
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Initialize_With_Dark_DefaultTheme(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BOBBlazorLayout> cut = ctx.Render<BOBBlazorLayout>();

        // Assert — layout hard-codes DefaultTheme="dark"
        await fake.Received(1).InitializeAsync("dark");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reload_Palette_When_Theme_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        Action<string>? handler = null;
        IThemeJsInterop fake = RegisterFakeTheme(ctx, h => handler = h);

        // Arrange
        IRenderedComponent<BOBBlazorLayout> cut = ctx.Render<BOBBlazorLayout>();
        int before = fake.ReceivedCalls().Count(c => c.GetMethodInfo().Name == "GetPaletteAsync");

        fake.GetPaletteAsync().Returns(
            new ValueTask<Dictionary<string, string>>(LightPalette));

        // Act
        handler.Should().NotBeNull("BOBInitializer must subscribe to OnThemeChanged");
        handler!.Invoke("light");
        cut.WaitForState(() => true, TimeSpan.FromMilliseconds(300));

        // Assert — palette reload triggered
        int after = fake.ReceivedCalls().Count(c => c.GetMethodInfo().Name == "GetPaletteAsync");
        after.Should().BeGreaterThan(before);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_BOBPalette_As_Cascading_Value_To_Body(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        BOBPalette? capturedFromBody = null;

        // Arrange & Act
        IRenderedComponent<BOBBlazorLayout> cut = ctx.Render<BOBBlazorLayout>(p => p
            .Add(c => c.Body, (RenderFragment)(b =>
            {
                b.OpenComponent<PaletteCapturingConsumer>(0);
                b.AddComponentParameter(1, nameof(PaletteCapturingConsumer.OnPaletteResolved),
                    EventCallback.Factory.Create<BOBPalette?>(this, p => capturedFromBody = p));
                b.CloseComponent();
            })));

        cut.WaitForState(() => capturedFromBody is not null, TimeSpan.FromSeconds(1));

        // Assert — cascading palette reaches Body
        capturedFromBody.Should().NotBeNull();
        capturedFromBody!.Primary.ToString().Should().NotBeNullOrWhiteSpace();
    }

    private sealed class PaletteCapturingConsumer : ComponentBase
    {
        [CascadingParameter] public BOBPalette? Palette { get; set; }
        [Parameter] public EventCallback<BOBPalette?> OnPaletteResolved { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (Palette is not null)
            {
                await OnPaletteResolved.InvokeAsync(Palette);
            }
        }

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "palette-consumer");
            builder.AddContent(2, Palette?.Primary.ToString() ?? "");
            builder.CloseElement();
        }
    }
}
