#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Genera el sitemap.xml del sitio de documentación a partir de las directivas @page.

.DESCRIPTION
    Escanea los archivos .razor bajo docs/BlazOrbit.Docs.Wasm/Pages/, extrae las
    rutas declaradas con @page y escribe un sitemap.xml válido en wwwroot/.

    Excluye automáticamente:
      - Rutas con parámetros dinámicos ({...} o catch-all *).
      - Páginas de error (por defecto /not-found).
      - Páginas de utilidad tipo search (por defecto /search).

    Asigna <priority> y <changefreq> mediante reglas heurísticas basadas en la
    profundidad y el segmento de la ruta.

.EXAMPLE
    ./scripts/generate-sitemap.ps1
    Genera docs/BlazOrbit.Docs.Wasm/wwwroot/sitemap.xml con la URL base
    por defecto https://blazorbit.com.

.EXAMPLE
    ./scripts/generate-sitemap.ps1 -BaseUrl "https://staging.blazorbit.com"
    Usa una URL base diferente.

.NOTES
    Author: Samuel Maícas (@cdcsharp)
    Version: 1.0.0
#>

[CmdletBinding()]
param(
    [string]$BaseUrl = 'https://blazorbit.com',

    [string]$PagesPath = 'docs/BlazOrbit.Docs.Wasm/Pages',

    [string]$OutputPath = 'docs/BlazOrbit.Docs.Wasm/wwwroot/sitemap.xml',

    [string[]]$ExcludeRoutes = @('/not-found', '/search'),

    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# 1. Normalizar URL base (sin trailing slash)
# ---------------------------------------------------------------------------
$BaseUrl = $BaseUrl.TrimEnd('/')

# ---------------------------------------------------------------------------
# 2. Recolectar rutas desde @page
# ---------------------------------------------------------------------------
$razorFiles = Get-ChildItem -Path $PagesPath -Filter '*.razor' -Recurse -File

$routes = [System.Collections.Generic.List[string]]::new()

foreach ($file in $razorFiles) {
    $firstLines = Get-Content -Path $file.FullName -TotalCount 10
    $pageLine = $firstLines | Where-Object { $_ -match '^\s*@page\s+"(.+)"' } | Select-Object -First 1

    if (-not $pageLine) { continue }

    $route = $matches[1].Trim()

    # Saltar rutas con parámetros dinámicos o catch-all
    if ($route -match '\{|\*') {
        Write-Verbose "Saltando ruta con parámetro: $route ($($file.Name))"
        continue
    }

    # Saltar rutas en lista de exclusión
    if ($ExcludeRoutes -contains $route) {
        Write-Verbose "Saltando ruta excluida: $route ($($file.Name))"
        continue
    }

    $routes.Add($route)
}

# Ordenar alfabéticamente; '/' siempre primero
$sortedRoutes = $routes | Sort-Object -Property {
    if ($_ -eq '/') { '' } else { $_ }
}

Write-Host "Rutas encontradas: $($sortedRoutes.Count)"

# ---------------------------------------------------------------------------
# 3. Heurística de prioridad y changefreq
# ---------------------------------------------------------------------------
function Get-PriorityAndFreq {
    param([string]$route)

    $segments = ($route -split '/').Where({ $_ -ne '' })
    $depth = $segments.Count

    # Reglas específicas por segmento raíz
    switch ($segments[0]) {
        'privacy'      { return @{ Priority = '0.3'; Freq = 'monthly' } }
        'utils'        { return @{ Priority = '0.5'; Freq = 'monthly' } }
        'features'     { return @{ Priority = '0.6'; Freq = 'monthly' } }
        'concepts'     { return @{ Priority = '0.7'; Freq = 'monthly' } }
        'live-development' { return @{ Priority = '0.5'; Freq = 'monthly' } }
    }

    # Todo lo que vive bajo /components/* es documentación de componentes
    if ($segments[0] -eq 'components') {
        return @{ Priority = '0.8'; Freq = 'weekly' }
    }

    # Reglas por profundidad (resto)
    if ($depth -eq 0) {
        return @{ Priority = '1.0'; Freq = 'weekly' }
    }
    elseif ($depth -eq 1) {
        return @{ Priority = '0.9'; Freq = 'weekly' }
    }
    else {
        return @{ Priority = '0.7'; Freq = 'weekly' }
    }
}

# ---------------------------------------------------------------------------
# 4. Construir XML
# ---------------------------------------------------------------------------
$xmlNs = 'http://www.sitemaps.org/schemas/sitemap/0.9'

# Construir el XML manualmente como string para controlar la declaración
$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add('<?xml version="1.0" encoding="UTF-8"?>')
$lines.Add('<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">')

foreach ($route in $sortedRoutes) {
    $meta = Get-PriorityAndFreq -route $route
    $lines.Add('  <url>')
    $lines.Add("    <loc>$([System.Security.SecurityElement]::Escape("$BaseUrl$route"))</loc>")
    $lines.Add("    <changefreq>$($meta.Freq)</changefreq>")
    $lines.Add("    <priority>$($meta.Priority)</priority>")
    $lines.Add('  </url>')
}

$lines.Add('</urlset>')

$sitemapContent = ($lines -join "`r`n") + "`r`n"

# ---------------------------------------------------------------------------
# 5. Guardar (o mostrar en DryRun)
# ---------------------------------------------------------------------------
if ($DryRun) {
    Write-Host "`n--- DRY RUN ---`n"
    Write-Host $sitemapContent
}
else {
    $outputDir = Split-Path -Parent $OutputPath
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    }

    [System.IO.File]::WriteAllText($OutputPath, $sitemapContent, [System.Text.UTF8Encoding]::new($false))
    Write-Host "Sitemap generado: $OutputPath ($($sortedRoutes.Count) URLs)"
}
