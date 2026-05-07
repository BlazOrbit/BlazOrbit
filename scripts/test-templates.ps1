#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    End-to-end smoke test for the BlazOrbit dotnet templates.

.DESCRIPTION
    Packs the BlazOrbit library + the templates package into a local NuGet feed,
    installs the templates with `dotnet new install`, generates a matrix of
    sample projects (server / wasm × net8 / net10 × +/- localization), builds
    each, and uninstalls the templates afterwards.

    The local feed lets the generated projects resolve `BlazOrbit 1.0.*` against
    the just-built bits instead of the public NuGet, so this script is the only
    way to validate template changes against unreleased library changes.

    Default matrix (4 combos):
        | template          | framework | localization |
        |-------------------|-----------|--------------|
        | blazorbit-server  | net8.0    | false        |
        | blazorbit-server  | net10.0   | true         |
        | blazorbit-wasm    | net8.0    | true         |
        | blazorbit-wasm    | net10.0   | false        |

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
    `Template`, `Framework`, `Localization`, `Name`.

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
if (-not $Matrix -or $Matrix.Count -eq 0) {
    $Matrix = @(
        @{ Template = "blazorbit-server"; Framework = "net8.0";  Localization = $false; Name = "Server_Net8_NoLoc" }
        @{ Template = "blazorbit-server"; Framework = "net10.0"; Localization = $true;  Name = "Server_Net10_Loc"   }
        @{ Template = "blazorbit-wasm";   Framework = "net8.0";  Localization = $true;  Name = "Wasm_Net8_Loc"      }
        @{ Template = "blazorbit-wasm";   Framework = "net10.0"; Localization = $false; Name = "Wasm_Net10_NoLoc"   }
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
    $name = $case.Name
    if ([string]::IsNullOrWhiteSpace($name)) {
        $name = "{0}_{1}_{2}" -f $template, $framework, ($loc ? "Loc" : "NoLoc")
    }
    $caseDir = Join-Path $WorkDir $name

    Write-Step ("Case: {0} | {1} | localization={2} → {3}" -f $template, $framework, $loc, $name)

    if (Test-Path $caseDir) { Remove-Item $caseDir -Recurse -Force }

    $newArgs = @(
        "new", $template,
        "-n", $name,
        "-o", $caseDir,
        "--Framework", $framework,
        "--IncludeLocalization", $loc.ToString().ToLower()
    )

    $caseResult = [pscustomobject]@{
        Name         = $name
        Template     = $template
        Framework    = $framework
        Localization = $loc
        Generate     = "skipped"
        Build        = "skipped"
        Error        = $null
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

# ---------- 7. Summary ----------
Write-Step "Summary"
$results | Format-Table Name, Template, Framework, Localization, Generate, Build -AutoSize | Out-String | Write-Host

$failures = $results | Where-Object { $_.Generate -ne "ok" -or ($_.Build -eq "fail") }
if ($failures.Count -gt 0) {
    Write-Host ""
    Write-Host "Failures:" -ForegroundColor Red
    foreach ($f in $failures) {
        Write-Host ("  - {0}: generate={1}, build={2}" -f $f.Name, $f.Generate, $f.Build) -ForegroundColor Red
        if ($f.Error) { Write-Host ("      {0}" -f $f.Error) -ForegroundColor DarkRed }
    }
}

# ---------- 8. Cleanup ----------
if (-not $KeepInstalled) {
    Write-Step "Uninstalling BlazOrbit.Templates"
    try {
        Invoke-DotNet @("new", "uninstall", "BlazOrbit.Templates") "uninstall templates"
        Write-Ok "uninstalled"
    } catch {
        Write-Warn2 "uninstall failed (templates may have been installed from a different path): $($_.Exception.Message)"
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

if ($failures.Count -gt 0) {
    Write-Host ""
    Write-Host "FAIL: $($failures.Count) of $($results.Count) cases failed." -ForegroundColor Red
    exit 1
} else {
    Write-Host ""
    Write-Host "OK: all $($results.Count) cases passed." -ForegroundColor Green
    exit 0
}
