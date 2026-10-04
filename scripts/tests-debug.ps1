#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Run all tests in Debug configuration.

.DESCRIPTION
    Executes dotnet test (Microsoft.Testing.Platform runner, see global.json)
    against the solution (BlazOrbit.slnx) using the Debug build configuration.
    Supports optional test filtering and code coverage.

.EXAMPLE
    ./tests-debug.ps1
    Run all tests in Debug.

.EXAMPLE
    ./tests-debug.ps1 -Filter "*Button*"
    Run only tests whose display name (fully qualified method name) matches the
    pattern. Wildcard '*' is supported at the beginning and/or end.

.EXAMPLE
    ./tests-debug.ps1 -Coverage
    Run all tests in Debug and collect code coverage (Cobertura) with coverlet.
    Reports land in TestResults/ at the repository root.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Filter,

    [Parameter()]
    [switch]$Coverage
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Colors = @{
    Success = "Green"
    Error   = "Red"
    Info    = "Cyan"
    Warning = "Yellow"
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Solution = Join-Path $RepoRoot "BlazOrbit.slnx"
$ResultsDir = Join-Path $RepoRoot "TestResults"

# Coverage scope: every shipped library plus the build-time generator assemblies.
$CoverageArguments = @(
    "--coverlet",
    "--coverlet-output-format", "cobertura",
    "--results-directory", $ResultsDir,
    "--coverlet-include",
        "[BlazOrbit]*", "[BlazOrbit.Core]*", "[BlazOrbit.CodeBlock]*",
        "[BlazOrbit.CodeGeneration]*", "[BlazOrbit.Core.CodeGeneration]*", "[BlazOrbit.Docs.CodeGeneration]*",
        "[BlazOrbit.SyntaxHighlight]*",
        "[BlazOrbit.Localization.Server]*", "[BlazOrbit.Localization.Shared]*", "[BlazOrbit.Localization.Wasm]*",
        "[BlazOrbit.Charts]*", "[BlazOrbit.Hotkeys]*", "[BlazOrbit.Notifications]*", "[BlazOrbit.FormsFluentValidation]*",
    "--coverlet-exclude-by-file",
        "**/*.g.cs", "**/*.designer.cs", "**/*.razor.g.cs", "**/Migrations/*.cs",
    "--coverlet-exclude-by-attribute",
        "Obsolete", "GeneratedCodeAttribute", "CompilerGeneratedAttribute"
)

$Arguments = @("test", "--solution", $Solution, "--configuration", "Debug")

if ($Coverage) {
    $Arguments += $CoverageArguments
}

if ($Filter) {
    # Exit code 8 = zero tests ran; expected for test projects with no match.
    $Arguments += @("--filter-display-name", $Filter, "--ignore-exit-code", "8")
}

Write-Host "`n=== Running tests (Debug) ===" -ForegroundColor $Colors.Info
if ($Filter) { Write-Host "Filter: $Filter" -ForegroundColor $Colors.Warning }
if ($Coverage) { Write-Host "Coverage: enabled (cobertura -> $ResultsDir)" -ForegroundColor $Colors.Warning }
Write-Host ""

# dotnet test picks the Microsoft.Testing.Platform runner from the repository
# global.json, which is resolved from the working directory.
Push-Location $RepoRoot
try {
    & dotnet @Arguments
} finally {
    Pop-Location
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n❌ Tests failed (exit code: $LASTEXITCODE)" -ForegroundColor $Colors.Error
    exit $LASTEXITCODE
}

Write-Host "`n✅ All tests passed (Debug)" -ForegroundColor $Colors.Success
