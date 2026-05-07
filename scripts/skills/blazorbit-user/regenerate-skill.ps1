<#
.SYNOPSIS
    Regenerates BlazOrbit user-skill reference files from compiled assemblies.

.DESCRIPTION
    Loads BlazOrbit.dll + BlazOrbit.Core.dll via reflection and emits
    components.md / variants.md / icons.md under references/. The handcrafted
    files (theming.md, presets.md, patterns.md, recipes.md) are NOT touched.

    Resolves the Microsoft.AspNetCore.App shared framework so reflection can
    bind ComponentBase / ParameterAttribute. Pass -SharedFrameworkPath to
    override autodiscovery.
#>
[CmdletBinding()]
param(
    [string]$Configuration = "Debug",
    [string]$Framework = "net8.0",
    [string]$ProjectRoot = "",
    [string]$SharedFrameworkPath = "",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

# --- Locate repo root ----------------------------------------------------
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $search = $PSScriptRoot
    while ($search -and -not (Test-Path (Join-Path $search "BlazOrbit.slnx"))) {
        $parent = Split-Path $search -Parent
        if ($parent -eq $search) { break }
        $search = $parent
    }
    if (Test-Path (Join-Path $search "BlazOrbit.slnx")) {
        $ProjectRoot = $search
    } else {
        throw "Cannot locate BlazOrbit.slnx. Run from inside the repo or pass -ProjectRoot."
    }
}

# --- Build (optional) ----------------------------------------------------
if (-not $SkipBuild) {
    Write-Host "Building solution..."
    Push-Location $ProjectRoot
    dotnet build "BlazOrbit.slnx" -c $Configuration --verbosity quiet | Out-Null
    Pop-Location
}

# --- Locate assemblies ---------------------------------------------------
$srcPath       = Join-Path $ProjectRoot "src"
$coreAssembly  = Join-Path $srcPath "BlazOrbit.Core\bin\$Configuration\$Framework\BlazOrbit.Core.dll"
$mainAssembly  = Join-Path $srcPath "BlazOrbit\bin\$Configuration\$Framework\BlazOrbit.dll"

if (-not (Test-Path $coreAssembly)) {
    $Framework = "net10.0"
    $coreAssembly  = Join-Path $srcPath "BlazOrbit.Core\bin\$Configuration\$Framework\BlazOrbit.Core.dll"
    $mainAssembly  = Join-Path $srcPath "BlazOrbit\bin\$Configuration\$Framework\BlazOrbit.dll"
}
if (-not (Test-Path $coreAssembly)) {
    throw "Assembly not found: $coreAssembly. Build the solution first."
}

# --- Locate shared framework (Microsoft.AspNetCore.App) ------------------
# The Razor class library does not copy framework dlls into bin/, so reflection
# needs to find ComponentBase / ParameterAttribute somewhere else.
function Resolve-AspNetCoreSharedPath {
    param([string]$RequestedTfm)

    $tfmTag = if ($RequestedTfm -match 'net(\d+)\.\d+') { $matches[1] } else { '' }

    # Parse `dotnet --list-runtimes`. Lines look like:
    #   Microsoft.AspNetCore.App 10.0.0 [C:\Program Files\dotnet\shared\...]
    $listing = & dotnet --list-runtimes 2>$null
    $best = $null
    foreach ($line in $listing) {
        if ($line -match '^Microsoft\.AspNetCore\.App\s+(\d+)\.(\d+)\.(\d+(?:-[\w.]+)?)\s+\[(.+)\]$') {
            $major = [int]$matches[1]
            if ($tfmTag -and $major.ToString() -ne $tfmTag) { continue }
            $version = "$($matches[1]).$($matches[2]).$($matches[3])"
            $folder = Join-Path $matches[4] $version
            if (Test-Path $folder) {
                if (-not $best -or [version]$best.Version -lt [version]"$($matches[1]).$($matches[2]).$($matches[3] -replace '-.*$','')") {
                    $best = [pscustomobject]@{ Path = $folder; Version = "$($matches[1]).$($matches[2]).$($matches[3] -replace '-.*$','')" }
                }
            }
        }
    }
    if ($best) { return $best.Path } else { return "" }
}

if ([string]::IsNullOrWhiteSpace($SharedFrameworkPath)) {
    $SharedFrameworkPath = Resolve-AspNetCoreSharedPath -RequestedTfm $Framework
}
if ([string]::IsNullOrWhiteSpace($SharedFrameworkPath) -or -not (Test-Path $SharedFrameworkPath)) {
    throw "Cannot locate Microsoft.AspNetCore.App shared framework. Pass -SharedFrameworkPath explicitly."
}
Write-Host "Shared framework: $SharedFrameworkPath"

# --- Output directory ----------------------------------------------------
# Skill content lives under `.agents/skills/<skill-name>/`; this script lives
# under `scripts/skills/<skill-name>/` so the path is computed from
# $ProjectRoot, not relative to $PSScriptRoot.
$skillName = Split-Path $PSScriptRoot -Leaf
$refPath   = Join-Path $ProjectRoot ".agents/skills/$skillName/references"
if (-not (Test-Path $refPath)) { New-Item -ItemType Directory -Path $refPath -Force | Out-Null }

# --- Run regenerator (.NET 10 file-based app) ---------------------------
# `dotnet run <file>.cs -- <args>` is supported on the .NET 10 SDK and avoids
# needing a separate csproj for the tool. The regenerator is a single .cs file
# with top-level statements that delegate to the `Regenerator` class.
$csFile = Join-Path $PSScriptRoot "Regenerator.cs"
& dotnet run $csFile -- $coreAssembly $mainAssembly $refPath $SharedFrameworkPath
if ($LASTEXITCODE -ne 0) { throw "Regenerator exited with code $LASTEXITCODE" }
