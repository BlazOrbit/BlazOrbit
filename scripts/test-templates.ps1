#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    End-to-end smoke test for the BlazOrbit dotnet templates.

.DESCRIPTION
    Packs the BlazOrbit library + the templates package into a local NuGet feed,
    installs the templates with `dotnet new install`, generates a matrix of
    sample projects (server / wasm × net8 / net10 × +/- localization × +/- charts),
    builds each, writes an aggregating slnx for manual exploration, and
    uninstalls the templates afterwards.

    The local feed lets the generated projects resolve `BlazOrbit 1.0.*` against
    the just-built bits instead of the public NuGet, so this script is the only
    way to validate template changes against unreleased library changes.

    Default matrix (20 combos — accumulative features across both templates):
        | #  | template          | framework | localization | charts | notifications | hotkeys |
        |----|-------------------|-----------|--------------|--------|---------------|---------|
        | 1  | blazorbit-server  | net8.0    | false        | false  | false         | false   |
        | 2  | blazorbit-server  | net8.0    | true         | false  | false         | false   |
        | 3  | blazorbit-server  | net8.0    | true         | true   | false         | false   |
        | 4  | blazorbit-server  | net8.0    | true         | true   | true          | false   |
        | 5  | blazorbit-server  | net8.0    | true         | true   | true          | true    |
        | 6  | blazorbit-wasm    | net8.0    | false        | false  | false         | false   |
        | 7  | blazorbit-wasm    | net8.0    | true         | false  | false         | false   |
        | 8  | blazorbit-wasm    | net8.0    | true         | true   | false         | false   |
        | 9  | blazorbit-wasm    | net8.0    | true         | true   | true          | false   |
        | 10 | blazorbit-wasm    | net8.0    | true         | true   | true          | true    |
        | 11 | blazorbit-server  | net10.0   | false        | false  | false         | false   |
        | 12 | blazorbit-server  | net10.0   | true         | false  | false         | false   |
        | 13 | blazorbit-server  | net10.0   | true         | true   | false         | false   |
        | 14 | blazorbit-server  | net10.0   | true         | true   | true          | false   |
        | 15 | blazorbit-server  | net10.0   | true         | true   | true          | true    |
        | 16 | blazorbit-wasm    | net10.0   | false        | false  | false         | false   |
        | 17 | blazorbit-wasm    | net10.0   | true         | false  | false         | false   |
        | 18 | blazorbit-wasm    | net10.0   | true         | true   | false         | false   |
        | 19 | blazorbit-wasm    | net10.0   | true         | true   | true          | false   |
        | 20 | blazorbit-wasm    | net10.0   | true         | true   | true          | true    |

    After the matrix runs, an aggregating `template-tests.slnx` is written at
    the work-dir root referencing every successfully-generated project. Pair
    with -KeepWorkDir to load it in VS / Rider or run
    `dotnet build artifacts/template-tests/template-tests.slnx` for ad-hoc
    inspection — the local feed stays configured so package restore works.

.PARAMETER Configuration
    MSBuild configuration for the pack + build steps. Default: Release.

.PARAMETER FeedDir
    Directory used as the local NuGet feed. Default:
    `<repo>/artifacts/local-feed`. Created if missing, never deleted automatically.

.PARAMETER WorkDir
    Directory where the generated test projects land. Default:
    `<repo>/artifacts/template-tests`. Wiped at the start of every run unless
    -KeepWorkDir is set.

.PARAMETER SkipBlazOrbitPack
    Skip packing the BlazOrbit library. Useful when iterating on the template
    only and the library packages already exist in the feed.

.PARAMETER SkipBuild
    Generate the projects but skip `dotnet build` on each. Faster smoke for
    pure template/content edits.

.PARAMETER KeepWorkDir
    Keep generated projects on disk after the run for manual inspection.

.PARAMETER KeepInstalled
    Skip the final `dotnet new uninstall BlazOrbit.Templates`. Useful if you
    want to keep poking the templates manually after the script finishes.

.PARAMETER Matrix
    Override the test matrix. Each entry is a hashtable with keys
    `Template`, `Framework`, `Localization`, `Charts`, `Name`.

