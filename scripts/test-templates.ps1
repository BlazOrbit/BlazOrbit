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

.PARAMETER SkipE2E
    Skip the Playwright end-to-end smoke against the generated apps. E2E runs
    by default after every successful build because that is the only stage that
    catches runtime regressions (asset 404s, JS errors, blank pages) — a
    template can compile clean yet still ship a broken home page if a referenced
    `_content/<pkg>/...` URL is missing from the BlazOrbit nupkg's static
    web asset manifest.

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
    Full run: pack everything, install, build all combos, run Playwright E2E,
    uninstall.

.EXAMPLE
    ./scripts/test-templates.ps1 -SkipBlazOrbitPack
    Faster iteration when only template content changed.

.EXAMPLE
    ./scripts/test-templates.ps1 -SkipBlazOrbitPack -SkipBuild -KeepWorkDir -KeepInstalled
    Just regenerate projects, keep them on disk and the templates installed.

.EXAMPLE
    ./scripts/test-templates.ps1 -SkipE2E
    Quick smoke: pack + build only. Skips Playwright (no browser spin-up).

.PARAMETER PublicFeed
    Validate the templates + libraries that are LIVE on nuget.org instead of
    packing locally. Implies -SkipBlazOrbitPack. The matrix is generated
    using `dotnet new install BlazOrbit.Templates::<Version>`, the workspace
    gets a `nuget.config` that pins resolution to `https://api.nuget.org/v3/index.json`
    (the local feed is bypassed even if registered globally), every generated
    csproj has `Version="*"` pinned to the resolved version (so prereleases
    are picked too — `*` only matches stable), and build + E2E run normally.
    Use this as a post-publish smoke against the bits real users will install.

.PARAMETER Version
    Specific package version to test under -PublicFeed (e.g. `1.0.0-preview.46`
    or `1.0.0`). Empty → auto-resolve the latest version from nuget.org
    according to -IncludePrerelease.

.PARAMETER IncludePrerelease
    Under -PublicFeed with no explicit -Version, pick the latest preview
    instead of the latest stable from nuget.org.

.EXAMPLE
    ./scripts/test-templates.ps1 -PublicFeed -IncludePrerelease
    Resolve the latest preview from nuget.org and run the full matrix
    against it. The canary for "did the preview ship cleanly?".

.EXAMPLE
    ./scripts/test-templates.ps1 -PublicFeed -Version 1.0.0-preview.46
    Pin a specific preview. Useful when chasing a regression report against
    a known shipped version without rebuilding locally.

.EXAMPLE
    ./scripts/test-templates.ps1 -PublicFeed
    Resolve the latest STABLE from nuget.org and run the full matrix.

.EXAMPLE
    ./scripts/test-templates.ps1 -Help
    Show this help message and exit.

.NOTES
    Idempotent: a previous install of `BlazOrbit.Templates` is uninstalled
    before reinstalling. Local feed source is added if missing, left in place
    so subsequent runs are faster.
#>
<#
.SAMPLE USAGE SCENARIOS
# Post-publish smoke contra el preview más reciente
  ./scripts/test-templates.ps1 -PublicFeed -IncludePrerelease

  ./scripts/test-templates.ps1 -PublicFeed -IncludePrerelease -SkipE2E -KeepWorkDir

  # Reproducir un bug reportado contra una versión concreta
  ./scripts/test-templates.ps1 -PublicFeed -Version 1.0.0-preview.46

  # Validar latest stable (release)
  ./scripts/test-templates.ps1 -PublicFeed

  # Combinable con flags existentes
  ./scripts/test-templates.ps1 -PublicFeed -IncludePrerelease -SkipE2E -KeepWorkDir
