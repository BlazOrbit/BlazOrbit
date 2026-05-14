<#
.SYNOPSIS
    Normaliza todos los archivos de texto del repo a UTF-8 (con BOM por default;
    sin BOM para extensiones declaradas en $NoBomExtensions).

.DESCRIPTION
    Modo normalizar (default):
      Recorre los archivos de texto conocidos, los lee como UTF-8 y los re-escribe
      con el BOM apropiado para su extensión. Corrige doble-BOM dejando uno solo.

    Modo check (`-Check`):
      No toca disco. Audita cada archivo y reporta drift:
        - BOM presente cuando debería ser no-BOM (.json, .tn).
        - BOM ausente cuando debería tenerlo (resto).
        - Bytes no válidos como UTF-8 (Latin-1/CP1252 accidental, doble-encoding).
      Exit 1 si hay drift. Pensado para wire-in en CI (`preview-gate.yml`).

    NO toca:
      - .git/, bin/, obj/, node_modules/, .vs/, artifacts/, coverage-out/
      - Archivos binarios (heurística de bytes nulos)

    Uso:
      pwsh ./scripts/fix-encoding-bom.ps1            # normaliza (muta)
      pwsh ./scripts/fix-encoding-bom.ps1 -Check     # audita (exit 1 si drift)
      pwsh ./scripts/fix-encoding-bom.ps1 -WhatIf    # dry-run del modo normalizar
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$Root = (Get-Location),
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Configuración (mismas exclusiones/extensiones que rename-to-blazorbit-org.ps1)
# ---------------------------------------------------------------------------
$ExcludeDirs = @(
    '.git', 'node_modules', 'bin', 'obj', '.vs',
    'artifacts', 'coverage-out', 'TestResults',
    'packages', '.vscode', '.idea'
)

$TextExtensions = @(
    '.cs', '.csproj', '.razor', '.razor.css', '.slnx', '.sln',
    '.md', '.props', '.targets', '.yml', '.yaml', '.ps1', '.txt',
    '.resx', '.js', '.ts', '.json', '.html', '.css', '.xml',
    '.config', '.runsettings', '.gitignore', '.gitattributes',
    '.sh', '.bash', '.zsh', '.cmd', '.bat',
    '.verified.txt', '.received.txt', '.cshtml', '.wasm',
    '.tn'
)

