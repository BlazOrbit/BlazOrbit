<#
.SYNOPSIS
    Normaliza todos los archivos de texto del repo a UTF-8 sin BOM.

.DESCRIPTION
    Política del repo: UTF-8 sin BOM en TODAS las extensiones (`.cs`, `.razor`,
    `.json`, `.tn`, `.ps1`, `.md`, `.yml`, etc). Razones:
      - Unicode consortium recommendation: el BOM no es "ni requerido ni
        recomendado" en UTF-8 (existe para desambiguar endianness UTF-16/32;
        UTF-8 no tiene endianness).
      - Cross-platform: shebangs `#!/usr/bin/env pwsh|bash` no resuelven si
        byte 0 es BOM — `./script.ps1` falla ENOEXEC en Linux/macOS.
      - Runtime parsers byte-sensibles: `JSON.parse`, BlazOrbit `TnParser`,
        Node module loaders fallan o misparsean con BOM.
      - `.editorconfig` declara `charset = utf-8` global, así que IDEs (VS,
        Rider, VS Code) guardan sin BOM sin que cada contributor configure
        encoding a mano.

    Modo normalizar (default):
      Recorre archivos de texto conocidos, los lee con detección automática
      de encoding y los re-escribe en UTF-8 sin BOM. Strip de doble-BOM.

    Modo check (`-Check`):
      No toca disco. Audita cada archivo y reporta drift:
        - BOM presente (la política global es no-BOM).
        - Bytes no válidos como UTF-8 (Latin-1/CP1252 accidental, doble-encoding).
      Exit 1 si hay drift. Wire-in en CI (`preview-gate.yml`).

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
# Configuración
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
    '.tn', '.editorconfig'
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
Write-Host "Encoding $mode (UTF-8 without BOM, repo-wide policy)"
Write-Host "Root: $Root"
Write-Host "========================================`n"

$files = Get-ChildItem -Path $Root -Recurse -File | Where-Object {
    -not (ShouldExclude $_.FullName)
}

$fixed = 0
$skippedBinary = 0
$skippedNoChange = 0
$drifts = New-Object System.Collections.Generic.List[string]
# Explicit no-BOM encoder. DO NOT use [System.Text.Encoding]::UTF8 — that
# static instance has `encoderShouldEmitUTF8Identifier = true` and
# File.WriteAllText emits the BOM via its preamble, silently re-adding the
# very thing this script exists to remove.
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

foreach ($file in $files) {
    if (-not (IsTextFile $file)) { continue }
    if ($file.FullName -eq $PSCommandPath) { continue }

    try {
        $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
        if ($bytes -contains 0) { $skippedBinary++; continue }

        $hasBom = StartsWithBom $bytes
        $relPath = [System.IO.Path]::GetRelativePath($Root, $file.FullName)

        if ($Check) {
            # Gate scope: blocks anything that breaks tooling at runtime.
            #   1. BOM present (repo-wide no-BOM policy).
            #   2. Double BOM (corruption signal).
            #   3. Invalid UTF-8 byte sequences (mojibake, Latin-1/CP1252
            #      from editors with wrong defaults).
            # Empty / BOM-only files trivially valid — skip the payload
            # decode (PowerShell array-slice on len<=3 produces a reversed
            # range and the strict UTF-8 check throws on the resulting bytes).
            if ($bytes.Length -gt 3) {
                $payload = if ($hasBom) { $bytes[3..($bytes.Length - 1)] } else { $bytes }
                if (-not (IsValidUtf8 $payload)) {
                    $drifts.Add("$relPath  -> invalid UTF-8 byte sequence (re-save the file as UTF-8 in your editor)")
                    continue
                }
            }

            if ($hasBom -and $bytes.Length -ge 6 -and (StartsWithBom $bytes[3..5])) {
                $drifts.Add("$relPath  -> double BOM detected")
            } elseif ($hasBom) {
                $drifts.Add("$relPath  -> unexpected BOM (repo policy is UTF-8 without BOM)")
            } else {
                $skippedNoChange++
            }
            continue
        }

        # Normalize mode: re-write as UTF-8 without BOM.
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
        if ($PSCmdlet.ShouldProcess($file.FullName, "Normalize UTF-8 without BOM")) {
            [System.IO.File]::WriteAllText($file.FullName, $content, $utf8NoBom)
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