#>
[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$FeedDir = "",
    [string]$WorkDir = "",
    [switch]$SkipBlazOrbitPack,
    [switch]$SkipBuild,
    [switch]$SkipE2E,
    [switch]$KeepWorkDir,
    [switch]$KeepInstalled,
    [hashtable[]]$Matrix,
    [switch]$PublicFeed,
    [string]$Version = "",
    [switch]$IncludePrerelease,
    [Alias("h")]
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

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

function Get-LatestNuGetVersion {
    <#
    .SYNOPSIS
    Resolve the latest version of a package id on nuget.org via the v3
    flat-container index. Filters prereleases out unless -AllowPrerelease.

    .DESCRIPTION
    Hits `https://api.nuget.org/v3-flatcontainer/<id>/index.json` (lowercase
    id, per the v3 spec) and returns the max version. NuGet's flat-container
    returns versions sorted oldest-first; we filter and pick the last.
    Uses semver-aware comparison via [System.Management.Automation.SemanticVersion]
    when available (PowerShell 7+) to keep `1.0.0-preview.40` < `1.0.0-preview.41`
    < `1.0.0` ordering right.
    #>
    param(
        [Parameter(Mandatory)][string]$PackageId,
        [switch]$AllowPrerelease
    )

    $url = "https://api.nuget.org/v3-flatcontainer/$($PackageId.ToLowerInvariant())/index.json"
    try {
        $response = Invoke-RestMethod -Uri $url -ErrorAction Stop
    } catch {
        throw "Failed to query nuget.org for '$PackageId' at $url : $($_.Exception.Message)"
    }
    if (-not $response.versions -or $response.versions.Count -eq 0) {
        throw "nuget.org reports no versions for package '$PackageId'."
    }

    $candidates = $response.versions
    if (-not $AllowPrerelease) {
        $candidates = $candidates | Where-Object { $_ -notmatch '-' }
    }
    if (-not $candidates) {
        $kind = if ($AllowPrerelease) { "any" } else { "stable" }
        throw "nuget.org has no $kind version for '$PackageId'."
    }

    # Sort with semver-aware comparator. PowerShell's `[semver]` (System.Management.Automation.SemanticVersion)
    # treats `-preview.41` < `1.0.0`, which is what NuGet does at resolve time.
    $sorted = $candidates | Sort-Object -Property @{ Expression = {
        try { [System.Management.Automation.SemanticVersion]$_ } catch { [version]($_ -split '-')[0] }
    } }
    return ($sorted | Select-Object -Last 1)
}

function Set-CsprojPackageVersion {
    <#
    .SYNOPSIS
    Replace `Version="*"` with `Version="<pinned>"` on every
    `<PackageReference Include="BlazOrbit*">` in the given csproj.

    .DESCRIPTION
    Templates ship `Version="*"` so consumers can float to the latest stable
    from nuget.org. Under -PublicFeed we test a SPECIFIC version (often a
    preview), and `*` does not match prereleases. This helper pins every
    BlazOrbit.* PackageReference to the target version in-place.
    Non-BlazOrbit packages (Microsoft.AspNetCore.*, FluentValidation, …)
    keep their templated version spec untouched.
    #>
    param(
        [Parameter(Mandatory)][string]$CsprojPath,
        [Parameter(Mandatory)][string]$PinnedVersion
    )

    $content = Get-Content $CsprojPath -Raw
    $pattern = '<PackageReference Include="(BlazOrbit[^"]*)" Version="\*"'
    $replacement = '<PackageReference Include="$1" Version="' + $PinnedVersion + '"'
    $updated = [regex]::Replace($content, $pattern, $replacement)

    if ($updated -ne $content) {
        Set-Content -Path $CsprojPath -Value $updated -Encoding UTF8 -NoNewline
        return $true
    }
    return $false
}

# ---------- 0. PublicFeed resolution (when validating live nuget.org bits) ----------
# When -PublicFeed is set we override several knobs up-front:
#   - SkipBlazOrbitPack ← always true (we want the LIVE nupkg, not a fresh local pack).
#   - $Version resolves via nuget.org if not provided explicitly.
#   - A workspace nuget.config is written later (step 5) to pin restore to
#     api.nuget.org regardless of any local-feed source registered globally.
if ($PublicFeed) {
    $SkipBlazOrbitPack = $true

    if ([string]::IsNullOrWhiteSpace($Version)) {
        Write-Step "Resolving latest BlazOrbit version on nuget.org (prerelease=$IncludePrerelease)"
        $Version = Get-LatestNuGetVersion -PackageId "BlazOrbit" -AllowPrerelease:$IncludePrerelease
        Write-Ok "resolved: $Version"
    } else {
        Write-Step "Using explicit version: $Version"
    }
}

# ---------- Ensure feed directory + nuget source ----------
# Skipped under -PublicFeed: we don't need the local feed at all, and the
# workspace nuget.config written later forces resolution to api.nuget.org
# regardless of any source registered globally.
$feedSourceName = "blazorbit-local-test"
if (-not $PublicFeed) {
    Write-Step "Preparing local feed at $FeedDir"
    if (-not (Test-Path $FeedDir)) {
        New-Item -ItemType Directory -Path $FeedDir -Force | Out-Null
        Write-Ok "created"
    } else {
        Write-Ok "exists"
    }

    $existingSource = & dotnet nuget list source 2>&1 | Select-String -Pattern $feedSourceName
    if (-not $existingSource) {
        Invoke-DotNet @("nuget", "add", "source", $FeedDir, "-n", $feedSourceName) "register local feed"
        Write-Ok "registered NuGet source '$feedSourceName'"
    } else {
        Write-Ok "NuGet source '$feedSourceName' already registered"
    }
} else {
    Write-Step "Skipping local feed prep (-PublicFeed → resolving from nuget.org)"
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
    if ($PublicFeed) {
        Write-Step "Skipping BlazOrbit pack (-PublicFeed → using version $Version from nuget.org)"
    } else {
        Write-Step "Skipping BlazOrbit pack (-SkipBlazOrbitPack)"
    }
}

# ---------- 2. Pack templates ----------
# Under -PublicFeed we install BlazOrbit.Templates directly from nuget.org
# (no local pack). $templatePkg stays $null and the install step branches
# on $PublicFeed below.
$templatePkg = $null
if (-not $PublicFeed) {
    Write-Step "Packing BlazOrbit.Templates"
    Invoke-DotNet @("pack", $templatesProj, "-c", $Configuration, "-o", $FeedDir, "--nologo") "pack templates"
    $templatePkg = Get-ChildItem $FeedDir -Filter "BlazOrbit.Templates.*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $templatePkg) { throw "BlazOrbit.Templates.*.nupkg not found in $FeedDir" }
    Write-Ok "templates packed: $($templatePkg.Name)"
} else {
    Write-Step "Skipping BlazOrbit.Templates pack (-PublicFeed → installing $Version from nuget.org)"
}

# ---------- 3. Clear caches so the new pkgs are picked up ----------
Write-Step "Clearing NuGet caches"
$globalPackages = Join-Path $env:USERPROFILE ".nuget\packages"
$blazOrbitPackages = Get-ChildItem -Path $globalPackages -Directory -Filter "blazorbit*" -ErrorAction SilentlyContinue
if ($blazOrbitPackages) {
    foreach ($pkg in $blazOrbitPackages) {
        Remove-Item -Recurse -Force $pkg.FullName -ErrorAction SilentlyContinue
        Write-Ok "removed stale global cache: $($pkg.Name)"
    }
} else {
    Write-Ok "no stale BlazOrbit packages in global cache"
}
Invoke-DotNet @("nuget", "locals", "http-cache", "--clear") "clear nuget http cache"
Write-Ok "cleared"

# ---------- 4. Install templates ----------
Write-Step "Installing BlazOrbit.Templates"
$installed = & dotnet new uninstall 2>&1 | Out-String
if ($installed -match "BlazOrbit\.Templates") {
    Write-Warn2 "previous BlazOrbit.Templates install detected — uninstalling first"
    Invoke-DotNet @("new", "uninstall", "BlazOrbit.Templates") "uninstall previous"
}
if ($PublicFeed) {
    # `Package@Version` syntax pins the version fetched from configured
    # nuget sources (nuget.org). `Package::Version` works today but emits
    # a deprecation warning; `@` is the documented replacement.
    Invoke-DotNet @("new", "install", "BlazOrbit.Templates@$Version", "--force") "install templates from nuget.org"
} else {
    Invoke-DotNet @("new", "install", $templatePkg.FullName, "--force") "install templates"
}
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

# ---------- 5b. Workspace nuget.config when -PublicFeed ----------
# Forces every `dotnet restore` under $WorkDir to resolve packages only from
# nuget.org. `<clear/>` discards any source inherited from the user-level
# nuget.config (where the local-feed source typically lives), guaranteeing
# the matrix consumes the LIVE published bits rather than whatever was last
# packed locally.
if ($PublicFeed) {
    $workspaceNuGetConfig = Join-Path $WorkDir "nuget.config"
    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
"@ | Set-Content -Path $workspaceNuGetConfig -Encoding UTF8
    Write-Ok "wrote nuget.config pinning resolution to nuget.org → $workspaceNuGetConfig"
}

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

    # Pin BlazOrbit.* PackageReferences to the resolved version under -PublicFeed.
    # Templates ship `Version="*"` which only matches stable — without this pin,
    # `dotnet restore` against a preview would resolve to the latest *stable*
    # (or fail if none exists) instead of the version we're trying to validate.
    if ($PublicFeed) {
        $csproj = Get-ChildItem $caseDir -Filter "*.csproj" -Recurse | Select-Object -First 1
        if ($csproj) {
            $changed = Set-CsprojPackageVersion -CsprojPath $csproj.FullName -PinnedVersion $Version
            if ($changed) { Write-Ok "pinned BlazOrbit.* → $Version in $($csproj.Name)" }
        }
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

# ---------- 8. E2E tests (default on; -SkipE2E opts out) ----------
# E2E is the only stage that catches runtime regressions (asset 404s, blank
# home page) that `dotnet build` happily compiles through, so it runs every
# time unless explicitly skipped.
$failures = @($results | Where-Object { $_.Generate -ne "ok" -or ($_.Build -eq "fail") })
$e2eResults = "skipped"
$RunE2E = -not $SkipE2E
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
if ($PublicFeed) {
    Write-Host ("Source: nuget.org (PublicFeed mode, version $Version)") -ForegroundColor Cyan
} else {
    Write-Host ("Source: local feed ($FeedDir)") -ForegroundColor DarkGray
}
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
    # E2E ran (the default) the templates may already be gone. Detect that and
    # stay quiet instead of warning.
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
