<#
.SYNOPSIS
    Normaliza todos los archivos de texto del repo a UTF-8 con BOM.
.DESCRIPTION
    Recorre recursivamente los archivos de texto conocidos, los lee con
    detección automática de encoding y los re-escribe en UTF-8 con exactamente
    un BOM al inicio.

    Corrige archivos afectados por doble-BOM (EF BB BF EF BB BF) dejando
    un único BOM válido.

    NO toca:
      - .git/, bin/, obj/, node_modules/, .vs/, artifacts/, coverage-out/
      - Archivos binarios (heurística de bytes nulos)

    Uso:
      .\scripts\fix-encoding-bom.ps1
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$Root = (Get-Location)
)

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
    '.verified.txt', '.received.txt', '.cshtml', '.wasm'
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
    if ($File.Name -like '*.min.js') { return $false }
    if ($File.Name -like '*.min.css') { return $false }
    if ($TextExtensions -contains $File.Extension.ToLowerInvariant()) { return $true }
    if ($File.Extension -eq '' -and ($File.Name -in @('Dockerfile', 'Makefile', 'LICENSE', 'NOTICE'))) { return $true }
    return $false
}

# Archivos que deben quedar SIN BOM porque herramientas Node.js/Vite los leen
# como bytes crudos (JSON.parse, require, etc.) y fallan con BOM inicial.
$NoBomExtensions = @('.json')

Write-Host "`n========================================"
Write-Host "Normalizando encoding a UTF-8 con BOM"
Write-Host "Root: $Root"
Write-Host "========================================`n"

$files = Get-ChildItem -Path $Root -Recurse -File | Where-Object {
    -not (ShouldExclude $_.FullName)
}

$fixed = 0
$skippedBinary = 0
$skippedNoChange = 0
$utf8WithBom = New-Object System.Text.UTF8Encoding($true)

foreach ($file in $files) {
    if (-not (IsTextFile $file)) { continue }
    if ($file.FullName -eq $PSCommandPath) { continue }

    try {
        $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
        if ($bytes -contains 0) { $skippedBinary++; continue }

        # Leer descartando cualquier BOM previo (simple o doble)
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)

        # Re-escribir: UTF-8 con BOM para la mayoría, SIN BOM para JSON/Node
        $useBom = $NoBomExtensions -notcontains $file.Extension.ToLowerInvariant()
        $encoding = if ($useBom) { $utf8WithBom } else { [System.Text.Encoding]::UTF8 }
        if ($PSCmdlet.ShouldProcess($file.FullName, "Normalizar encoding UTF-8$(if($useBom){' con BOM'}else{' sin BOM'})")) {
            [System.IO.File]::WriteAllText($file.FullName, $content, $encoding)
        }
        $fixed++
    }
    catch {
        Write-Warning "No se pudo procesar $($file.FullName): $_"
    }
}

Write-Host "`n  -> $fixed archivos normalizados."
Write-Host "  -> $skippedBinary archivos binarios omitidos."
Write-Host "`n========================================"
Write-Host "Done."
Write-Host "========================================"
