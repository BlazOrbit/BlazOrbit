using AngleSharp.Dom;
using BlazOrbit.Localization.Wasm;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Infrastructure.Fakes;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Globalization;
using WasmSelector = BlazOrbit.Components.Wasm.BOBCultureSelector;
using SharedVariant = BlazOrbit.Localization.Shared.BOBCultureSelectorVariant;

namespace BlazOrbit.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component Integration", "BOBCultureSelector")]
public class Wasm_BOBCultureSelectorIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.OnlyWasm), MemberType = typeof(TestScenarios))]
    public async Task Should_ForceReload_When_ForceReload_Is_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — replace the real persistence with a fake so SetCultureAsync succeeds
        ILocalizationPersistence fakePersistence = Substitute.For<ILocalizationPersistence>();
        fakePersistence.SetStoredCultureAsync(Arg.Any<string>()).Returns(Task.CompletedTask);
        ctx.Services.AddSingleton(fakePersistence);

        FakeNavigationManager navigation = ctx.Services.GetRequiredService<FakeNavigationManager>();

        IRenderedComponent<WasmSelector> cut = ctx.Render<WasmSelector>(p => p
            .Add(c => c.Variant, SharedVariant.Dropdown)
            .Add(c => c.ForceReload, true));

        cut.Find("button.bob-dropdown__trigger").Click();
        IReadOnlyList<IElement> options = cut.FindAll(".bob-dropdown__option");

        IElement? target = options.FirstOrDefault(o =>
            o.GetAttribute("aria-selected") != "true");
        target.Should().NotBeNull();

        // Act
        target!.Click();

        // Assert
        navigation.LastForceLoad.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.OnlyWasm), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_CurrentCulture_InPlace_When_ForceReload_Is_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — replace the real persistence with a fake so SetCultureAsync succeeds
        ILocalizationPersistence fakePersistence = Substitute.For<ILocalizationPersistence>();
        fakePersistence.SetStoredCultureAsync(Arg.Any<string>()).Returns(Task.CompletedTask);
        ctx.Services.AddSingleton(fakePersistence);

        FakeNavigationManager navigation = ctx.Services.GetRequiredService<FakeNavigationManager>();
        CultureInfo? capturedCulture = null;

        IRenderedComponent<WasmSelector> cut = ctx.Render<WasmSelector>(p => p
            .Add(c => c.Variant, SharedVariant.Dropdown)
            .Add(c => c.ForceReload, false)
            .Add(c => c.OnCultureChanged, (CultureInfo ci) => capturedCulture = ci));

        cut.Find("button.bob-dropdown__trigger").Click();
        IReadOnlyList<IElement> options = cut.FindAll(".bob-dropdown__option");

        IElement? target = options.FirstOrDefault(o =>
            o.GetAttribute("aria-selected") != "true");
        target.Should().NotBeNull();

        // Act
        target!.Click();

        // Assert
        navigation.LastForceLoad.Should().BeNull("no navigation should occur when ForceReload is false");
        capturedCulture.Should().NotBeNull();
        await fakePersistence.Received(1).SetStoredCultureAsync(capturedCulture!.Name);
    }
}
