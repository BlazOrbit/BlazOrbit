#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Run all tests in Release configuration.

.DESCRIPTION
    Executes dotnet test against the solution (BlazOrbit.slnx) using the Release
    build configuration. Supports optional test filtering and code coverage.

.EXAMPLE
    ./tests-release.ps1
    Run all tests in Release.

.EXAMPLE
    ./tests-release.ps1 -Filter "FullyQualifiedName~Button"
    Run only tests whose fully qualified name contains "Button".

.EXAMPLE
    ./tests-release.ps1 -Coverage
    Run all tests in Release and collect code coverage (Cobertura) using
    coverage.runsettings at the repository root.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Filter,

    [Parameter()]
    [switch]$Coverage
)

$ErrorActionPreference = "Stop"

$Colors = @{
    Success = "Green"
    Error   = "Red"
    Info    = "Cyan"
    Warning = "Yellow"
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Solution = Join-Path $RepoRoot "BlazOrbit.slnx"
$Settings = Join-Path $RepoRoot "coverage.runsettings"

$Arguments = @("test", $Solution, "--configuration", "Release")

if ($Coverage) {
    if (-not (Test-Path $Settings)) {
        Write-Host "❌ Coverage settings not found: $Settings" -ForegroundColor $Colors.Error
        exit 1
    }
    $Arguments += @("--settings", $Settings)
}

if ($Filter) {
    $Arguments += @("--filter", $Filter)
}

Write-Host "`n=== Running tests (Release) ===" -ForegroundColor $Colors.Info
if ($Filter) { Write-Host "Filter: $Filter" -ForegroundColor $Colors.Warning }
if ($Coverage) { Write-Host "Coverage: enabled ($Settings)" -ForegroundColor $Colors.Warning }
Write-Host ""

& dotnet @Arguments

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n❌ Tests failed (exit code: $LASTEXITCODE)" -ForegroundColor $Colors.Error
    exit $LASTEXITCODE
}

Write-Host "`n✅ All tests passed (Release)" -ForegroundColor $Colors.Success