.EXAMPLE
    ./scripts/test-templates.ps1
    Full run: pack everything, install, build all 4 combos, uninstall.

.EXAMPLE
    ./scripts/test-templates.ps1 -SkipBlazOrbitPack
    Faster iteration when only template content changed.

.EXAMPLE
    ./scripts/test-templates.ps1 -SkipBlazOrbitPack -SkipBuild -KeepWorkDir -KeepInstalled
    Just regenerate projects, keep them on disk and the templates installed.

.EXAMPLE
    ./scripts/test-templates.ps1 -RunE2E
    After building all projects, run Playwright end-to-end tests against the generated projects.

.EXAMPLE
    ./scripts/test-templates.ps1 -Help
    Show this help message and exit.

.NOTES
    Idempotent: a previous install of `BlazOrbit.Templates` is uninstalled
    before reinstalling. Local feed source is added if missing, left in place
    so subsequent runs are faster.
#>

[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$FeedDir = "",
    [string]$WorkDir = "",
    [switch]$SkipBlazOrbitPack,
    [switch]$SkipBuild,
    [switch]$RunE2E,
    [switch]$KeepWorkDir,
    [switch]$KeepInstalled,
    [hashtable[]]$Matrix,
    [Alias("h")]
    [switch]$Help
)

if ($Help) {
    Get-Help $PSCommandPath -Full | Out-String | Write-Host
    exit 0
}

$ErrorActionPreference = "Stop"

# ---------- Resolve repo root ----------
$repoRoot = $PSScriptRoot
while ($repoRoot -and -not (Test-Path (Join-Path $repoRoot "BlazOrbit.slnx"))) {
    $parent = Split-Path $repoRoot -Parent
    if ($parent -eq $repoRoot) { break }
    $repoRoot = $parent
}
if (-not (Test-Path (Join-Path $repoRoot "BlazOrbit.slnx"))) {
    throw "Cannot locate BlazOrbit.slnx. Run from inside the repo."
}

if ([string]::IsNullOrWhiteSpace($FeedDir)) {
    $FeedDir = Join-Path $repoRoot "artifacts\local-feed"
}
if ([string]::IsNullOrWhiteSpace($WorkDir)) {
    $WorkDir = Join-Path $repoRoot "artifacts\template-tests"
}

$mainSln       = Join-Path $repoRoot "BlazOrbit.slnx"
$templatesSln  = Join-Path $repoRoot "templates\BlazOrbit.Templates.slnx"
$templatesProj = Join-Path $repoRoot "templates\BlazOrbit.Templates.csproj"

