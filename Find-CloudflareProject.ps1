$apiToken = Read-Host -Prompt 'API Token' -AsSecureString
$plainToken = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($apiToken))

$headers = @{
    'Authorization' = "Bearer $plainToken"
    'Content-Type'  = 'application/json'
}

try {
    $accounts = Invoke-RestMethod -Uri 'https://api.cloudflare.com/client/v4/accounts' -Headers $headers -Method Get
    if (-not $accounts.success) {
        Write-Host 'Error listando cuentas' -ForegroundColor Red
        exit
    }

    foreach ($acc in $accounts.result) {
        Write-Host "
===========================================" -ForegroundColor DarkGray
        Write-Host "Account: $(.name)" -ForegroundColor Yellow
        Write-Host "ID:      $(.id)" -ForegroundColor DarkYellow

        try {
            $projects = Invoke-RestMethod -Uri "https://api.cloudflare.com/client/v4/accounts/$($acc.id)/pages/projects" -Headers $headers -Method Get
            if ($projects.success -and $projects.result.Count -gt 0) {
                Write-Host 'Pages projects:' -ForegroundColor Green
                $projects.result | ForEach-Object { Write-Host "   - $(.name)" -ForegroundColor Cyan }
            } else {
                Write-Host 'Sin proyectos de Pages' -ForegroundColor DarkGray
            }
        } catch {
            Write-Host 'No se pudo leer proyectos de esta cuenta' -ForegroundColor Red
        }
    }
} catch {
    Write-Host "Error: $($apiToken = Read-Host -Prompt 'API Token' -AsSecureString
$plainToken = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($apiToken))

$headers = @{
    'Authorization' = "Bearer $plainToken"
    'Content-Type'  = 'application/json'
}

try {
    $accounts = Invoke-RestMethod -Uri 'https://api.cloudflare.com/client/v4/accounts' -Headers $headers -Method Get
    if (-not $accounts.success) {
        Write-Host 'Error listando cuentas' -ForegroundColor Red
        exit
    }

    foreach ($acc in $accounts.result) {
        Write-Host "
===========================================" -ForegroundColor DarkGray
        Write-Host "Account: $(.name)" -ForegroundColor Yellow
        Write-Host "ID:      $(.id)" -ForegroundColor DarkYellow

        try {
            $projects = Invoke-RestMethod -Uri "https://api.cloudflare.com/client/v4/accounts/$($acc.id)/pages/projects" -Headers $headers -Method Get
            if ($projects.success -and $projects.result.Count -gt 0) {
                Write-Host 'Pages projects:' -ForegroundColor Green
                $projects.result | ForEach-Object { Write-Host "   - $(.name)" -ForegroundColor Cyan }
            } else {
                Write-Host 'Sin proyectos de Pages' -ForegroundColor DarkGray
            }
        } catch {
            Write-Host 'No se pudo leer proyectos de esta cuenta' -ForegroundColor Red
        }
    }
} catch {
    Write-Host "Error: $(.Exception.Message)" -ForegroundColor Red
}
.Exception.Message)" -ForegroundColor Red
}
