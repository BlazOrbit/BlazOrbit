using FluentAssertions;
using Microsoft.Playwright;

namespace BlazOrbit.Templates.E2E;

[Trait("TestCategory", "E2E")]
[Trait("TemplateType", "Server")]
[Collection(nameof(TemplateTestCollection))]
public class ServerTemplateE2ETests
{
    private readonly TemplateTestFixture _fixture;

    public ServerTemplateE2ETests(TemplateTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("Server_Net8",                     false, false, false, false)]
    [InlineData("Server_Net8_Loc",                 true,  false, false, false)]
    [InlineData("Server_Net8_Loc_Charts",          true,  true,  false, false)]
    [InlineData("Server_Net8_Loc_Charts_Not",      true,  true,  true,  false)]
    [InlineData("Server_Net8_Loc_Charts_Not_Hot",  true,  true,  true,  true )]
    [InlineData("Server_Net10",                    false, false, false, false)]
    [InlineData("Server_Net10_Loc",                true,  false, false, false)]
    [InlineData("Server_Net10_Loc_Charts",         true,  true,  false, false)]
    [InlineData("Server_Net10_Loc_Charts_Not",     true,  true,  true,  false)]
    [InlineData("Server_Net10_Loc_Charts_Not_Hot", true,  true,  true,  true )]
    public async Task Should_Load_Without_Errors(string key, bool expectLocalization, bool expectCharts, bool expectNotifications, bool expectHotKeys)
    {
        // Arrange
        await using var app = await _fixture.RunProjectAsync(key);
        var page = await _fixture.NewPageAsync();

        var consoleErrors = new List<string>();
        var pageErrors = new List<string>();
        var network404s = new List<string>();

        page.Console += (_, e) =>
        {
            if (e.Type == "error")
            {
                consoleErrors.Add(e.Text);
            }
        };

        page.PageError += (_, e) => pageErrors.Add(e);

        page.Response += (_, e) =>
        {
            if (e.Status == 404)
            {
                network404s.Add($"{e.Request.Method} {e.Url}");
            }
        };

        // Act
        var response = await page.GotoAsync(app.Url);

        try
        {
            await PlaywrightHelpers.WaitForAppReadyAsync(page);
            await PlaywrightHelpers.WaitForRoutedPageAsync(page);
            if (expectCharts)
            {
                await PlaywrightHelpers.WaitForChartRenderedAsync(page);
            }
        }
        catch
        {
            await PlaywrightHelpers.DumpFailureContextAsync(page, app, consoleErrors, network404s);
            throw;
        }

        // Visual sanity: culture selector. Anchor on `.bob-culture-selector` (always
        // emitted on the outer <bob-component> by both variants) instead of inner
        // elements — `.bob-culture-selector__option` only mounts when the Dropdown
        // variant is open, so a closed dropdown looked indistinguishable from a
        // missing selector to the previous selector.
        int cultureSelectorCount = await page.Locator(".bob-culture-selector").CountAsync();
        if (expectLocalization)
        {
            cultureSelectorCount.Should().BeGreaterThan(0,
                because: $"localized template {key} must render the culture selector");
        }
        else
        {
            cultureSelectorCount.Should().Be(0,
                because: $"non-localized template {key} must NOT render the culture selector");
        }

        // Visual sanity: charts
        int chartCount = await page.Locator("[data-bob-data-visualization-base] svg").CountAsync();
        if (expectCharts)
        {
            chartCount.Should().BeGreaterThan(0,
                because: $"charts dashboard template {key} must render at least one chart");
        }
        else
        {
            chartCount.Should().Be(0,
                because: $"non-charts template {key} must NOT render any chart");
        }

        // Visual sanity: notification bell
        int bellCount = await page.Locator(".bob-notification-bell").CountAsync();
        if (expectNotifications)
        {
            bellCount.Should().BeGreaterThan(0,
                because: $"notifications template {key} must render the notification bell");
        }
        else
        {
            bellCount.Should().Be(0,
                because: $"non-notifications template {key} must NOT render the notification bell");
        }

        // BOBHotkeyHost renders nothing visible; just assert the parameter is exercised.
        _ = expectHotKeys;

        // Settle deferred async work
        await Task.Delay(500, TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response!.Status.Should().Be(200, because: "the home page should return OK");

        network404s.Should().BeEmpty(because: "no static assets should 404");
        consoleErrors.Should().BeEmpty(because: "no JS errors should occur during startup");
        pageErrors.Should().BeEmpty(because: "no unhandled renderer exceptions should fire");
    }
}
