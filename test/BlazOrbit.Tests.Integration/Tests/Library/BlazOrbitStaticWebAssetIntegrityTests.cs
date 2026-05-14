using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Guards that every <c>integrity</c> hash declared by the Razor SDK in the
/// per-TFM static-web-assets endpoints manifest actually matches the SHA-256 of
/// the asset bytes it points to.
///
/// Regression context (1.0.0-preview.46): the published BlazOrbit nupkg shipped
/// with <c>build/Microsoft.AspNetCore.StaticWebAssetEndpoints.props</c> declaring
/// integrity values that did <i>not</i> match the bytes of the JS modules
/// inside the same nupkg. .NET 10 consumers enforce SRI on RCL JS modules
/// loaded via importmap (Blazor WASM in particular), so the browser blocked
/// <c>ThemeInterop</c> and <c>LocalStorageInterop</c>, which in turn made
/// <c>BOBPalette</c>'s constructor throw
/// <c>KeyNotFoundException("--palette-background")</c> from
/// <c>BOBInitializer.OnAfterRenderAsync</c> and prevented <c>ChildContent</c>
/// from ever rendering — i.e. all WASM net10 template E2E tests timed out.
///
/// Root cause: the pre-refactor maintainer pipeline (now removed) regenerated
/// <c>wwwroot/js/**/*.min.js</c> via Vite during both the outer multi-TFM
/// build and every inner build, racing the integrity capture against the
/// bytes shipped in the .nupkg. After the build-tools refactor, those JS
/// modules are committed JSDoc-typed <c>.js</c> source files — no regeneration
/// step, no race surface — but this test stays as a permanent guard against
/// any future drift between manifest integrity and asset bytes.
///
/// The test fails fast whenever any future change reintroduces that drift: it
/// parses <c>obj/&lt;Config&gt;/&lt;TFM&gt;/staticwebassets.build.endpoints.json</c>
/// (the source the Pack step consumes) and asserts integrity == SHA-256(asset
/// bytes) for every endpoint that declares one.
/// </summary>
[Trait("Library", "Packaging")]
public class BlazOrbitStaticWebAssetIntegrityTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();

    private const string Configuration =
#if DEBUG
        "Debug";
#else
        "Release";
#endif

    /// <summary>
    /// Libraries that ship JS/CSS under <c>wwwroot/</c> AND target net10.0 (so
    /// the Razor SDK emits a <c>StaticWebAssetEndpoints</c> manifest with
    /// integrity hashes). Charts/Hotkeys are included defensively — even though
    /// their build pipelines already use MSBuild Inputs/Outputs incrementality,
    /// a future change could introduce the same race pattern.
    /// </summary>
    public static IEnumerable<object[]> Net10Projects() => new[]
    {
        new object[] { "BlazOrbit" },
        new object[] { "BlazOrbit.Charts" },
        new object[] { "BlazOrbit.Hotkeys" },
    };

    [Theory]
    [MemberData(nameof(Net10Projects))]
    public void Integrity_in_endpoints_manifest_must_match_asset_file_bytes(string projectDir)
    {
        var manifestPath = Path.Combine(
            RepoRoot, "src", projectDir, "obj", Configuration, "net10.0",
            "staticwebassets.build.endpoints.json");

        File.Exists(manifestPath).Should().BeTrue(
            $"the Razor SDK must have produced {manifestPath} during the {projectDir} build " +
            $"(run 'dotnet build BlazOrbit.slnx -c {Configuration}' first).");

        using var stream = File.OpenRead(manifestPath);
        using var doc = JsonDocument.Parse(stream);

        var endpoints = doc.RootElement.GetProperty("Endpoints").EnumerateArray().ToList();
        endpoints.Should().NotBeEmpty($"{manifestPath} must list at least one endpoint.");

        // Cache file → hash so we don't rehash the same file once per fingerprinted endpoint.
        var hashCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var mismatches = new List<string>();

        foreach (var endpoint in endpoints)
        {
            var route = endpoint.GetProperty("Route").GetString()!;
            var assetFileRelative = endpoint.GetProperty("AssetFile").GetString()!;
            string? declaredIntegrity = null;

            foreach (var prop in endpoint.GetProperty("EndpointProperties").EnumerateArray())
            {
                if (prop.GetProperty("Name").GetString() == "integrity")
                {
                    declaredIntegrity = prop.GetProperty("Value").GetString();
                    break;
                }
            }

            if (declaredIntegrity is null)
            {
                continue;
            }

            // AssetFile is either an absolute path or relative to the project's
            // wwwroot/. Razor SDK writes it relative for most assets; resolve
            // both layouts.
            var assetPath = ResolveAssetPath(projectDir, assetFileRelative);
            if (!File.Exists(assetPath))
            {
                // Compressed sibling (.gz / .br) may not be regenerated on every
                // platform-specific build; skip rather than fail loudly.
                continue;
            }

            if (!hashCache.TryGetValue(assetPath, out var actualHash))
            {
                using var fs = File.OpenRead(assetPath);
                using var sha = SHA256.Create();
                actualHash = "sha256-" + Convert.ToBase64String(sha.ComputeHash(fs));
                hashCache[assetPath] = actualHash;
            }

            if (!string.Equals(declaredIntegrity, actualHash, StringComparison.Ordinal))
            {
                mismatches.Add(
                    $"  Route={route}\n    asset={assetPath}\n    declared={declaredIntegrity}\n    actual  ={actualHash}");
            }
        }

        mismatches.Should().BeEmpty(
            $"every endpoint's declared integrity must equal SHA-256 of its AssetFile, " +
            $"otherwise consumers' browsers will block the resource via SRI. " +
            $"Mismatches in {manifestPath}:\n" + string.Join("\n", mismatches));
    }

    private static string ResolveAssetPath(string projectDir, string assetFileRelative)
    {
        // .NET 10 emits AssetFile relative to the project's wwwroot/ for build-time
        // assets (e.g. "js/Types/Theme/ThemeInterop.js"). For scoped CSS bundles
        // and gzip companions it sometimes emits paths relative to the project
        // intermediate output. Try wwwroot first, then obj.
        var wwwrootCandidate = Path.Combine(RepoRoot, "src", projectDir, "wwwroot", assetFileRelative);
        if (File.Exists(wwwrootCandidate))
        {
            return wwwrootCandidate;
        }

        // Try as absolute path (Razor SDK occasionally emits one).
        if (Path.IsPathRooted(assetFileRelative) && File.Exists(assetFileRelative))
        {
            return assetFileRelative;
        }

        // Fall back to obj/<Config>/net10.0/ for compressed/scoped variants.
        var objCandidate = Path.Combine(
            RepoRoot, "src", projectDir, "obj", Configuration, "net10.0", assetFileRelative);
        return objCandidate;
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
