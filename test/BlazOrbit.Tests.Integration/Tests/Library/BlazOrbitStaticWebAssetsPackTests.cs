using System.Runtime.CompilerServices;
using System.Text.Json;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Pins the contents of every BlazOrbit-shipping nupkg's static-web-assets manifest.
///
/// For each Razor class library that ships JS/CSS under <c>wwwroot/</c> the Razor SDK
/// writes <c>obj/&lt;Config&gt;/&lt;TFM&gt;/staticwebassets.pack.json</c> during the
/// normal build (no <c>dotnet pack</c> required) listing every file that will end up
/// under <c>staticwebassets/</c> inside the produced .nupkg. Consumers reference those
/// entries via <c>_content/&lt;PackageId&gt;/...</c> at runtime — and a missing entry
/// surfaces as a 404 on the consumer's first page load, with no build/test failure.
///
/// Without this guard a regression like the one in commit <c>f57da79</c> can slip
/// through: <c>wwwroot/css/blazorbit.css</c> + the JS interop modules end up shipped
/// only under <c>content/</c> (regular NuGet content) and consumer apps hit 404 on
/// every <c>_content/BlazOrbit/...</c> URL. Build and tests stay green because the
/// bundled scoped CSS <i>does</i> reach static web assets — only the interop assets
/// are missing.
///
/// Charts + Hotkeys are covered defensively: today their csprojs don't replicate
/// BlazOrbit's clean/regen pattern so they're unaffected by the root cause, but if a
/// future refactor copies the broken pattern over, this test fails immediately.
///
/// The test reads <c>pack.json</c> for the same Configuration the test was built with,
/// so CI's <c>dotnet test -c Release --no-build</c> step naturally exercises the
/// Release manifest (the one consumers actually receive).
/// </summary>
[Trait("Library", "Packaging")]
public class BlazOrbitStaticWebAssetsPackTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();

    private const string Configuration =
#if DEBUG
        "Debug";
#else
        "Release";
#endif

    /// <summary>
    /// (ProjectDirectoryName, TargetFramework, expected <c>RelativePath</c> entries
    /// inside <c>staticwebassets/</c>). Whenever a new asset is added under any of the
    /// listed libraries' <c>wwwroot/</c> add a row here so the test guards it too.
    /// </summary>
    public static IEnumerable<object[]> Manifests() => new[]
    {
        // BlazOrbit — main design system. Loaded via BOBInitializer + every template's
        // App.razor / index.html through `_content/BlazOrbit/css/blazorbit.css` and
        // the JSModulesReference constants.
        new object[] { "BlazOrbit", "net8.0",  new[] { "css/blazorbit.css", "js/Types/Theme/ThemeInterop.js" } },
        new object[] { "BlazOrbit", "net10.0", new[] { "css/blazorbit.css", "js/Types/Theme/ThemeInterop.js" } },

        // BlazOrbit.Charts — SVG-native charts. `_content/BlazOrbit.Charts/...` paths.
        new object[] { "BlazOrbit.Charts", "net8.0",  new[] { "css/blazorbit-charts.css", "js/Types/Chart/ChartInterop.js" } },
        new object[] { "BlazOrbit.Charts", "net10.0", new[] { "css/blazorbit-charts.css", "js/Types/Chart/ChartInterop.js" } },

        // BlazOrbit.Hotkeys — keyboard registry. JS-only (no scoped CSS).
        new object[] { "BlazOrbit.Hotkeys", "net8.0",  new[] { "js/Hotkey/HotkeyInterop.js" } },
        new object[] { "BlazOrbit.Hotkeys", "net10.0", new[] { "js/Hotkey/HotkeyInterop.js" } },
    };

    [Theory]
    [MemberData(nameof(Manifests))]
    public void Should_register_consumer_facing_assets_as_static_web_assets_When_packed(
        string projectDir, string tfm, string[] expectedRelativePaths)
    {
        var manifestPath = Path.Combine(
            RepoRoot, "src", projectDir, "obj", Configuration, tfm, "staticwebassets.pack.json");

        File.Exists(manifestPath).Should().BeTrue(
            $"the Razor SDK must have produced {manifestPath} during the {projectDir} build " +
            $"(run 'dotnet build BlazOrbit.slnx -c {Configuration}' first).");

        using var stream = File.OpenRead(manifestPath);
        using var doc = JsonDocument.Parse(stream);

        doc.RootElement.TryGetProperty("Files", out var files).Should().BeTrue(
            $"{manifestPath} must contain a 'Files' array.");

        // pack.json's PackagePath is `staticwebassets\<RelativePath>` on Windows and
        // `staticwebassets/<RelativePath>` on Linux. Normalize then strip the prefix
        // so the assertion data above stays portable.
        var registered = files
            .EnumerateArray()
            .Select(f => f.GetProperty("PackagePath").GetString())
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => p!.Replace('\\', '/'))
            .Where(p => p.StartsWith("staticwebassets/", StringComparison.Ordinal))
            .Select(p => p["staticwebassets/".Length..])
            .ToHashSet(StringComparer.Ordinal);

        foreach (var expected in expectedRelativePaths)
        {
            registered.Should().Contain(expected,
                $"the {Configuration}/{tfm} {projectDir}.nupkg must ship '{expected}' as a static web asset " +
                $"so consumers can load it via '_content/{projectDir}/{expected}'. " +
                "Registered: " + string.Join(", ", registered.OrderBy(p => p, StringComparer.Ordinal)));
        }
    }

    private static string ResolveRepoRoot([CallerFilePath] string? thisFile = null)
    {
        DirectoryInfo? dir = new FileInfo(thisFile!).Directory;
        for (int i = 0; i < 4 && dir is not null; i++)
        {
            dir = dir.Parent;
        }

        return dir!.FullName;
    }
}
