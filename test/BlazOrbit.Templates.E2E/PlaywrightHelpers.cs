using Microsoft.Playwright;

namespace BlazOrbit.Templates.E2E;

public static class PlaywrightHelpers
{
    /// <summary>
    /// Waits until the Blazor app has booted to the point where at least one
    /// <c>&lt;bob-component&gt;</c> element is visible in the DOM.
    /// <para>
    /// This is a stronger signal than waiting for the static <c>.bob-app__loader</c>
    /// div to disappear: <c>BOBInitializer</c> only renders its <c>ChildContent</c>
    /// (which contains the BOB components) once <c>_palette</c> is loaded via JS interop,
    /// which only happens after the interactive render mode has activated.
    /// </para>
    /// </summary>
    public static async Task WaitForAppReadyAsync(IPage page, int timeoutMs = 60_000)
    {
        await page.WaitForSelectorAsync("bob-component", new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
    }

    /// <summary>
    /// Waits for the actual page <c>&lt;h1&gt;</c> + body content to render.
    /// Stronger than <see cref="WaitForAppReadyAsync"/>: catches the case
    /// where Blazor mounts the layout but the routed page itself fails
    /// (e.g. unhandled DI exceptions in a page component, missing service
    /// registrations) — which previously slipped through because the
    /// layout's <c>&lt;bob-component&gt;</c> wrapper renders even when the
    /// inner route component throws.
    /// </summary>
    public static async Task WaitForRoutedPageAsync(IPage page, int timeoutMs = 30_000)
    {
        await page.WaitForSelectorAsync("h1", new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
    }

    /// <summary>
    /// Asserts the page rendered a chart-family root: every BOBCharts
    /// component emits <c>data-bob-data-visualization-base</c> so a single
    /// selector catches the dashboard regardless of which chart types it
    /// includes. Surface DI / Razor errors that only fire when the chart
    /// service-resolver path executes.
    /// </summary>
    public static async Task WaitForChartRenderedAsync(IPage page, int timeoutMs = 30_000)
    {
        await page.WaitForSelectorAsync("[data-bob-data-visualization-base] svg", new()
        {
            State = WaitForSelectorState.Attached,
            Timeout = timeoutMs
        });
    }

    /// <summary>
    /// On a Playwright failure, dump page state (HTML, screenshot, console errors,
    /// 404 responses) and the running dotnet process output to a known directory so
    /// the next CI / local run can inspect what actually went wrong.
    /// </summary>
    public static async Task DumpFailureContextAsync(
        IPage page,
        RunningApp app,
        IReadOnlyList<string> consoleErrors,
        IReadOnlyList<string> network404s)
    {
        var dir = Path.Combine(Path.GetTempPath(), "blazorbit-e2e-logs");
        Directory.CreateDirectory(dir);

        var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var prefix = $"{app.Key}-{ts}";

        try
        {
            await page.ScreenshotAsync(new() { Path = Path.Combine(dir, $"{prefix}.png"), FullPage = true });
        }
        catch
        {
            // Best-effort
        }

        try
        {
            var html = await page.ContentAsync();
            await File.WriteAllTextAsync(Path.Combine(dir, $"{prefix}.html"), html);
        }
        catch
        {
            // Best-effort
        }

        var summary = new List<string>
        {
            $"# E2E failure: {app.Key}",
            $"URL: {app.Url}",
            "",
            "## Console errors",
        };
        summary.AddRange(consoleErrors.DefaultIfEmpty("(none)"));
        summary.Add("");
        summary.Add("## Network 404s");
        summary.AddRange(network404s.DefaultIfEmpty("(none)"));
        summary.Add("");
        summary.Add("## dotnet stdout");
        summary.AddRange(app.StdOut.DefaultIfEmpty("(empty)"));
        summary.Add("");
        summary.Add("## dotnet stderr");
        summary.AddRange(app.StdErr.DefaultIfEmpty("(empty)"));

        await File.WriteAllLinesAsync(Path.Combine(dir, $"{prefix}.log"), summary);

        Console.WriteLine($"[E2E] Failure context written to: {dir} (prefix: {prefix})");
    }

    public static async Task<IReadOnlyList<string>> GetConsoleErrorsAsync(IPage page)
    {
        var errors = new List<string>();
        page.Console += (_, e) =>
        {
            if (e.Type == "error")
            {
                errors.Add(e.Text);
            }
        };

        // Give the page a moment to settle and emit any early errors
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        return errors;
    }

    public static async Task<IReadOnlyList<string>> GetNetwork404sAsync(IPage page)
    {
        var notFounds = new List<string>();
        page.Response += (_, e) =>
        {
            if (e.Status == 404)
            {
                notFounds.Add($"{e.Request.Method} {e.Url}");
            }
        };

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        return notFounds;
    }
}
