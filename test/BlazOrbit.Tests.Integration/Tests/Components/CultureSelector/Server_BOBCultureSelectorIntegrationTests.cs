using AngleSharp.Dom;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Infrastructure.Fakes;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServerSelector = BlazOrbit.Components.Server.BOBCultureSelector;
using SharedVariant = BlazOrbit.Localization.Shared.BOBCultureSelectorVariant;

namespace BlazOrbit.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component Integration", "BOBCultureSelector")]
public class Server_BOBCultureSelectorIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.OnlyServer), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_To_Culture_Endpoint_With_ForceLoad_When_Culture_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        FakeNavigationManager navigation = ctx.Services.GetRequiredService<FakeNavigationManager>();

        // Arrange
        IRenderedComponent<ServerSelector> cut = ctx.Render<ServerSelector>(p => p
            .Add(c => c.Variant, SharedVariant.Dropdown));

        cut.Find("button.bob-dropdown__trigger").Click();
        IReadOnlyList<IElement> options = cut.FindAll(".bob-dropdown__option");

        IElement? target = options.FirstOrDefault(o =>
            o.GetAttribute("aria-selected") != "true");
        target.Should().NotBeNull("there must be at least one non-active culture option");

        // Act
        target!.Click();

        // Assert
        navigation.LastNavigationUri.Should().Contain("culture=");
        navigation.LastForceLoad.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.OnlyServer), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_Current_Path_As_RedirectUri(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        FakeNavigationManager navigation = ctx.Services.GetRequiredService<FakeNavigationManager>();

        // Arrange — navigate to a specific page first
        navigation.NavigateTo("/docs/getting-started");

        IRenderedComponent<ServerSelector> cut = ctx.Render<ServerSelector>(p => p
            .Add(c => c.Variant, SharedVariant.Dropdown));

        cut.Find("button.bob-dropdown__trigger").Click();
        IReadOnlyList<IElement> options = cut.FindAll(".bob-dropdown__option");

        IElement? target = options.FirstOrDefault(o =>
            o.GetAttribute("aria-selected") != "true");
        target.Should().NotBeNull();

        // Act
        target!.Click();

        // Assert
        navigation.LastNavigationUri.Should().Contain("redirectUri=");
        navigation.LastNavigationUri.Should().Contain(Uri.EscapeDataString("/docs/getting-started"));
    }
}
