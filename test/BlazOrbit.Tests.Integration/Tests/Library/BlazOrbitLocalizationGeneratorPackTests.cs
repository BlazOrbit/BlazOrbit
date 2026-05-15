using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Pins the source generator into the consumer-facing localization nupkg.
///
/// <c>BlazOrbit.Localization.CodeGeneration</c> is a Roslyn source generator that scans
/// <c>[assembly: BobLocalizationBundle(typeof(TMarker))]</c> declarations and emits a
/// <c>[ModuleInitializer]</c> that registers each bundle via <c>BobLocalize.RegisterBundle</c>.
/// Without that initializer, <see cref="BlazOrbit.Localization.BobLocalizer{T}"/> falls back
/// to literal - consumer translations silently never apply.
///
/// <para>
/// Roslyn source generators only run for downstream consumers when they ship under
/// <c>analyzers/dotnet/cs/</c> inside a nupkg. <c>ProjectReference</c> with
/// <c>OutputItemType="Analyzer"</c> wires the generator to the *referencing* project's own
/// compilation but DOES NOT propagate it to consumers of the resulting nupkg - that's the
/// regression we guard against here.
/// </para>
///
/// <para>
/// The test re-packs <c>BlazOrbit.Localization.Shared</c> into a temp directory (cheap with
/// <c>--no-build --no-restore</c> because the test project already depends on it) and asserts
/// the analyzer DLL lands under <c>analyzers/dotnet/cs/</c>.
/// </para>
/// </summary>
[Trait("Library", "Packaging")]
public sealed class BlazOrbitLocalizationGeneratorPackTests : IAsyncLifetime
{
    private const string Configuration =
#if DEBUG
        "Debug";
#else
        "Release";
#endif

    private static readonly string RepoRoot = ResolveRepoRoot();
    private string _tempDir = "";

    public async ValueTask InitializeAsync()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"blazorbit-loc-pack-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var csproj = Path.Combine(RepoRoot, "src", "BlazOrbit.Localization.Shared",
            "BlazOrbit.Localization.Shared.csproj");

        var psi = new ProcessStartInfo("dotnet",
            $"pack \"{csproj}\" -c {Configuration} --no-build --no-restore -o \"{_tempDir}\" --nologo")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var p = Process.Start(psi)!;
        var stdout = await p.StandardOutput.ReadToEndAsync();
        var stderr = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();

        if (p.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"`dotnet pack` for {csproj} failed (exit {p.ExitCode}).\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");
        }
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup - leaving the temp dir behind only costs disk space.
        }

        return ValueTask.CompletedTask;
    }

    [Fact(DisplayName = "BlazOrbit_Localization_Shared_nupkg_ships_the_CodeGeneration_analyzer")]
    public void BlazOrbit_Localization_Shared_nupkg_ships_the_CodeGeneration_analyzer()
    {
        var nupkg = Directory.GetFiles(_tempDir, "BlazOrbit.Localization.Shared.*.nupkg")
            .FirstOrDefault();

        nupkg.Should().NotBeNull(
            $"`dotnet pack` must have produced exactly one BlazOrbit.Localization.Shared.*.nupkg in {_tempDir}");

        using var archive = ZipFile.OpenRead(nupkg!);
        var entries = archive.Entries.Select(e => e.FullName).ToList();

        // Roslyn convention: source generators ship under analyzers/dotnet/cs/ so the
        // .NET SDK auto-applies them to every consumer that installs the nupkg.
        entries.Should().Contain(e =>
            e.Equals("analyzers/dotnet/cs/BlazOrbit.Localization.CodeGeneration.dll",
                StringComparison.Ordinal),
            "the source generator DLL must ship under 'analyzers/dotnet/cs/' inside the nupkg " +
            "so consumer projects automatically run BlazOrbit.Localization.CodeGeneration on their " +
            "own assemblies - without it, the consumer's [assembly: BobLocalizationBundle(...)] " +
            "declarations never produce the [ModuleInitializer] that registers bundles at runtime, " +
            "and IStringLocalizer<T> silently falls back to literal keys. " +
            "Entries present: " + string.Join(", ", entries.OrderBy(e => e, StringComparer.Ordinal)));

        // Companion: the buildTransitive .targets must ship too so consumers don't need to
        // manually wire `<AdditionalFiles Include="Translations/**/*.tn"/>` in their csproj
        // for the generator to see their translation files.
        entries.Should().Contain(e =>
            e.Equals("buildTransitive/BlazOrbit.Localization.Shared.targets",
                StringComparison.Ordinal),
            "the .targets file under buildTransitive/ must ship so NuGet auto-imports it " +
            "in the consumer build and the generator picks up Translations/**/*.tn files " +
            "without per-project boilerplate. " +
            "Entries present: " + string.Join(", ", entries.OrderBy(e => e, StringComparer.Ordinal)));
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
