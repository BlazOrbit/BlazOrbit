using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using FluentAssertions;
using Microsoft.Playwright;

namespace BlazOrbit.Templates.E2E;

public sealed class TemplateTestFixture : IAsyncLifetime
{
    private string _repoRoot = "";
    private string _workDir = "";
    private bool _cleanupWorkDir = true;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public Dictionary<string, string> ProjectPaths { get; } = new(StringComparer.Ordinal);

    public async ValueTask InitializeAsync()
    {
        _repoRoot = FindRepoRoot();
        _workDir = ResolveWorkDir();

        var envWorkDir = Environment.GetEnvironmentVariable("BLAZORBIT_TEMPLATE_TEST_DIR");
        var reuseExisting = !string.IsNullOrWhiteSpace(envWorkDir);

        var nupkg = FindTemplateNupkg();
        await InstallTemplatesAsync(nupkg);

        if (reuseExisting)
        {
            // Projects were already generated + built by test-templates.ps1; just discover them
            DiscoverExistingProjects();
        }
        else
        {
            GenerateMatrix();
            await BuildProjectsAsync();
        }

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        _playwright?.Dispose();

        try
        {
            await UninstallTemplatesAsync();
        }
        catch
        {
            // Best-effort cleanup
        }

        if (_cleanupWorkDir && Directory.Exists(_workDir))
        {
            Directory.Delete(_workDir, recursive: true);
        }
    }

    public async Task<IPage> NewPageAsync()
    {
        if (_browser is null)
            throw new InvalidOperationException("Browser not initialized.");

        return await _browser.NewPageAsync();
    }