# ---------- Default matrix ----------
# Eight cases cover all (Loc × Charts) cells across both templates so the
# `IncludeCharts` opt-in interacts cleanly with the existing Localization
# wiring (Program.cs / Layout / NavMenu variants). The Loc+Charts cells
# are the ones most likely to break — both modifiers reach into the
# project root and conflicts in `template.json` exclusions surface here.
if (-not $Matrix -or $Matrix.Count -eq 0) {
    $Matrix = @(
        @{ Template = "blazorbit-server"; Framework = "net8.0";  Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Server_Net8" }
        @{ Template = "blazorbit-server"; Framework = "net8.0";  Localization = $true;  Charts = $false; Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Server_Net8_Loc" }
        @{ Template = "blazorbit-server"; Framework = "net8.0";  Localization = $true;  Charts = $true;  Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Server_Net8_Loc_Charts" }
        @{ Template = "blazorbit-server"; Framework = "net8.0";  Localization = $true;  Charts = $true;  Notifications = $true;  HotKeys = $false; Theme = "None"; Name = "Server_Net8_Loc_Charts_Not" }
        @{ Template = "blazorbit-server"; Framework = "net8.0";  Localization = $true;  Charts = $true;  Notifications = $true;  HotKeys = $true;  Theme = "None"; Name = "Server_Net8_Loc_Charts_Not_Hot" }
        @{ Template = "blazorbit-wasm";   Framework = "net8.0";  Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Wasm_Net8" }
        @{ Template = "blazorbit-wasm";   Framework = "net8.0";  Localization = $true;  Charts = $false; Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Wasm_Net8_Loc" }
        @{ Template = "blazorbit-wasm";   Framework = "net8.0";  Localization = $true;  Charts = $true;  Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Wasm_Net8_Loc_Charts" }
        @{ Template = "blazorbit-wasm";   Framework = "net8.0";  Localization = $true;  Charts = $true;  Notifications = $true;  HotKeys = $false; Theme = "None"; Name = "Wasm_Net8_Loc_Charts_Not" }
        @{ Template = "blazorbit-wasm";   Framework = "net8.0";  Localization = $true;  Charts = $true;  Notifications = $true;  HotKeys = $true;  Theme = "None"; Name = "Wasm_Net8_Loc_Charts_Not_Hot" }
        @{ Template = "blazorbit-server"; Framework = "net10.0"; Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Server_Net10" }
        @{ Template = "blazorbit-server"; Framework = "net10.0"; Localization = $true;  Charts = $false; Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Server_Net10_Loc" }
        @{ Template = "blazorbit-server"; Framework = "net10.0"; Localization = $true;  Charts = $true;  Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Server_Net10_Loc_Charts" }
        @{ Template = "blazorbit-server"; Framework = "net10.0"; Localization = $true;  Charts = $true;  Notifications = $true;  HotKeys = $false; Theme = "None"; Name = "Server_Net10_Loc_Charts_Not" }
        @{ Template = "blazorbit-server"; Framework = "net10.0"; Localization = $true;  Charts = $true;  Notifications = $true;  HotKeys = $true;  Theme = "None"; Name = "Server_Net10_Loc_Charts_Not_Hot" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Wasm_Net10" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $true;  Charts = $false; Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Wasm_Net10_Loc" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $true;  Charts = $true;  Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Wasm_Net10_Loc_Charts" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $true;  Charts = $true;  Notifications = $true;  HotKeys = $false; Theme = "None"; Name = "Wasm_Net10_Loc_Charts_Not" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $true;  Charts = $true;  Notifications = $true;  HotKeys = $true;  Theme = "None"; Name = "Wasm_Net10_Loc_Charts_Not_Hot" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "None"; Name = "Wasm_Net10_Theme_None" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "NeoBrutalism"; Name = "Wasm_Net10_Theme_NeoBrutalism" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "BentoGrid"; Name = "Wasm_Net10_Theme_BentoGrid" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "Glassmorphism"; Name = "Wasm_Net10_Theme_Glassmorphism" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "FlatDesign"; Name = "Wasm_Net10_Theme_FlatDesign" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "MaterialDesign"; Name = "Wasm_Net10_Theme_MaterialDesign" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "Neomorphism"; Name = "Wasm_Net10_Theme_Neomorphism" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "Claymorphism"; Name = "Wasm_Net10_Theme_Claymorphism" }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $false; Charts = $false; Notifications = $false; HotKeys = $false; Theme = "RetroWeb"; Name = "Wasm_Net10_Theme_RetroWeb" }
    )
}

# ---------- Pretty output helpers ----------
function Write-Step($msg)    { Write-Host ""; Write-Host "==> $msg" -ForegroundColor Cyan }
function Write-Ok($msg)      { Write-Host "    [ok] $msg" -ForegroundColor Green }
function Write-Warn2($msg)   { Write-Host "    [warn] $msg" -ForegroundColor Yellow }
function Write-Err($msg)     { Write-Host "    [err] $msg" -ForegroundColor Red }

function Invoke-DotNet {
    param([string[]]$ArgsList, [string]$Description)
    Write-Host "    > dotnet $($ArgsList -join ' ')" -ForegroundColor DarkGray
    & dotnet @ArgsList
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($ArgsList -join ' ') failed (exit $LASTEXITCODE) — $Description"
    }
}

# ---------- Ensure feed directory + nuget source ----------
Write-Step "Preparing local feed at $FeedDir"
if (-not (Test-Path $FeedDir)) {
    New-Item -ItemType Directory -Path $FeedDir -Force | Out-Null
    Write-Ok "created"
} else {
    Write-Ok "exists"
}

$feedSourceName = "blazorbit-local-test"
$existingSource = & dotnet nuget list source 2>&1 | Select-String -Pattern $feedSourceName
if (-not $existingSource) {
    Invoke-DotNet @("nuget", "add", "source", $FeedDir, "-n", $feedSourceName) "register local feed"
    Write-Ok "registered NuGet source '$feedSourceName'"
} else {
    Write-Ok "NuGet source '$feedSourceName' already registered"
}

# ---------- 1. Pack BlazOrbit library (optional) ----------
if (-not $SkipBlazOrbitPack) {
    Write-Step "Cleaning BlazOrbit solution to avoid stale static-asset manifests"
    Invoke-DotNet @("clean", $mainSln, "-c", $Configuration, "--nologo") "clean solution"
    Write-Ok "cleaned"

    Write-Step "Packing BlazOrbit library to $FeedDir"
    Invoke-DotNet @("pack", $mainSln, "-c", $Configuration, "-o", $FeedDir, "--nologo") "pack BlazOrbit.slnx"
    Write-Ok "library packed"
} else {
    Write-Step "Skipping BlazOrbit pack (-SkipBlazOrbitPack)"
}

# ---------- 2. Pack templates ----------
Write-Step "Packing BlazOrbit.Templates"
Invoke-DotNet @("pack", $templatesProj, "-c", $Configuration, "-o", $FeedDir, "--nologo") "pack templates"
$templatePkg = Get-ChildItem $FeedDir -Filter "BlazOrbit.Templates.*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $templatePkg) { throw "BlazOrbit.Templates.*.nupkg not found in $FeedDir" }
Write-Ok "templates packed: $($templatePkg.Name)"

# ---------- 3. Clear http cache so the new pkg is picked up ----------
Write-Step "Clearing NuGet http cache"
Invoke-DotNet @("nuget", "locals", "http-cache", "--clear") "clear nuget cache"
Write-Ok "cleared"

# ---------- 4. Install templates ----------
Write-Step "Installing BlazOrbit.Templates"
$installed = & dotnet new uninstall 2>&1 | Out-String
if ($installed -match "BlazOrbit\.Templates") {
    Write-Warn2 "previous BlazOrbit.Templates install detected — uninstalling first"
    Invoke-DotNet @("new", "uninstall", "BlazOrbit.Templates") "uninstall previous"
}
Invoke-DotNet @("new", "install", $templatePkg.FullName, "--force") "install templates"
Write-Ok "installed"

# ---------- 5. Prep work dir ----------
Write-Step "Preparing work dir at $WorkDir"
if (Test-Path $WorkDir) {
    if ($KeepWorkDir) {
        Write-Warn2 "WorkDir exists and -KeepWorkDir set — leaving in place; per-test dirs will still be wiped"
    } else {
        Remove-Item $WorkDir -Recurse -Force
        Write-Ok "wiped previous work dir"
    }
}
New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null

# ---------- 6. Run matrix ----------
$results = New-Object System.Collections.Generic.List[object]
foreach ($case in $Matrix) {
    $template = $case.Template
    $framework = $case.Framework
    $loc = [bool]$case.Localization
    $charts = [bool]$case.Charts
    $notifications = [bool]$case.Notifications
    $hotkeys = [bool]$case.HotKeys
    $theme = $case.Theme
    $name = $case.Name
    if ([string]::IsNullOrWhiteSpace($name)) {
        $suffix = ($loc ? "Loc" : "NoLoc")
        if ($charts) { $suffix += "_Charts" }
        if ($notifications) { $suffix += "_Not" }
        if ($hotkeys) { $suffix += "_Hot" }
        $name = "{0}_{1}_{2}" -f $template, $framework, $suffix
    }
    $caseDir = Join-Path $WorkDir $name

    Write-Step ("Case: {0} | {1} | localization={2} | charts={3} | notifications={4} | hotkeys={5} → {6}" -f $template, $framework, $loc, $charts, $notifications, $hotkeys, $name)

    if (Test-Path $caseDir) { Remove-Item $caseDir -Recurse -Force }

    $newArgs = @(
        "new", $template,
        "-n", $name,
        "-o", $caseDir,
        "--Framework", $framework,
        "--IncludeLocalization", $loc.ToString().ToLower(),
        "--IncludeCharts", $charts.ToString().ToLower(),
        "--UseNotificationsCenter", $notifications.ToString().ToLower(),
        "--UseHotKeys", $hotkeys.ToString().ToLower(),
        "--Theme", $theme
    )

    $caseResult = [pscustomobject]@{
        Name          = $name
        Template      = $template
        Framework     = $framework
        Localization  = $loc
        Charts        = $charts
        Notifications = $notifications
        HotKeys       = $hotkeys
        Generate      = "skipped"
        Build         = "skipped"
        Error         = $null
    }

    try {
        Invoke-DotNet $newArgs "generate $name"
        $caseResult.Generate = "ok"
        Write-Ok "generated"
    } catch {
        $caseResult.Generate = "fail"
        $caseResult.Error = $_.Exception.Message
        Write-Err "generation failed: $($_.Exception.Message)"
        $results.Add($caseResult)
        continue
    }

    if ($SkipBuild) {
        Write-Warn2 "build skipped (-SkipBuild)"
        $results.Add($caseResult)
        continue
    }

    try {
        $csproj = Get-ChildItem $caseDir -Filter "*.csproj" -Recurse | Select-Object -First 1
        if (-not $csproj) { throw "no .csproj found under $caseDir" }
        Invoke-DotNet @("build", $csproj.FullName, "-c", $Configuration, "--nologo") "build $name"
        $caseResult.Build = "ok"
        Write-Ok "built"
    } catch {
        $caseResult.Build = "fail"
        $caseResult.Error = $_.Exception.Message
        Write-Err "build failed: $($_.Exception.Message)"
    }

    $results.Add($caseResult)
}

# ---------- 7. Aggregate slnx for manual exploration ----------
# Pair with -KeepWorkDir to open / build / debug the eight generated
# projects from a single solution: `dotnet build $WorkDir\template-tests.slnx`
# or open it in VS / Rider. Survives across runs as long as the work dir
# is not cleaned.
$slnxPath = Join-Path $WorkDir "template-tests.slnx"
Write-Step "Writing slnx → $slnxPath"
$slnxLines = New-Object System.Collections.Generic.List[string]
$slnxLines.Add("<Solution>") | Out-Null
$slnxIncluded = 0
foreach ($r in $results) {
    if ($r.Generate -ne "ok") { continue }
    $caseDir = Join-Path $WorkDir $r.Name
    $csproj = Get-ChildItem $caseDir -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $csproj) { continue }
    # slnx Path entries are relative to the slnx location and use forward
    # slashes for cross-platform compatibility.
    $rel = $csproj.FullName.Substring($WorkDir.Length).TrimStart('\', '/').Replace('\', '/')
    $slnxLines.Add("  <Project Path=`"$rel`" />") | Out-Null
    $slnxIncluded++
}
$slnxLines.Add("</Solution>") | Out-Null
($slnxLines -join "`n") | Set-Content -Path $slnxPath -Encoding UTF8
Write-Ok "slnx written ($slnxIncluded project(s))"

# ---------- 8. E2E tests (optional) ----------
$failures = $results | Where-Object { $_.Generate -ne "ok" -or ($_.Build -eq "fail") }
$e2eResults = "skipped"
if ($RunE2E) {
    if ($SkipBuild) {
        Write-Warn2 "Cannot run E2E tests with -SkipBuild. Projects must be built first."
    }
    elseif ($failures.Count -gt 0) {
        Write-Warn2 "Skipping E2E tests because some builds failed."
    }
    else {
        Write-Step "Running Playwright E2E tests"

        $e2eProj = Join-Path $repoRoot "test\BlazOrbit.Templates.E2E\BlazOrbit.Templates.E2E.csproj"
        if (-not (Test-Path $e2eProj)) {
            throw "E2E test project not found: $e2eProj"
        }

        # 1. Build E2E project (idempotent — no-op if already built)
        Invoke-DotNet @("build", $e2eProj, "-c", $Configuration, "--nologo") "build E2E project"

        # 2. Ensure Playwright browsers are installed (idempotent — skips if present)
        $playwrightPs1 = Join-Path $repoRoot "test\BlazOrbit.Templates.E2E\bin\$Configuration\net10.0\playwright.ps1"
        if (-not (Test-Path $playwrightPs1)) {
            throw "playwright.ps1 not found at $playwrightPs1. Ensure the E2E project built successfully."
        }

        Write-Host "    > pwsh $playwrightPs1 install chromium" -ForegroundColor DarkGray
        & pwsh $playwrightPs1 install chromium
        if ($LASTEXITCODE -ne 0) {
            throw "Playwright browser installation failed"
        }

        # 3. Pass the work directory to the fixture so it reuses generated projects
        $env:BLAZORBIT_TEMPLATE_TEST_DIR = $WorkDir

        try {
            Invoke-DotNet @("test", $e2eProj, "-c", $Configuration, "--no-build", "--verbosity", "normal", "--nologo") "run E2E tests"
            $e2eResults = "ok"
            Write-Ok "E2E tests passed"
        } catch {
            $e2eResults = "fail"
            Write-Err "E2E tests failed: $($_.Exception.Message)"
        }
    }
}

# ---------- 9. Summary ----------
Write-Step "Summary"
$results | Format-Table Name, Template, Framework, Localization, Charts, Notifications, HotKeys, Generate, Build -AutoSize | Out-String | Write-Host

if ($failures.Count -gt 0) {
    Write-Host ""
    Write-Host "Failures:" -ForegroundColor Red
    foreach ($f in $failures) {
        Write-Host ("  - {0}: generate={1}, build={2}" -f $f.Name, $f.Generate, $f.Build) -ForegroundColor Red
        if ($f.Error) { Write-Host ("      {0}" -f $f.Error) -ForegroundColor DarkRed }
    }
}

if ($RunE2E) {
    Write-Host ""
    Write-Host ("E2E tests: {0}" -f $e2eResults) -ForegroundColor $(if ($e2eResults -eq "ok") { "Green" } elseif ($e2eResults -eq "fail") { "Red" } else { "Yellow" })
}

# ---------- 10. Cleanup ----------
if (-not $KeepInstalled) {
    Write-Step "Uninstalling BlazOrbit.Templates"
    # The E2E fixture's DisposeAsync already calls `dotnet new uninstall`, so when
    # -RunE2E was used the templates may already be gone. Detect that and stay
    # quiet instead of warning.
    $installedNow = & dotnet new uninstall 2>&1 | Out-String
    if ($installedNow -match "BlazOrbit\.Templates") {
        try {
            Invoke-DotNet @("new", "uninstall", "BlazOrbit.Templates") "uninstall templates"
            Write-Ok "uninstalled"
        } catch {
            Write-Warn2 "uninstall failed (templates may have been installed from a different path): $($_.Exception.Message)"
        }
    } else {
        Write-Ok "already uninstalled"
    }
} else {
    Write-Step "Leaving BlazOrbit.Templates installed (-KeepInstalled)"
}

if (-not $KeepWorkDir) {
    Write-Step "Cleaning $WorkDir"
    Remove-Item $WorkDir -Recurse -Force
    Write-Ok "wiped"
} else {
    Write-Step "Leaving generated projects in $WorkDir (-KeepWorkDir)"
}

# Exit non-zero if templates failed OR E2E (when requested) failed.
$overallFail = $failures.Count -gt 0 -or ($RunE2E -and $e2eResults -eq "fail")
if ($overallFail) {
    Write-Host ""
    if ($failures.Count -gt 0) {
        Write-Host "FAIL: $($failures.Count) of $($results.Count) template cases failed." -ForegroundColor Red
    }
    if ($RunE2E -and $e2eResults -eq "fail") {
        Write-Host "FAIL: E2E tests failed. Diagnostics in $env:TEMP\blazorbit-e2e-logs" -ForegroundColor Red
    }
    exit 1
} else {
    Write-Host ""
    Write-Host "OK: all $($results.Count) cases passed." -ForegroundColor Green
    if ($RunE2E) { Write-Host "OK: E2E tests passed." -ForegroundColor Green }
    exit 0
}