function ShouldExclude([string]$Path) {
    $normalized = $Path.Replace('/', '\').ToLowerInvariant()
    foreach ($ed in $ExcludeDirs) {
        $pattern = "\$ed\"
        if ($normalized -like "*$pattern*") { return $true }
        if ($normalized -like "*\$ed") { return $true }
    }
    return $false
}

function IsTextFile([System.IO.FileInfo]$File) {
    if ($File.Name -like '*.min.css') { return $false }
    if ($TextExtensions -contains $File.Extension.ToLowerInvariant()) { return $true }
    if ($File.Extension -eq '' -and ($File.Name -in @('Dockerfile', 'Makefile', 'LICENSE', 'NOTICE'))) { return $true }
    return $false
}

# Files that must stay no-BOM:
#   .json — some tools read JSON as raw bytes (JSON.parse, require, etc.) and
#           choke on a leading BOM.
#   .tn   — BlazOrbit BOBLocalize translation files. The runtime TnParser does
#           rawLine.TrimStart() + dispatches on `trimmed[0] == '@' | '#'`.
#           U+FEFF is Unicode category `Format`, not `WhiteSpace`, so TrimStart()
#           does NOT strip it. If a `.tn` file is loaded via the explicit
#           `File.ReadAllText(path, Encoding.UTF8)` overload, the BOM survives
#           into the first line ("﻿@es") and the culture header check
#           fails silently — the entire file mis-parses with no diagnostic.
#           (The Roslyn compile-time path strips BOM transparently, so the
#           source generator is unaffected; the no-BOM rule guards runtime
#           providers and any hand-written loader.)
$NoBomExtensions = @('.json', '.tn')

$Bom = [byte[]](0xEF, 0xBB, 0xBF)

function StartsWithBom([byte[]]$Bytes) {
    return $Bytes.Length -ge 3 -and $Bytes[0] -eq $Bom[0] -and $Bytes[1] -eq $Bom[1] -and $Bytes[2] -eq $Bom[2]
}

function IsValidUtf8([byte[]]$Bytes) {
    # Strict UTF-8 decode: throws DecoderFallbackException on any invalid byte sequence.
    $strict = New-Object System.Text.UTF8Encoding($false, $true)
    try {
        [void]$strict.GetString($Bytes)
        return $true
    } catch [System.Text.DecoderFallbackException] {
        return $false
    }
}

$mode = if ($Check) { 'Check' } else { 'Normalize' }
Write-Host "`n========================================"
Write-Host "Encoding $mode (UTF-8 with/without BOM per extension)"
Write-Host "Root: $Root"
Write-Host "========================================`n"

$files = Get-ChildItem -Path $Root -Recurse -File | Where-Object {
    -not (ShouldExclude $_.FullName)
}

$fixed = 0
$skippedBinary = 0
$skippedNoChange = 0
$drifts = New-Object System.Collections.Generic.List[string]
$utf8WithBom = New-Object System.Text.UTF8Encoding($true)

foreach ($file in $files) {
    if (-not (IsTextFile $file)) { continue }
    if ($file.FullName -eq $PSCommandPath) { continue }

    try {
        $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
        if ($bytes -contains 0) { $skippedBinary++; continue }

        $ext = $file.Extension.ToLowerInvariant()
        $shouldHaveBom = $NoBomExtensions -notcontains $ext
        $hasBom = StartsWithBom $bytes
        $relPath = [System.IO.Path]::GetRelativePath($Root, $file.FullName)

        if ($Check) {
            # Gate scope (intentional narrow): we only block what has a real
            # technical consequence. Adding/removing BOM in `.cs` / `.razor`
            # is cosmetic — Roslyn handles both, and the repo has no
            # editorconfig/gitattributes `charset` directive that would
            # justify enforcing one over the other. If the maintainer ever
            # wants a project-wide normalization, run this script without
            # `-Check` (mutates) and commit. Until then, the gate only fails on:
            #
            #   1. Invalid UTF-8 byte sequences (mojibake, accidental Latin-1
            #      / CP1252 from editors with wrong defaults).
            #   2. BOM present on extensions where it breaks tooling
            #      (`.json` — Node JSON.parse / `.tn` — runtime TnParser).
            #   3. Double BOM (EF BB BF EF BB BF) — corruption signal.
            # Empty or BOM-only files are trivially valid — skip the payload
            # decode (PowerShell array-slice on len<=3 produces a reversed
            # range and the strict UTF-8 check throws on the resulting bytes).
            if ($bytes.Length -gt 3) {
                $payload = if ($hasBom) { $bytes[3..($bytes.Length - 1)] } else { $bytes }
                if (-not (IsValidUtf8 $payload)) {
                    $drifts.Add("$relPath  -> invalid UTF-8 byte sequence (re-save the file as UTF-8 in your editor)")
                    continue
                }
            }

            if (-not $shouldHaveBom -and $hasBom) {
                $drifts.Add("$relPath  -> unexpected BOM (extension '$ext' must be UTF-8 without BOM)")
            } elseif ($hasBom -and $bytes.Length -ge 6 -and (StartsWithBom $bytes[3..5])) {
                $drifts.Add("$relPath  -> double BOM detected")
            } else {
                $skippedNoChange++
            }
            continue
        }

        # Normalize mode: re-write with the canonical BOM/no-BOM encoding.
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
        $encoding = if ($shouldHaveBom) { $utf8WithBom } else { [System.Text.Encoding]::UTF8 }
        if ($PSCmdlet.ShouldProcess($file.FullName, "Normalize UTF-8$(if($shouldHaveBom){' with BOM'}else{' without BOM'})")) {
            [System.IO.File]::WriteAllText($file.FullName, $content, $encoding)
        }
        $fixed++
    }
    catch {
        Write-Warning "No se pudo procesar $($file.FullName): $_"
    }
}

Write-Host ""
if ($Check) {
    Write-Host "  -> $skippedNoChange archivos OK"
    Write-Host "  -> $($drifts.Count) archivos con drift"
    Write-Host "  -> $skippedBinary archivos binarios omitidos"
    if ($drifts.Count -gt 0) {
        Write-Host ""
        Write-Host "Drift detected:" -ForegroundColor Yellow
        foreach ($d in $drifts) {
            Write-Host "  $d" -ForegroundColor Yellow
        }
        Write-Host ""
        Write-Host "Fix locally with: pwsh ./scripts/fix-encoding-bom.ps1" -ForegroundColor Yellow
        Write-Host "========================================"
        exit 1
    }
} else {
    Write-Host "  -> $fixed archivos normalizados."
    Write-Host "  -> $skippedBinary archivos binarios omitidos."
}
Write-Host "========================================"
Write-Host "Done."
Write-Host "========================================"
