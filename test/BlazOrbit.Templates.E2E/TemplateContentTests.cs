using FluentAssertions;

namespace BlazOrbit.Templates.E2E;

/// <summary>
/// Structural sanity checks on the generated project files. These run
/// without spinning up the full Playwright pipeline so cross-wired
/// modifiers / rename rules in <c>template.json</c> fail fast — the
/// previous matrix only validated runtime errors, which silently passed
/// when (e.g.) the dashboard Home variant leaked into a no-charts
/// generation because the Razor page still parsed.
/// </summary>
[Trait("TestCategory", "TemplateContent")]
[Collection(nameof(TemplateTestCollection))]
public class TemplateContentTests
{
    private readonly TemplateTestFixture _fixture;

    public TemplateContentTests(TemplateTestFixture fixture)
    {
        _fixture = fixture;
    }

    public sealed record Scenario(string Key, bool Localization, bool Charts, bool Notifications, bool HotKeys, string Template, string Theme = "None");

    public static IEnumerable<object[]> AllScenarios() => new[]
    {
        new object[] { new Scenario("Server_Net8",                     false, false, false, false, "server") },
        new object[] { new Scenario("Server_Net8_Loc",                 true,  false, false, false, "server") },
        new object[] { new Scenario("Server_Net8_Loc_Charts",          true,  true,  false, false, "server") },
        new object[] { new Scenario("Server_Net8_Loc_Charts_Not",      true,  true,  true,  false, "server") },
        new object[] { new Scenario("Server_Net8_Loc_Charts_Not_Hot",  true,  true,  true,  true,  "server") },
        new object[] { new Scenario("Wasm_Net8",                       false, false, false, false, "wasm")   },
        new object[] { new Scenario("Wasm_Net8_Loc",                   true,  false, false, false, "wasm")   },
        new object[] { new Scenario("Wasm_Net8_Loc_Charts",            true,  true,  false, false, "wasm")   },
        new object[] { new Scenario("Wasm_Net8_Loc_Charts_Not",        true,  true,  true,  false, "wasm")   },
        new object[] { new Scenario("Wasm_Net8_Loc_Charts_Not_Hot",    true,  true,  true,  true,  "wasm")   },
        new object[] { new Scenario("Server_Net10",                    false, false, false, false, "server") },
        new object[] { new Scenario("Server_Net10_Loc",                true,  false, false, false, "server") },
        new object[] { new Scenario("Server_Net10_Loc_Charts",         true,  true,  false, false, "server") },
        new object[] { new Scenario("Server_Net10_Loc_Charts_Not",     true,  true,  true,  false, "server") },
        new object[] { new Scenario("Server_Net10_Loc_Charts_Not_Hot", true,  true,  true,  true,  "server") },
        new object[] { new Scenario("Wasm_Net10",                      false, false, false, false, "wasm")   },
        new object[] { new Scenario("Wasm_Net10_Loc",                  true,  false, false, false, "wasm")   },
        new object[] { new Scenario("Wasm_Net10_Loc_Charts",           true,  true,  false, false, "wasm")   },
        new object[] { new Scenario("Wasm_Net10_Loc_Charts_Not",       true,  true,  true,  false, "wasm")   },
        new object[] { new Scenario("Wasm_Net10_Loc_Charts_Not_Hot",   true,  true,  true,  true,  "wasm")   },
        new object[] { new Scenario("Wasm_Net10_Theme_None",           false, false, false, false, "wasm",   "None") },
        new object[] { new Scenario("Wasm_Net10_Theme_NeoBrutalism",   false, false, false, false, "wasm",   "NeoBrutalism") },
        new object[] { new Scenario("Wasm_Net10_Theme_BentoGrid",      false, false, false, false, "wasm",   "BentoGrid") },
        new object[] { new Scenario("Wasm_Net10_Theme_Glassmorphism",  false, false, false, false, "wasm",   "Glassmorphism") },
        new object[] { new Scenario("Wasm_Net10_Theme_FlatDesign",     false, false, false, false, "wasm",   "FlatDesign") },
        new object[] { new Scenario("Wasm_Net10_Theme_MaterialDesign", false, false, false, false, "wasm",   "MaterialDesign") },
        new object[] { new Scenario("Wasm_Net10_Theme_Neomorphism",    false, false, false, false, "wasm",   "Neomorphism") },
        new object[] { new Scenario("Wasm_Net10_Theme_Claymorphism",   false, false, false, false, "wasm",   "Claymorphism") },
        new object[] { new Scenario("Wasm_Net10_Theme_RetroWeb",       false, false, false, false, "wasm",   "RetroWeb") },
    };

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Home_Razor_Should_Match_Charts_Flag(Scenario sc)
    {
        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;
        string homePath = sc.Template == "server"
            ? Path.Combine(projDir, "Components", "Pages", "Home.razor")
            : Path.Combine(projDir, "Pages", "Home.razor");

        File.Exists(homePath).Should().BeTrue($"Home.razor must exist for {sc.Key}");
        string home = await File.ReadAllTextAsync(homePath, TestContext.Current.CancellationToken);

        if (sc.Charts)
        {
            home.Should().Contain("BlazOrbit.Charts",
                $"Charts=true scenario {sc.Key} must use the dashboard Home variant");
            home.Should().Contain("bob-dashboard__",
                $"dashboard scoped CSS classes must be present in {sc.Key}");
            home.Should().NotContain("bob-showcase__hero",
                $"the showcase Home leaked into Charts=true scenario {sc.Key}");
        }
        else
        {
            home.Should().Contain("bob-showcase__",
                $"showcase scoped CSS classes must be present in NoCharts scenario {sc.Key}");
            home.Should().NotContain("BlazOrbit.Charts",
                $"the dashboard Home leaked into NoCharts scenario {sc.Key}");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task NavMenu_Should_Match_Localization_Flag(Scenario sc)
    {
        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;
        string navPath = sc.Template == "server"
            ? Path.Combine(projDir, "Components", "Layout", "NavMenu.razor")
            : Path.Combine(projDir, "Layout", "NavMenu.razor");

        File.Exists(navPath).Should().BeTrue($"NavMenu.razor must exist for {sc.Key}");
        string nav = await File.ReadAllTextAsync(navPath, TestContext.Current.CancellationToken);

        if (sc.Localization)
        {
            nav.Should().Contain("BOBCultureSelector",
                $"Localization=true scenario {sc.Key} must include the language selector");
        }
        else
        {
            nav.Should().NotContain("BOBCultureSelector",
                $"Localization=false scenario {sc.Key} must NOT include the language selector");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Csproj_Should_Reference_Charts_Package_When_Flagged(Scenario sc)
    {
        string csproj = await File.ReadAllTextAsync(
            _fixture.ProjectPaths[sc.Key], TestContext.Current.CancellationToken);

        if (sc.Charts)
        {
            csproj.Should().Contain("BlazOrbit.Charts",
                $"Charts=true scenario {sc.Key} must reference BlazOrbit.Charts");
        }
        else
        {
            csproj.Should().NotContain("BlazOrbit.Charts",
                $"Charts=false scenario {sc.Key} must NOT reference BlazOrbit.Charts");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Program_Should_Register_Charts_Service_When_Flagged(Scenario sc)
    {
        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;
        string program = await File.ReadAllTextAsync(
            Path.Combine(projDir, "Program.cs"), TestContext.Current.CancellationToken);

        if (sc.Charts)
        {
            program.Should().Contain("AddBlazOrbitCharts",
                $"Charts=true scenario {sc.Key} must register IChartJsInterop via AddBlazOrbitCharts");
        }
        else
        {
            program.Should().NotContain("AddBlazOrbitCharts",
                $"Charts=false scenario {sc.Key} must NOT call AddBlazOrbitCharts");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Wasm_Index_Should_Link_Scoped_Css(Scenario sc)
    {
        if (sc.Template != "wasm") return; // Server uses MapStaticAssets for scoped css

        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;
        string indexPath = Path.Combine(projDir, "wwwroot", "index.html");
        File.Exists(indexPath).Should().BeTrue($"index.html must exist for {sc.Key}");
        string index = await File.ReadAllTextAsync(indexPath, TestContext.Current.CancellationToken);

        // The scoped CSS bundle is named after the project. With sourceName
        // replacement, "BlazorApp.styles.css" becomes "<ProjectName>.styles.css".
        index.Should().Contain($"{sc.Key}.styles.css",
            $"WASM template must link the scoped CSS bundle so per-component .razor.css rules apply ({sc.Key})");
        index.Should().NotMatch("*<!--*styles.css*-->*",
            $"the scoped CSS link must not be commented out ({sc.Key})");
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Wasm_Index_Should_Link_Charts_Css_When_Flagged(Scenario sc)
    {
        if (sc.Template != "wasm") return;

        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;
        string indexPath = Path.Combine(projDir, "wwwroot", "index.html");
        string index = await File.ReadAllTextAsync(indexPath, TestContext.Current.CancellationToken);

        // The chart-interop JS module also auto-injects the CSS, but the
        // template adds an explicit <link> when IncludeCharts=true so the
        // first paint already has chart styles applied.
        if (sc.Charts)
        {
            index.Should().Contain("_content/BlazOrbit.Charts/css/blazorbit-charts.css",
                $"Charts=true WASM template {sc.Key} should pre-load the chart CSS bundle to avoid first-paint flash");
        }
        else
        {
            index.Should().NotContain("blazorbit-charts.css",
                $"Charts=false WASM template {sc.Key} must not link the chart CSS");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Theme_Should_Match_Flag(Scenario sc)
    {
        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;

        if (sc.Template == "server")
        {
            string appPath = Path.Combine(projDir, "Components", "App.razor");
            File.Exists(appPath).Should().BeTrue($"App.razor must exist for {sc.Key}");
            string app = await File.ReadAllTextAsync(appPath, TestContext.Current.CancellationToken);

            if (sc.Theme != "None")
            {
                app.Should().Contain("theme.css",
                    $"Theme={sc.Theme} scenario {sc.Key} must link the theme CSS");
            }
            else
            {
                app.Should().NotContain("theme.css",
                    $"Theme=None scenario {sc.Key} must NOT link the theme CSS");
            }
        }
        else
        {
            string indexPath = Path.Combine(projDir, "wwwroot", "index.html");
            File.Exists(indexPath).Should().BeTrue($"index.html must exist for {sc.Key}");
            string index = await File.ReadAllTextAsync(indexPath, TestContext.Current.CancellationToken);

            if (sc.Theme != "None")
            {
                index.Should().Contain("css/theme.css",
                    $"Theme={sc.Theme} scenario {sc.Key} must link the theme CSS");
            }
            else
            {
                index.Should().NotContain("css/theme.css",
                    $"Theme=None scenario {sc.Key} must NOT link the theme CSS");
            }
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Csproj_Should_Reference_Notifications_Package_When_Flagged(Scenario sc)
    {
        string csproj = await File.ReadAllTextAsync(
            _fixture.ProjectPaths[sc.Key], TestContext.Current.CancellationToken);

        if (sc.Notifications)
        {
            csproj.Should().Contain("BlazOrbit.Notifications",
                $"Notifications=true scenario {sc.Key} must reference BlazOrbit.Notifications");
        }
        else
        {
            csproj.Should().NotContain("BlazOrbit.Notifications",
                $"Notifications=false scenario {sc.Key} must NOT reference BlazOrbit.Notifications");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Csproj_Should_Reference_HotKeys_Package_When_Flagged(Scenario sc)
    {
        string csproj = await File.ReadAllTextAsync(
            _fixture.ProjectPaths[sc.Key], TestContext.Current.CancellationToken);

        if (sc.HotKeys)
        {
            csproj.Should().Contain("BlazOrbit.Hotkeys",
                $"HotKeys=true scenario {sc.Key} must reference BlazOrbit.Hotkeys");
        }
        else
        {
            csproj.Should().NotContain("BlazOrbit.Hotkeys",
                $"HotKeys=false scenario {sc.Key} must NOT reference BlazOrbit.Hotkeys");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Program_Should_Register_Notifications_When_Flagged(Scenario sc)
    {
        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;
        string program = await File.ReadAllTextAsync(
            Path.Combine(projDir, "Program.cs"), TestContext.Current.CancellationToken);

        if (sc.Notifications)
        {
            program.Should().Contain("AddBlazOrbitNotifications",
                $"Notifications=true scenario {sc.Key} must register AddBlazOrbitNotifications");
        }
        else
        {
            program.Should().NotContain("AddBlazOrbitNotifications",
                $"Notifications=false scenario {sc.Key} must NOT call AddBlazOrbitNotifications");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Program_Should_Register_HotKeys_When_Flagged(Scenario sc)
    {
        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;
        string program = await File.ReadAllTextAsync(
            Path.Combine(projDir, "Program.cs"), TestContext.Current.CancellationToken);

        if (sc.HotKeys)
        {
            program.Should().Contain("AddBlazOrbitHotkeys",
                $"HotKeys=true scenario {sc.Key} must register AddBlazOrbitHotkeys");
        }
        else
        {
            program.Should().NotContain("AddBlazOrbitHotkeys",
                $"HotKeys=false scenario {sc.Key} must NOT call AddBlazOrbitHotkeys");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task MainLayout_Should_Contain_HotkeyHost_When_Flagged(Scenario sc)
    {
        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;
        string layoutPath = sc.Template == "server"
            ? Path.Combine(projDir, "Components", "Layout", "MainLayout.razor")
            : Path.Combine(projDir, "Layout", "MainLayout.razor");

        File.Exists(layoutPath).Should().BeTrue($"MainLayout.razor must exist for {sc.Key}");
        string layout = await File.ReadAllTextAsync(layoutPath, TestContext.Current.CancellationToken);

        if (sc.HotKeys)
        {
            layout.Should().Contain("BOBHotkeyHost",
                $"HotKeys=true scenario {sc.Key} must include BOBHotkeyHost");
        }
        else
        {
            layout.Should().NotContain("BOBHotkeyHost",
                $"HotKeys=false scenario {sc.Key} must NOT include BOBHotkeyHost");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task NavMenu_Should_Contain_Bell_When_Flagged(Scenario sc)
    {
        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;
        string navPath = sc.Template == "server"
            ? Path.Combine(projDir, "Components", "Layout", "NavMenu.razor")
            : Path.Combine(projDir, "Layout", "NavMenu.razor");

        File.Exists(navPath).Should().BeTrue($"NavMenu.razor must exist for {sc.Key}");
        string nav = await File.ReadAllTextAsync(navPath, TestContext.Current.CancellationToken);

        if (sc.Notifications)
        {
            nav.Should().Contain("BOBNotificationBell",
                $"Notifications=true scenario {sc.Key} must include BOBNotificationBell");
        }
        else
        {
            nav.Should().NotContain("BOBNotificationBell",
                $"Notifications=false scenario {sc.Key} must NOT include BOBNotificationBell");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Home_Razor_Should_Contain_Notifications_Section_When_Flagged(Scenario sc)
    {
        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;
        string homePath = sc.Template == "server"
            ? Path.Combine(projDir, "Components", "Pages", "Home.razor")
            : Path.Combine(projDir, "Pages", "Home.razor");

        File.Exists(homePath).Should().BeTrue($"Home.razor must exist for {sc.Key}");
        string home = await File.ReadAllTextAsync(homePath, TestContext.Current.CancellationToken);

        if (sc.Notifications)
        {
            home.Should().Contain("INotificationCenter",
                $"Notifications=true scenario {sc.Key} must reference INotificationCenter");
            home.Should().Contain("PushNotificationAsync",
                $"Notifications=true scenario {sc.Key} must contain PushNotificationAsync");
        }
        else
        {
            home.Should().NotContain("INotificationCenter",
                $"Notifications=false scenario {sc.Key} must NOT reference INotificationCenter");
            home.Should().NotContain("PushNotificationAsync",
                $"Notifications=false scenario {sc.Key} must NOT contain PushNotificationAsync");
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public async Task Home_Razor_Should_Contain_Hotkeys_Section_When_Flagged(Scenario sc)
    {
        string projDir = Path.GetDirectoryName(_fixture.ProjectPaths[sc.Key])!;
        string homePath = sc.Template == "server"
            ? Path.Combine(projDir, "Components", "Pages", "Home.razor")
            : Path.Combine(projDir, "Pages", "Home.razor");

        File.Exists(homePath).Should().BeTrue($"Home.razor must exist for {sc.Key}");
        string home = await File.ReadAllTextAsync(homePath, TestContext.Current.CancellationToken);

        if (sc.HotKeys)
        {
            home.Should().Contain("IHotkeyService",
                $"HotKeys=true scenario {sc.Key} must reference IHotkeyService");
            home.Should().Contain("Hotkeys.Register",
                $"HotKeys=true scenario {sc.Key} must contain Hotkeys.Register");
        }
        else
        {
            home.Should().NotContain("IHotkeyService",
                $"HotKeys=false scenario {sc.Key} must NOT reference IHotkeyService");
            home.Should().NotContain("Hotkeys.Register",
                $"HotKeys=false scenario {sc.Key} must NOT contain Hotkeys.Register");
        }
    }
}