    public async Task<RunningApp> RunProjectAsync(string key)
    {
        if (!ProjectPaths.TryGetValue(key, out var csproj))
            throw new ArgumentException($"Unknown project key '{key}'.");

        // 1. Ask the OS for a free port
        var port = GetFreePort();
        var url = $"http://127.0.0.1:{port}";

        // -c Release matches what test-templates.ps1 built; without it, dotnet run
        // defaults to Debug and triggers a cold rebuild of every project on each
        // test, which makes the polling-for-200 race fragile.
        // --no-build assumes the projects were built externally (test-templates.ps1
        // -RunE2E does this); when running BlazOrbit.Templates.E2E in isolation the
        // fixture's own BuildProjectsAsync() handles it.
        // ASPNETCORE_ENVIRONMENT=Development matches the experience a real user
        // gets via launchSettings.json. Without it the host defaults to
        // Production, where MapStaticAssets / UseStaticFiles look for publish-time
        // (fingerprinted) assets that don't exist after a plain `dotnet build`,
        // so all `_content/<rcl>/*` requests 404.
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{csproj}\" -c Release --no-build --urls {url} --no-launch-profile",
            WorkingDirectory = Path.GetDirectoryName(csproj)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
        psi.EnvironmentVariables["DOTNET_ENVIRONMENT"] = "Development";

        var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

        var stdOutBuffer = new List<string>();
        var stdErrBuffer = new List<string>();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null) stdOutBuffer.Add(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null) stdErrBuffer.Add(e.Data);
        };

        Console.WriteLine($"[E2E] Starting {key}: dotnet {psi.Arguments}");
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // 2. Poll the known port until it responds
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var reachable = await RetryAsync(async () =>
        {
            try
            {
                var response = await http.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }, maxRetries: 60, delayMs: 500);

        if (!reachable)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
            await Task.Delay(500); // give buffers time to drain

            var logDir = Path.Combine(Path.GetTempPath(), "blazorbit-e2e-logs");
            Directory.CreateDirectory(logDir);
            var outLog = Path.Combine(logDir, $"{key}-stdout.log");
            var errLog = Path.Combine(logDir, $"{key}-stderr.log");
            await File.WriteAllLinesAsync(outLog, stdOutBuffer);
            await File.WriteAllLinesAsync(errLog, stdErrBuffer);

            throw new TimeoutException(
                $"Project '{key}' URL {url} was not reachable after 30 seconds. " +
                $"Logs written to: {outLog} and {errLog}");
        }

        Console.WriteLine($"[E2E] {key} is ready at {url}");
        return new RunningApp(process, url, key, stdOutBuffer, stdErrBuffer);
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "BlazOrbit.slnx")))
                return dir;

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException(
            "Could not locate repo root. Ensure BlazOrbit.slnx exists in an ancestor directory.");
    }

    private string ResolveWorkDir()
    {
        var env = Environment.GetEnvironmentVariable("BLAZORBIT_TEMPLATE_TEST_DIR");
        if (!string.IsNullOrWhiteSpace(env))
        {
            _cleanupWorkDir = false;
            return env;
        }

        var path = Path.Combine(_repoRoot, "artifacts", "template-tests-e2e");
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);

        Directory.CreateDirectory(path);
        return path;
    }

    private string FindTemplateNupkg()
    {
        var feed = Path.Combine(_repoRoot, "artifacts", "local-feed");
        if (!Directory.Exists(feed))
            throw new InvalidOperationException($"Local feed not found: {feed}. Run ./scripts/test-templates.ps1 first to pack templates.");

        var pkg = Directory.GetFiles(feed, "BlazOrbit.Templates.*.nupkg")
            .OrderByDescending(File.GetLastWriteTime)
            .FirstOrDefault();

        if (pkg is null)
            throw new InvalidOperationException($"No BlazOrbit.Templates nupkg found in {feed}.");

        return pkg;
    }

    private async Task InstallTemplatesAsync(string nupkg)
    {
        // Uninstall first to avoid conflicts
        await RunDotNetAsync(new[] { "new", "uninstall", "BlazOrbit.Templates" }, ignoreExitCode: true);
        await RunDotNetAsync(new[] { "new", "install", nupkg, "--force" }, ignoreExitCode: false);
    }

    private async Task UninstallTemplatesAsync()
    {
        await RunDotNetAsync(new[] { "new", "uninstall", "BlazOrbit.Templates" }, ignoreExitCode: true);
    }

    private void DiscoverExistingProjects()
    {
        var matrix = new[]
        {
            new { Key = "Server_Net8", Template = "blazorbit-server", Framework = "net8.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net8_Loc", Template = "blazorbit-server", Framework = "net8.0", Localization = true, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net8_Loc_Charts", Template = "blazorbit-server", Framework = "net8.0", Localization = true, Charts = true, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net8_Loc_Charts_Not", Template = "blazorbit-server", Framework = "net8.0", Localization = true, Charts = true, Notifications = true, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net8_Loc_Charts_Not_Hot", Template = "blazorbit-server", Framework = "net8.0", Localization = true, Charts = true, Notifications = true, HotKeys = true, Theme = "None" },
            new { Key = "Wasm_Net8", Template = "blazorbit-wasm", Framework = "net8.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net8_Loc", Template = "blazorbit-wasm", Framework = "net8.0", Localization = true, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net8_Loc_Charts", Template = "blazorbit-wasm", Framework = "net8.0", Localization = true, Charts = true, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net8_Loc_Charts_Not", Template = "blazorbit-wasm", Framework = "net8.0", Localization = true, Charts = true, Notifications = true, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net8_Loc_Charts_Not_Hot", Template = "blazorbit-wasm", Framework = "net8.0", Localization = true, Charts = true, Notifications = true, HotKeys = true, Theme = "None" },
            new { Key = "Server_Net10", Template = "blazorbit-server", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net10_Loc", Template = "blazorbit-server", Framework = "net10.0", Localization = true, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net10_Loc_Charts", Template = "blazorbit-server", Framework = "net10.0", Localization = true, Charts = true, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net10_Loc_Charts_Not", Template = "blazorbit-server", Framework = "net10.0", Localization = true, Charts = true, Notifications = true, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net10_Loc_Charts_Not_Hot", Template = "blazorbit-server", Framework = "net10.0", Localization = true, Charts = true, Notifications = true, HotKeys = true, Theme = "None" },
            new { Key = "Wasm_Net10", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net10_Loc", Template = "blazorbit-wasm", Framework = "net10.0", Localization = true, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net10_Loc_Charts", Template = "blazorbit-wasm", Framework = "net10.0", Localization = true, Charts = true, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net10_Loc_Charts_Not", Template = "blazorbit-wasm", Framework = "net10.0", Localization = true, Charts = true, Notifications = true, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net10_Loc_Charts_Not_Hot", Template = "blazorbit-wasm", Framework = "net10.0", Localization = true, Charts = true, Notifications = true, HotKeys = true, Theme = "None" },
            new { Key = "Wasm_Net10_Theme_None", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net10_Theme_NeoBrutalism", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "NeoBrutalism" },
            new { Key = "Wasm_Net10_Theme_BentoGrid", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "BentoGrid" },
            new { Key = "Wasm_Net10_Theme_Glassmorphism", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "Glassmorphism" },
            new { Key = "Wasm_Net10_Theme_FlatDesign", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "FlatDesign" },
            new { Key = "Wasm_Net10_Theme_MaterialDesign", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "MaterialDesign" },
            new { Key = "Wasm_Net10_Theme_Neomorphism", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "Neomorphism" },
            new { Key = "Wasm_Net10_Theme_Claymorphism", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "Claymorphism" },
            new { Key = "Wasm_Net10_Theme_RetroWeb", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "RetroWeb" },
        };

        foreach (var item in matrix)
        {
            var caseDir = Path.Combine(_workDir, item.Key);
            if (!Directory.Exists(caseDir))
            {
                throw new InvalidOperationException($"Expected project directory not found: {caseDir}. Run test-templates.ps1 first or unset BLAZORBIT_TEMPLATE_TEST_DIR.");
            }

            var csproj = Directory.GetFiles(caseDir, "*.csproj", SearchOption.AllDirectories).FirstOrDefault()
                ?? throw new InvalidOperationException($"No .csproj found for {item.Key} under {caseDir}");

            ProjectPaths[item.Key] = csproj;
        }
    }

    private void GenerateMatrix()
    {
        var matrix = new[]
        {
            new { Key = "Server_Net8", Template = "blazorbit-server", Framework = "net8.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net8_Loc", Template = "blazorbit-server", Framework = "net8.0", Localization = true, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net8_Loc_Charts", Template = "blazorbit-server", Framework = "net8.0", Localization = true, Charts = true, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net8_Loc_Charts_Not", Template = "blazorbit-server", Framework = "net8.0", Localization = true, Charts = true, Notifications = true, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net8_Loc_Charts_Not_Hot", Template = "blazorbit-server", Framework = "net8.0", Localization = true, Charts = true, Notifications = true, HotKeys = true, Theme = "None" },
            new { Key = "Wasm_Net8", Template = "blazorbit-wasm", Framework = "net8.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net8_Loc", Template = "blazorbit-wasm", Framework = "net8.0", Localization = true, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net8_Loc_Charts", Template = "blazorbit-wasm", Framework = "net8.0", Localization = true, Charts = true, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net8_Loc_Charts_Not", Template = "blazorbit-wasm", Framework = "net8.0", Localization = true, Charts = true, Notifications = true, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net8_Loc_Charts_Not_Hot", Template = "blazorbit-wasm", Framework = "net8.0", Localization = true, Charts = true, Notifications = true, HotKeys = true, Theme = "None" },
            new { Key = "Server_Net10", Template = "blazorbit-server", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net10_Loc", Template = "blazorbit-server", Framework = "net10.0", Localization = true, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net10_Loc_Charts", Template = "blazorbit-server", Framework = "net10.0", Localization = true, Charts = true, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net10_Loc_Charts_Not", Template = "blazorbit-server", Framework = "net10.0", Localization = true, Charts = true, Notifications = true, HotKeys = false, Theme = "None" },
            new { Key = "Server_Net10_Loc_Charts_Not_Hot", Template = "blazorbit-server", Framework = "net10.0", Localization = true, Charts = true, Notifications = true, HotKeys = true, Theme = "None" },
            new { Key = "Wasm_Net10", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net10_Loc", Template = "blazorbit-wasm", Framework = "net10.0", Localization = true, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net10_Loc_Charts", Template = "blazorbit-wasm", Framework = "net10.0", Localization = true, Charts = true, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net10_Loc_Charts_Not", Template = "blazorbit-wasm", Framework = "net10.0", Localization = true, Charts = true, Notifications = true, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net10_Loc_Charts_Not_Hot", Template = "blazorbit-wasm", Framework = "net10.0", Localization = true, Charts = true, Notifications = true, HotKeys = true, Theme = "None" },
            new { Key = "Wasm_Net10_Theme_None", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "None" },
            new { Key = "Wasm_Net10_Theme_NeoBrutalism", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "NeoBrutalism" },
            new { Key = "Wasm_Net10_Theme_BentoGrid", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "BentoGrid" },
            new { Key = "Wasm_Net10_Theme_Glassmorphism", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "Glassmorphism" },
            new { Key = "Wasm_Net10_Theme_FlatDesign", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "FlatDesign" },
            new { Key = "Wasm_Net10_Theme_MaterialDesign", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "MaterialDesign" },
            new { Key = "Wasm_Net10_Theme_Neomorphism", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "Neomorphism" },
            new { Key = "Wasm_Net10_Theme_Claymorphism", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "Claymorphism" },
            new { Key = "Wasm_Net10_Theme_RetroWeb", Template = "blazorbit-wasm", Framework = "net10.0", Localization = false, Charts = false, Notifications = false, HotKeys = false, Theme = "RetroWeb" },
        };

        foreach (var item in matrix)
        {
            var caseDir = Path.Combine(_workDir, item.Key);
            Directory.CreateDirectory(caseDir);

            RunDotNetSync(new[]
            {
                "new", item.Template,
                "-n", item.Key,
                "-o", caseDir,
                "--Framework", item.Framework,
                "--IncludeLocalization", item.Localization.ToString().ToLowerInvariant(),
                "--IncludeCharts", item.Charts.ToString().ToLowerInvariant(),
                "--UseNotificationsCenter", item.Notifications.ToString().ToLowerInvariant(),
                "--UseHotKeys", item.HotKeys.ToString().ToLowerInvariant(),
                "--Theme", item.Theme
            });

            var csproj = Directory.GetFiles(caseDir, "*.csproj", SearchOption.AllDirectories).FirstOrDefault()
                ?? throw new InvalidOperationException($"No .csproj found for {item.Key}");

            ProjectPaths[item.Key] = csproj;
        }
    }

    private async Task BuildProjectsAsync()
    {
        foreach (var (key, csproj) in ProjectPaths)
        {
            await RunDotNetAsync(new[] { "build", csproj, "-c", "Release", "--nologo" }, ignoreExitCode: false, description: $"build {key}");
        }
    }

    private static void RunDotNetSync(string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = string.Join(" ", args.Select(a => a.Contains(' ') ? $"\"{a}\"" : a)),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet process.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var err = errorTask.GetAwaiter().GetResult();
            throw new InvalidOperationException($"dotnet {string.Join(" ", args)} failed: {err}");
        }
    }

    private static async Task RunDotNetAsync(string[] args, bool ignoreExitCode, string? description = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = string.Join(" ", args.Select(a => a.Contains(' ') ? $"\"{a}\"" : a)),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet process.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0 && !ignoreExitCode)
        {
            var err = await errorTask;
            throw new InvalidOperationException($"dotnet {string.Join(" ", args)} failed: {err}");
        }
    }

    private static async Task<bool> RetryAsync(Func<Task<bool>> action, int maxRetries, int delayMs)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                if (await action())
                    return true;
            }
            catch
            {
                // ignore and retry
            }

            await Task.Delay(delayMs);
        }

        return false;
    }
}

[CollectionDefinition(nameof(TemplateTestCollection))]
public class TemplateTestCollection : ICollectionFixture<TemplateTestFixture>
{
}
