#!/usr/bin/env pwsh
#Requires -Version 5.1
<#
.SYNOPSIS
    Gestor de GitHub Actions self-hosted runners para VM Ubuntu (Hyper-V).
    Corre en el host Windows y controla la VM via SSH.

.USAGE
    .\runners.ps1 open     # Punto de entrada unico (configura SSH + sudo + REPL)
    .\runners.ps1 <cmd>    # Ejecuta un comando suelto sin sesion interactiva
    .\runners.ps1 help     # Muestra ayuda

.DESCRIPCION DE SESION INTERACTIVA
    'open' comprueba si hay configuracion previa; si no la hay, la solicita
    automaticamente (igual que 'login'). Despues configura el ssh-agent solo para
    esta sesion (sin tocarlo en Startup), guarda la contrasena sudo en memoria y
    entra en un bucle interactivo estilo diskpart: escribe directamente 'status',
    'start', 'install'... sin anteponer '.\runners.ps1'. Escribe 'exit' o 'close'
    para salir; el ssh-agent arrancado por nosotros se detiene automaticamente.

.CONFIG
    Guarda conexion en: runners.config.json (junto al script)
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet("login", "open", "close", "install", "status", "restart", "stop",
                 "start", "pause", "resume", "logs", "list", "shell", "remove", "update", "help")]
    [string]$Command = "help"
)

# ── Auto-bootstrap ────────────────────────────────────────────────────────────
# DEBE ir despues del bloque param(): en PowerShell, [CmdletBinding()]/param()
# tienen que ser la primera sentencia no-comentario; cualquier codigo ejecutable
# antes de ellos provoca "Unexpected attribute 'CmdletBinding'".
#
# 1. Elimina el ADS Zone.Identifier para que Windows no vuelva a mostrar el
#    aviso de seguridad. Solo surte efecto la primera vez.
if ($PSCommandPath) {
    Unblock-File -Path $PSCommandPath -ErrorAction SilentlyContinue
}
# 2. Si la politica del PROCESO es Restricted o AllSigned, la sube a Bypass
#    solo para esta instancia de PowerShell. No toca la configuracion global.
$_ep = Get-ExecutionPolicy -Scope Process
if ($_ep -in @([Microsoft.PowerShell.ExecutionPolicy]::Restricted,
               [Microsoft.PowerShell.ExecutionPolicy]::AllSigned)) {
    Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force
}
Remove-Variable _ep -ErrorAction SilentlyContinue

Set-StrictMode -Off
$ErrorActionPreference = "Continue"

# Rastrea si NOSOTROS arrancamos el ssh-agent para poder detenerlo al salir.
# Nunca se escribe en disco ni persiste entre ejecuciones del script.
$script:AgentStartedByUs       = $false
$script:AgentOriginalStartType = $null

#region ── Configuracion ────────────────────────────────────────────────────────

$ConfigFile = Join-Path $PSScriptRoot "runners.config.json"

# La contrasena de sudo se guarda SOLO en memoria como variable de entorno
# de la sesion PowerShell actual. Nunca se escribe en disco.
# Nombre de la variable: RUNNERS_SUDO_PASS (ya definida por 'open' si se uso)

function Get-RunnerConfig {
    if (-not (Test-Path $ConfigFile)) { return $null }
    try {
        $content = Get-Content $ConfigFile -Raw -ErrorAction Stop
        if ([string]::IsNullOrWhiteSpace($content)) { return $null }
        return $content | ConvertFrom-Json -AsHashtable
    } catch { return $null }
}

function Save-RunnerConfig {
    param($Config)
    if (-not $Config) { return }
    $Config | ConvertTo-Json -Depth 5 | Set-Content $ConfigFile -Encoding UTF8
}

function Get-SshBaseArgs {
    <#
    Devuelve array de argumentos SSH comunes.
    Si ssh-agent tiene claves cargadas, la autenticacion es automatica.
    BatchMode=no permite que ssh-agent responda sin TTY.
    #>
    param([hashtable]$Cfg)
    $portPart = if ($Cfg.Port -and [int]$Cfg.Port -ne 22) { @("-p", "$($Cfg.Port)") } else { @() }
    return $portPart + @(
        "-o", "ConnectTimeout=10",
        "-o", "StrictHostKeyChecking=accept-new",
        "-o", "BatchMode=no"
    )
}

function Get-RunnerServicePattern {
    param([hashtable]$Cfg)
    if ($Cfg.RepoUrl) {
        $ownerRepo = ($Cfg.RepoUrl -replace "https://github.com/", "") -replace "/", "-"
        return "actions.runner.$ownerRepo.*"
    }
    return "actions.runner.*"
}

function Get-RunnerServiceName {
    param([hashtable]$Cfg, [string]$RunnerName)
    if ($Cfg.RepoUrl) {
        $ownerRepo = ($Cfg.RepoUrl -replace "https://github.com/", "") -replace "/", "-"
        return "actions.runner.$ownerRepo.$RunnerName.service"
    }
    return "actions.runner.*.$RunnerName.service"
}

function Get-AllServiceNames {
    param([hashtable]$Cfg)
    if (-not $Cfg.RunnerCount -or $Cfg.RunnerCount -le 0) { return @() }
    return 1..[int]$Cfg.RunnerCount | ForEach-Object {
        Get-RunnerServiceName -Cfg $Cfg -RunnerName "$($Cfg.RunnerBaseName)-$_"
    }
}

function Test-SshAvailable {
    $ssh = Get-Command ssh -ErrorAction SilentlyContinue
    if (-not $ssh) {
        Write-Host "ERROR: 'ssh' no encontrado en PATH." -ForegroundColor Red
        Write-Host "Instala el cliente OpenSSH:" -ForegroundColor Yellow
        Write-Host "  Configuracion > Aplicaciones > Caracteristicas opcionales > OpenSSH Client" -ForegroundColor Gray
        return $false
    }
    return $true
}

#endregion

#region ── Funciones SSH ────────────────────────────────────────────────────────

function Invoke-Ssh {
    <#
    Ejecuta un comando remoto y devuelve {ExitCode, StdOut, StdErr}.
    Si hay sesion persistente la usa automaticamente (sin password).
    BatchMode=no permite que ssh-agent responda sin TTY.
    $Interactive = $true: no redirige stdio (para sudo interactivo o instalacion).
    #>
    param(
        [string]$RemoteCommand,
        [switch]$Interactive,
        [int]$TimeoutSec = 90
    )

    $cfg = Get-RunnerConfig
    if (-not $cfg) {
        Write-Host "No hay configuracion. Ejecuta: .\runners.ps1 open" -ForegroundColor Red
        return $null
    }

    $baseArgs  = Get-SshBaseArgs -Cfg $cfg
    $target    = "$($cfg.User)@$($cfg.IP)"
    $allArgs   = $baseArgs + @($target, $RemoteCommand)

    if ($Interactive) {
        # Con -t para TTY (sudo interactivo, instalacion, etc.)
        $allArgs = $baseArgs + @("-t", $target, $RemoteCommand)
        $psi = New-Object System.Diagnostics.ProcessStartInfo("ssh")
        $psi.Arguments        = $allArgs -join " "
        $psi.UseShellExecute  = $false
        $proc = [System.Diagnostics.Process]::Start($psi)
        $exited = $proc.WaitForExit($TimeoutSec * 1000)
        if (-not $exited) { try { $proc.Kill() } catch {} }
        return [pscustomobject]@{ ExitCode = $proc.ExitCode; StdOut = ""; StdErr = "" }
    }

    # Modo captura (stdout/stderr redirigidos)
    $psi = New-Object System.Diagnostics.ProcessStartInfo("ssh")
    $psi.Arguments                = $allArgs -join " "
    $psi.RedirectStandardOutput   = $true
    $psi.RedirectStandardError    = $true
    $psi.UseShellExecute          = $false
    $psi.StandardOutputEncoding   = [System.Text.Encoding]::UTF8
    $psi.StandardErrorEncoding    = [System.Text.Encoding]::UTF8

    $proc   = [System.Diagnostics.Process]::Start($psi)
    $exited = $proc.WaitForExit($TimeoutSec * 1000)

    if (-not $exited) {
        try { $proc.Kill() } catch {}
        Write-Host "TIMEOUT: El comando tardo mas de ${TimeoutSec}s." -ForegroundColor Red
        return $null
    }

    $stdout = $proc.StandardOutput.ReadToEnd()
    $stderr = $proc.StandardError.ReadToEnd()

    return [pscustomobject]@{ ExitCode = $proc.ExitCode; StdOut = $stdout; StdErr = $stderr }
}

function Invoke-SshSudo {
    <#
    Ejecuta un comando remoto como sudo.

    - Si RUNNERS_SUDO_PASS esta definido (sesion 'open' activa):
        usa printf para pasar la contrasena a sudo -S de forma robusta,
        tolerando caracteres especiales incluidos comillas simples y backslashes.
    - Si no: modo interactivo con -t, el usuario escribe la contrasena manualmente.

    IMPORTANTE: el comando se envuelve en bash -c con heredoc para evitar
    problemas de escape de comillas en comandos complejos.
    #>
    param(
        [string]$RemoteCommand,
        [int]$TimeoutSec = 120
    )

    $cfg = Get-RunnerConfig
    if (-not $cfg) { Write-Host "No hay configuracion." -ForegroundColor Red; return $null }

    $baseArgs = Get-SshBaseArgs -Cfg $cfg
    $target   = "$($cfg.User)@$($cfg.IP)"

    $hasSudoPass = -not [string]::IsNullOrEmpty($env:RUNNERS_SUDO_PASS)

    if ($hasSudoPass) {
        # Escapar comillas simples en el comando para embeber en bash -c '...'
        $escaped = $RemoteCommand -replace "'", "'\'''"
        # printf es mas robusto que echo para passwords con caracteres especiales.
        # \n al final es necesario para que sudo -S lea la linea completa.
        $fullCmd = "printf '%s\n' '$($env:RUNNERS_SUDO_PASS -replace "'","'\\''")' | sudo -S -p '' bash -c '$escaped' 2>&1"

        $psi = New-Object System.Diagnostics.ProcessStartInfo("ssh")
        $psi.Arguments              = ($baseArgs + @($target, $fullCmd)) -join " "
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError  = $true
        $psi.UseShellExecute        = $false
        $psi.StandardOutputEncoding = [System.Text.Encoding]::UTF8
        $psi.StandardErrorEncoding  = [System.Text.Encoding]::UTF8

        $proc   = [System.Diagnostics.Process]::Start($psi)
        $exited = $proc.WaitForExit($TimeoutSec * 1000)
        if (-not $exited) { try { $proc.Kill() } catch {} }

        $stdout = $proc.StandardOutput.ReadToEnd()
        $stderr = $proc.StandardError.ReadToEnd()
        return [pscustomobject]@{ ExitCode = $proc.ExitCode; StdOut = $stdout; StdErr = $stderr }
    }

    # Sin sesion open: interactivo. El usuario escribe la contrasena manualmente.
    Write-Host "(sudo) " -NoNewline -ForegroundColor DarkYellow
    $escaped = $RemoteCommand -replace "'", "'\'''"
    $psi = New-Object System.Diagnostics.ProcessStartInfo("ssh")
    $psi.Arguments       = ($baseArgs + @("-t", $target, "sudo bash -c '$escaped'")) -join " "
    $psi.UseShellExecute = $false
    $proc   = [System.Diagnostics.Process]::Start($psi)
    $exited = $proc.WaitForExit($TimeoutSec * 1000)
    if (-not $exited) { try { $proc.Kill() } catch {} }
    return [pscustomobject]@{ ExitCode = $proc.ExitCode; StdOut = ""; StdErr = "" }
}

function Invoke-SshSudoExpanded {
    <#
    Igual que Invoke-SshSudo pero expande correctamente los globs de systemctl.
    En lugar de pasar el glob como argumento, lista primero los servicios
    y luego ejecuta el comando sobre cada uno.
    #>
    param(
        [string]$SystemctlAction,   # stop, start, restart, etc.
        [hashtable]$Cfg
    )

    $services = Get-AllServiceNames -Cfg $Cfg
    if ($services.Count -eq 0) {
        Write-Host "No hay runners en la configuracion local." -ForegroundColor Yellow
        return
    }

    $svcList = ($services | ForEach-Object { "`"$_`"" }) -join " "
    Invoke-SshSudo -RemoteCommand "systemctl $SystemctlAction $svcList" -TimeoutSec 60 | Out-Null
}

function Send-FileToRemote {
    param([string]$Content, [string]$RemotePath)
    $cfg       = Get-RunnerConfig
    $baseArgs  = Get-SshBaseArgs -Cfg $cfg
    $target    = "$($cfg.User)@$($cfg.IP)"
    $allArgs   = ($baseArgs + @($target, "cat > $RemotePath")) -join " "

    $psi = New-Object System.Diagnostics.ProcessStartInfo("ssh")
    $psi.Arguments              = $allArgs
    $psi.RedirectStandardInput  = $true
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError  = $true
    $psi.UseShellExecute        = $false

    $proc = [System.Diagnostics.Process]::Start($psi)
    $proc.StandardInput.Write($Content)
    $proc.StandardInput.Close()
    $proc.WaitForExit(60000) | Out-Null

    if ($proc.ExitCode -ne 0) {
        $err = $proc.StandardError.ReadToEnd()
        Write-Host "Error transfiriendo archivo: $err" -ForegroundColor Red
        return $false
    }
    return $true
}

function Read-HostMasked {
    param([string]$Prompt)
    Write-Host "$Prompt " -NoNewline
    $secure = Read-Host -AsSecureString
    $ptr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { return [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
    finally { [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
}

#endregion

#region ── Comandos ─────────────────────────────────────────────────────────────

function Invoke-CmdLogin {
    <#
    Configura (o reconfigura) la conexion SSH a la VM.
    Puede llamarse tanto desde la CLI como desde dentro del REPL de 'open'.
    #>
    Write-Host "`n=== Configurar conexion SSH a la VM Ubuntu ===" -ForegroundColor Cyan

    if (-not (Test-SshAvailable)) { return }

    $ip = Read-Host "IP de la VM (ej: 192.168.1.50)"
    if ([string]::IsNullOrWhiteSpace($ip)) { Write-Host "IP requerida." -ForegroundColor Red; return }

    $portStr = Read-Host "Puerto SSH [22]"
    $port    = if ([string]::IsNullOrWhiteSpace($portStr)) { 22 } else { [int]$portStr }

    $user = Read-Host "Usuario admin de la VM (con privilegios sudo)"
    if ([string]::IsNullOrWhiteSpace($user)) { Write-Host "Usuario requerido." -ForegroundColor Red; return }

    $repoUrl = Read-Host "URL del repo u organizacion de GitHub (ej: https://github.com/Org/Repo)"
    if ([string]::IsNullOrWhiteSpace($repoUrl)) { Write-Host "URL requerida." -ForegroundColor Red; return }

    Write-Host "Validando conexion SSH a ${user}@${ip}:${port} ..." -ForegroundColor Yellow

    $portArg = if ($port -ne 22) { "-p $port" } else { "" }
    $psi = New-Object System.Diagnostics.ProcessStartInfo("ssh")
    $psi.Arguments              = "$portArg -o ConnectTimeout=10 -o StrictHostKeyChecking=accept-new ${user}@${ip} echo RUNNERS_SSH_OK"
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError  = $true
    $psi.UseShellExecute        = $false
    $proc = [System.Diagnostics.Process]::Start($psi)
    $proc.WaitForExit(15000) | Out-Null
    $stdout = $proc.StandardOutput.ReadToEnd()
    $stderr = $proc.StandardError.ReadToEnd()

    if ($stdout.Trim() -ne "RUNNERS_SSH_OK") {
        Write-Host "No se pudo conectar. Detalles:" -ForegroundColor Red
        if ($stderr) { Write-Host $stderr -ForegroundColor DarkRed }
        return
    }

    Write-Host "Conexion exitosa!" -ForegroundColor Green

    # Intentar configurar clave SSH sin password
    Invoke-SshKeySetup -Ip $ip -Port $port -User $user

    # Preservar RunnerBaseName y RunnerCount si ya existian
    $existing = Get-RunnerConfig
    Save-RunnerConfig -Config @{
        IP             = $ip
        Port           = $port
        User           = $user
        RepoUrl        = $repoUrl
        RunnerBaseName = if ($existing) { $existing.RunnerBaseName } else { $null }
        RunnerCount    = if ($existing) { $existing.RunnerCount }    else { $null }
    }
    Write-Host "Configuracion guardada en: $ConfigFile" -ForegroundColor Green
}

function Invoke-SshKeySetup {
    param([string]$Ip, [int]$Port, [string]$User)

    Write-Host "`n=== Configuracion de clave SSH sin password ===" -ForegroundColor Cyan

    $sshDir = Join-Path $env:USERPROFILE ".ssh"
    $pubKey = Get-ChildItem -Path "$sshDir\*" -Include "id_ed25519.pub","id_rsa.pub","id_ecdsa.pub" -ErrorAction SilentlyContinue | Select-Object -First 1

    if (-not $pubKey) {
        $gen = Read-Host "No se encontro clave SSH publica. Generar una nueva? [S/n]"
        if ($gen.ToLower() -ne "n") {
            if (-not (Test-Path $sshDir)) { New-Item -ItemType Directory -Path $sshDir | Out-Null }
            & ssh-keygen -t ed25519 -C "runners@vm" -f "$sshDir\id_ed25519" -N ""
            if ($LASTEXITCODE -eq 0) {
                $pubKey = Get-Item "$sshDir\id_ed25519.pub"
                Write-Host "Clave generada: $($pubKey.FullName)" -ForegroundColor Green
            } else {
                Write-Host "No se pudo generar la clave." -ForegroundColor Yellow
                return $false
            }
        } else {
            return $false
        }
    } else {
        Write-Host "Clave publica detectada: $($pubKey.FullName)" -ForegroundColor Green
    }

    # Helper: probar conexion sin password usando BatchMode
    function Test-SshNoPassword {
        param([string]$TestIp, [int]$TestPort, [string]$TestUser)
        $portFlag = if ($TestPort -ne 22) { @("-p","$TestPort") } else { @() }
        $args = $portFlag + @("-o","ConnectTimeout=5","-o","BatchMode=yes","-o","StrictHostKeyChecking=accept-new","${TestUser}@${TestIp}","echo","SSH_NOPASS_OK")
        $out = & ssh @args 2>&1
        return ($out -match "SSH_NOPASS_OK")
    }

    if (Test-SshNoPassword -TestIp $Ip -TestPort $Port -TestUser $User) {
        Write-Host "La VM ya acepta conexiones sin password." -ForegroundColor Green
        return $true
    }

    $copy = Read-Host "Copiar clave publica a la VM? (pedira la password de SSH UNA vez) [S/n]"
    if ($copy.ToLower() -eq "n") { return $false }

    Write-Host "Copiando clave publica a la VM..." -ForegroundColor Yellow

    $pubContent = Get-Content $pubKey.FullName -Raw
    $portFlag   = if ($Port -ne 22) { "-p $Port" } else { "" }
    $remoteCmd  = "mkdir -p ~/.ssh && chmod 700 ~/.ssh && cat >> ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys && chown -R `$USER:`$USER ~/.ssh"

    $psi = New-Object System.Diagnostics.ProcessStartInfo("ssh")
    $psi.Arguments              = "$portFlag -o ConnectTimeout=10 -o StrictHostKeyChecking=accept-new ${User}@${Ip} `"$remoteCmd`""
    $psi.RedirectStandardInput  = $true
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError  = $true
    $psi.UseShellExecute        = $false

    $proc = [System.Diagnostics.Process]::Start($psi)
    $proc.StandardInput.Write($pubContent)
    $proc.StandardInput.Close()
    $proc.WaitForExit(30000) | Out-Null

    if ($proc.ExitCode -ne 0) {
        $err = $proc.StandardError.ReadToEnd()
        Write-Host "Error copiando la clave: $err" -ForegroundColor Yellow
        return $false
    }

    Write-Host "Clave copiada. Verificando..." -ForegroundColor Yellow
    Start-Sleep -Seconds 1

    if (Test-SshNoPassword -TestIp $Ip -TestPort $Port -TestUser $User) {
        Write-Host "Conexion sin password configurada correctamente!" -ForegroundColor Green
        return $true
    } else {
        Write-Host "La clave se copio pero la verificacion fallo." -ForegroundColor Yellow
        Write-Host "Posibles causas:" -ForegroundColor Gray
        Write-Host "  - El servidor SSH de la VM tiene 'PubkeyAuthentication no' en /etc/ssh/sshd_config" -ForegroundColor Gray
        Write-Host "  - El directorio .ssh o authorized_keys tiene propietario incorrecto en la VM" -ForegroundColor Gray
        Write-Host "  - Conecta manualmente y ejecuta: ls -la ~/.ssh/" -ForegroundColor Gray
        return $false
    }
}

function Invoke-CmdOpen {
    <#
    Objetivo: que todos los comandos del script no pidan password.

    Si no hay configuracion previa, llama a Invoke-CmdLogin internamente
    (misma invocacion del script, mismo $script:AgentStartedByUs).

    Dos mecanismos independientes:
      1. SSH sin password  -> ssh-agent de Windows con la clave privada cargada.
         Si el servicio estaba parado lo ponemos en Manual (NO Automatic) y lo
         arrancamos. Al salir (exit/close) lo detenemos y restauramos el tipo.
      2. sudo sin password -> contrasena guardada en $env:RUNNERS_SUDO_PASS
         (variable de entorno de esta sesion PowerShell, nunca en disco).
         Desaparece al cerrar la consola.

    Por que no ControlMaster: el cliente OpenSSH de Windows no soporta
    sockets Unix (ControlMaster/ControlPath). Falla con "getsockname failed".

    Al terminar la configuracion entra en el bucle REPL interactivo.
    #>
    Write-Host "`n=== Abrir sesion interactiva (ssh-agent + sudo en memoria) ===" -ForegroundColor Cyan

    if (-not (Test-SshAvailable)) { return }

    # ── Si no hay config, solicitarla ahora (misma invocacion = mismo AgentStartedByUs) ──
    $cfg = Get-RunnerConfig
    if (-not $cfg) {
        Write-Host "No hay configuracion previa. Iniciando configuracion de conexion..." -ForegroundColor Yellow
        Invoke-CmdLogin
        $cfg = Get-RunnerConfig
        if (-not $cfg) {
            Write-Host "Configuracion cancelada o fallida. Saliendo." -ForegroundColor Red
            return
        }
    }

    # ── Paso 1: ssh-agent ─────────────────────────────────────────────────────
    Write-Host "`n[1/2] Configurando ssh-agent para autenticacion sin password..." -ForegroundColor Yellow

    $agentSvc = Get-Service -Name ssh-agent -ErrorAction SilentlyContinue
    if (-not $agentSvc) {
        Write-Host "WARN: El servicio 'ssh-agent' no existe en este sistema." -ForegroundColor Yellow
        Write-Host "      Instala el cliente OpenSSH de Windows:" -ForegroundColor Gray
        Write-Host "      Configuracion > Aplicaciones > Caracteristicas opcionales > OpenSSH Client" -ForegroundColor Gray
    } elseif ($agentSvc.Status -ne "Running") {
        Write-Host "Arrancando ssh-agent para esta sesion..." -ForegroundColor Yellow
        try {
            # Guardamos el tipo de inicio original para restaurarlo al salir.
            $script:AgentOriginalStartType = $agentSvc.StartType
            # Manual: el servicio puede arrancarse sin estar en el Startup del sistema.
            Set-Service ssh-agent -StartupType Manual -ErrorAction Stop
            Start-Service ssh-agent -ErrorAction Stop
            $script:AgentStartedByUs = $true
            Write-Host "OK ssh-agent arrancado (se detendra al hacer 'exit' o 'close')." -ForegroundColor Green
        } catch {
            Write-Host "ERROR: No se pudo arrancar ssh-agent: $_" -ForegroundColor Red
            Write-Host "Asegurate de ejecutar PowerShell como Administrador." -ForegroundColor Yellow
        }
    } else {
        Write-Host "OK ssh-agent ya estaba en ejecucion." -ForegroundColor Green
    }

    # Buscar clave privada y cargarla en el agente si no esta ya
    $sshDir  = Join-Path $env:USERPROFILE ".ssh"
    $privKey = Get-ChildItem -Path "$sshDir\*" -Include "id_ed25519","id_rsa","id_ecdsa" -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $privKey) {
        Write-Host "No se encontro clave privada en $sshDir." -ForegroundColor Yellow
        Write-Host "Ejecuta 'login' desde el REPL para generar y copiar la clave." -ForegroundColor Gray
    } else {
        $loadedKeys = & ssh-add -l 2>&1
        $keyLoaded  = $loadedKeys | Where-Object { $_ -match [regex]::Escape($privKey.Name) -or $_ -match [regex]::Escape($privKey.FullName) }
        if ($keyLoaded) {
            Write-Host "OK clave '$($privKey.Name)' ya cargada en ssh-agent." -ForegroundColor Green
        } else {
            Write-Host "Cargando clave '$($privKey.Name)' en ssh-agent..." -ForegroundColor Yellow
            Write-Host "(Si la clave tiene passphrase te la pedira ahora)" -ForegroundColor Gray
            & ssh-add $privKey.FullName
            if ($LASTEXITCODE -ne 0) {
                Write-Host "WARN: No se pudo cargar la clave en ssh-agent." -ForegroundColor Yellow
            } else {
                Write-Host "OK clave cargada." -ForegroundColor Green
            }
        }
    }

    # Verificar que SSH a la VM funciona sin password
    $baseArgs = Get-SshBaseArgs -Cfg $cfg
    $target   = "$($cfg.User)@$($cfg.IP)"
    $testArgs = ($baseArgs + @("-o", "BatchMode=yes", $target, "echo SSH_OK")) -join " "
    $testOut  = & ssh $testArgs.Split(" ") 2>&1
    if ($testOut -match "SSH_OK") {
        Write-Host "OK conexion SSH sin password verificada." -ForegroundColor Green
    } else {
        Write-Host "WARN: La conexion SSH todavia pide password." -ForegroundColor Yellow
        Write-Host "      Asegurate de haber copiado la clave publica a la VM ('login')" -ForegroundColor Gray
        Write-Host "      y que el agente tiene la clave cargada (ssh-add -l)" -ForegroundColor Gray
    }

    # ── Paso 2: contrasena sudo ────────────────────────────────────────────────
    Write-Host "`n[2/2] Configurando contrasena sudo en memoria..." -ForegroundColor Yellow
    Write-Host "(Se guarda SOLO en esta sesion PowerShell. Nunca en disco.)" -ForegroundColor Gray

    if (-not [string]::IsNullOrEmpty($env:RUNNERS_SUDO_PASS)) {
        Write-Host "Ya hay una contrasena sudo en memoria." -ForegroundColor Green
        $reuse = Read-Host "Usar la misma? [S/n]"
        if ($reuse.ToLower() -eq "n") { Remove-Item Env:RUNNERS_SUDO_PASS -ErrorAction SilentlyContinue }
        else { Write-Host "OK reutilizando contrasena sudo existente." -ForegroundColor Green }
    }

    if ([string]::IsNullOrEmpty($env:RUNNERS_SUDO_PASS)) {
        $sudoPass = Read-HostMasked "Contrasena sudo del usuario '$($cfg.User)' en la VM"
        if ([string]::IsNullOrEmpty($sudoPass)) {
            Write-Host "Sin contrasena sudo. Los comandos que requieran sudo pediran password manualmente." -ForegroundColor Yellow
        } else {
            # Verificar: escapar comillas simples para la linea de shell
            $escapedPass = $sudoPass -replace "'", "'\'''"
            $verifyCmd = ($baseArgs + @($target, "printf '%s\n' '$escapedPass' | sudo -S -p '' echo SUDO_OK 2>/dev/null")) -join " "
            $verifyOut = & ssh $verifyCmd.Split(" ") 2>&1
            if ($verifyOut -match "SUDO_OK") {
                $env:RUNNERS_SUDO_PASS = $sudoPass
                Write-Host "OK contrasena sudo verificada y almacenada en memoria." -ForegroundColor Green
            } else {
                Write-Host "WARN: Contrasena sudo incorrecta o sin permisos sudo. Los comandos sudo pediran password manualmente." -ForegroundColor Yellow
            }
        }
    }

    # ── Entrar en REPL interactivo ────────────────────────────────────────────
    Start-RunnerRepl
}

function Invoke-CmdClose {
    param([switch]$Silent)
    if (-not $Silent) { Write-Host "`n=== Cerrar sesion ===" -ForegroundColor Cyan }

    # Usar Remove-Item garantiza que la variable desaparezca del entorno del proceso
    Remove-Item Env:RUNNERS_SUDO_PASS -ErrorAction SilentlyContinue
    Write-Host "Contrasena sudo eliminada de memoria." -ForegroundColor Green

    # Si nosotros arrancamos el ssh-agent, lo detenemos y restauramos el tipo de inicio.
    if ($script:AgentStartedByUs) {
        try {
            Stop-Service ssh-agent -Force -ErrorAction Stop
            if ($script:AgentOriginalStartType) {
                Set-Service ssh-agent -StartupType $script:AgentOriginalStartType -ErrorAction SilentlyContinue
            }
            $script:AgentStartedByUs       = $false
            $script:AgentOriginalStartType = $null
            Write-Host "ssh-agent detenido y restaurado al estado original." -ForegroundColor Green
        } catch {
            Write-Host "WARN: No se pudo detener ssh-agent: $_" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Nota: el ssh-agent no fue arrancado por este script; sigue corriendo." -ForegroundColor Gray
        Write-Host "Para descargar las claves del agente ejecuta: ssh-add -D" -ForegroundColor Gray
    }
}

function Start-RunnerRepl {
    <#
    Bucle interactivo
    El usuario escribe comandos directamente (sin '.\runners.ps1' delante).
    'exit' o 'close' limpia la sesion y termina el bucle.
    #>
    $cfg  = Get-RunnerConfig
    $repo = if ($cfg -and $cfg.RepoUrl) { ($cfg.RepoUrl -replace "https://github.com/","") } else { "?" }

    Write-Host "`n  Sesion activa para: " -NoNewline -ForegroundColor Gray
    Write-Host $repo -ForegroundColor White
    Write-Host "  Escribe un comando o 'help' / '?' para ver la lista. 'exit' para salir." -ForegroundColor Gray
    Write-Host ""

    while ($true) {
        $sudoTag = if ($env:RUNNERS_SUDO_PASS) { "sudo" } else { "nosudo" }
        Write-Host "runners[$sudoTag]> " -NoNewline -ForegroundColor Cyan
        $raw = Read-Host
        $cmd = $raw.Trim().ToLower()

        if ([string]::IsNullOrWhiteSpace($cmd)) { continue }

        switch ($cmd) {
            { $_ -in @("exit","quit","close") } {
                Invoke-CmdClose
                Write-Host "Sesion cerrada." -ForegroundColor Cyan
                return
            }
            "status"  { Invoke-CmdStatus }
            "start"   { Invoke-CmdStart }
            "stop"    { Invoke-CmdStop }
            "restart" { Invoke-CmdRestart }
            "pause"   { Invoke-CmdPause }
            "resume"  { Invoke-CmdResume }
            "logs"    { Invoke-CmdLogs }
            "list"    { Invoke-CmdList }
            "shell"   { Invoke-CmdShell }
            "remove"  { Invoke-CmdRemove }
            "update"  { Invoke-CmdUpdate }
            "install" { Invoke-CmdInstall }
            "login"   { Invoke-CmdLogin }
            { $_ -in @("help","?") } { Invoke-CmdHelp }
            default {
                Write-Host "Comando desconocido: '$cmd'. Escribe 'help' o '?' para ver los disponibles." -ForegroundColor Yellow
            }
        }
        Write-Host ""
    }
}

function Invoke-CmdInstall {
    Write-Host "`n=== Instalar runners + entorno ===" -ForegroundColor Cyan

    $cfg = Get-RunnerConfig
    if (-not $cfg) { Write-Host "Ejecuta: .\runners.ps1 open" -ForegroundColor Red; return }
    if (-not (Test-SshAvailable)) { return }

    $url = if ($cfg.RepoUrl) { $cfg.RepoUrl } else { Read-Host "URL del repo u organizacion (ej: https://github.com/Org/Repo)" }
    if ([string]::IsNullOrWhiteSpace($url)) { Write-Host "URL requerida." -ForegroundColor Red; return }

    # Validar URL via API de GitHub (desde el host, no desde la VM)
    Write-Host "Validando URL del repo..." -ForegroundColor Yellow
    try {
        $apiUrl = $url -replace "https://github.com/", "https://api.github.com/repos/"
        $resp   = Invoke-WebRequest -Uri $apiUrl -UseBasicParsing -ErrorAction Stop -TimeoutSec 10
        if ($resp.StatusCode -eq 200) {
            Write-Host "URL valida." -ForegroundColor Green
        } else {
            Write-Host "WARN: La URL devolvio HTTP $($resp.StatusCode)." -ForegroundColor Yellow
        }
    } catch {
        Write-Host "WARN: No se pudo validar la URL ($($_.Exception.Message)). Continuando..." -ForegroundColor Yellow
    }

    $token = Read-HostMasked "Token de registro (GitHub > Settings > Actions > Runners > New self-hosted runner)"
    if ([string]::IsNullOrWhiteSpace($token)) { Write-Host "Token requerido." -ForegroundColor Red; return }

    $countStr = Read-Host "Numero de runners a crear [1]"
    if ([string]::IsNullOrWhiteSpace($countStr)) { $countStr = "1" }
    if (-not [int]::TryParse($countStr, [ref]$null)) { Write-Host "Numero invalido." -ForegroundColor Red; return }
    $count = [int]$countStr

    $baseName = Read-Host "Nombre base de los runners [ubuntu-runner]"
    if ([string]::IsNullOrWhiteSpace($baseName)) { $baseName = "ubuntu-runner" }

    # Docker es obligatorio: cada job corre dentro de su propio contenedor para
    # aislar herramientas (dotnet, node, etc.) y garantizar entornos limpios.
    # No instalamos SDK/runtime en la VM — los workflows declaran 'container:'
    # con la imagen que necesiten.
    $installDocker = $true

    Write-Host "`nResumen:" -ForegroundColor Yellow
    Write-Host "  Repo:        $url"
    Write-Host "  Runners:     $count"
    Write-Host "  Nombre base: $baseName"
    Write-Host "  Docker:      Si (obligatorio para container jobs)"
    $confirm = Read-Host "Continuar? [S/n]"
    if ($confirm.ToLower() -eq "n") { Write-Host "Cancelado." -ForegroundColor Yellow; return }

    # Verificar pwsh en la VM
    Write-Host "`nVerificando PowerShell en la VM..." -ForegroundColor Yellow
    $pwshCheck = Invoke-Ssh -RemoteCommand "which pwsh 2>/dev/null || echo NOTFOUND"
    if ($pwshCheck -and $pwshCheck.StdOut.Trim() -eq "NOTFOUND") {
        Write-Host "PowerShell no encontrado en la VM. Instalando..." -ForegroundColor Yellow
        $installPwsh = @"
export DEBIAN_FRONTEND=noninteractive
apt-get update -q
apt-get install -y wget apt-transport-https software-properties-common
UBUNTU_VER=\$(lsb_release -rs)
wget -q "https://packages.microsoft.com/config/ubuntu/\${UBUNTU_VER}/packages-microsoft-prod.deb" -O /tmp/ms-prod.deb
dpkg -i /tmp/ms-prod.deb
apt-get update -q
apt-get install -y powershell
"@
        Invoke-SshSudo -RemoteCommand $installPwsh -TimeoutSec 180 | Out-Null
        $pwshCheck2 = Invoke-Ssh -RemoteCommand "which pwsh 2>/dev/null || echo NOTFOUND"
        if ($pwshCheck2 -and $pwshCheck2.StdOut.Trim() -eq "NOTFOUND") {
            Write-Host "ERROR: No se pudo instalar PowerShell en la VM. Instalalo manualmente." -ForegroundColor Red
            return
        }
    }
    Write-Host "PowerShell OK en la VM." -ForegroundColor Green

    Write-Host "`nTransfiriendo script de instalacion..." -ForegroundColor Yellow

    # Script embebido que se ejecuta en la VM como root via pwsh.
    # NOTA DE SEGURIDAD: el token de registro se pasa via variable de entorno
    # (RUNNER_TOKEN) en lugar de como argumento de linea de comandos, para que
    # no aparezca en 'ps aux' durante la instalacion.
    $setupScript = @'
#!/usr/bin/env pwsh
param(
    [string]$Url,
    [int]$Count = 1,
    [string]$BaseName = "ubuntu-runner",
    [switch]$SkipDocker
)
$ErrorActionPreference = "Stop"

# El token de registro se lee de la variable de entorno para evitar
# que aparezca en 'ps aux' como argumento de linea de comandos.
$Token = $env:RUNNER_TOKEN
if ([string]::IsNullOrWhiteSpace($Token)) {
    Write-Host "ERR: Variable de entorno RUNNER_TOKEN no definida." -ForegroundColor Red
    exit 1
}

# Helper: ejecuta bash y devuelve resultado
function sb {
    param([string]$c)
    $p = New-Object System.Diagnostics.ProcessStartInfo("bash")
    $p.ArgumentList.Add("-c")
    $p.ArgumentList.Add($c)
    $p.RedirectStandardOutput = $true
    $p.RedirectStandardError  = $true
    $p.UseShellExecute        = $false
    $pr = [System.Diagnostics.Process]::Start($p)
    $pr.WaitForExit(300000) | Out-Null
    return [pscustomobject]@{
        Out  = $pr.StandardOutput.ReadToEnd()
        Err  = $pr.StandardError.ReadToEnd()
        Code = $pr.ExitCode
    }
}

$usr = "vmusr"

if ((& id -u) -ne "0") { Write-Host "ERR: Ejecutar como root" -ForegroundColor Red; exit 1 }

Write-Host "[=== Dependencias base ===]" -ForegroundColor Cyan
$pkgs = "git curl wget tar gzip unzip jq ca-certificates apt-transport-https software-properties-common libicu-dev"
$r = sb "export DEBIAN_FRONTEND=noninteractive && apt-get update -q && apt-get install -y $pkgs"
if ($r.Code -ne 0) { Write-Host "WARN apt: $($r.Err)" -ForegroundColor Yellow } else { Write-Host "OK paquetes base" -ForegroundColor Green }

$id = sb "id -u $usr 2>/dev/null || echo NOTFOUND"
if ($id.Out.Trim() -eq "NOTFOUND") {
    sb "useradd -m -s /bin/bash $usr" | Out-Null
    sb "passwd -l $usr" | Out-Null
    Write-Host "OK usuario $usr creado" -ForegroundColor Green
} else {
    Write-Host "WARN usuario $usr ya existe" -ForegroundColor Yellow
}
sb "chown -R ${usr}:${usr} /home/$usr" | Out-Null

if (-not $SkipDocker) {
    Write-Host "[=== Docker CE ===]" -ForegroundColor Cyan
    # Docker CE oficial (docker.io del repo Ubuntu va por detras varios releases).
    # Comprobacion idempotente: si ya esta instalado y el daemon corre, no
    # reinstalamos.
    $dc = sb "docker --version 2>/dev/null || echo NOTFOUND"
    if ($dc.Out.Trim() -match "^Docker version") {
        Write-Host "WARN Docker ya instalado: $($dc.Out.Trim())" -ForegroundColor Yellow
    } else {
        # Anadir GPG key + repo oficial Docker
        sb "install -m 0755 -d /etc/apt/keyrings" | Out-Null
        sb "curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --batch --yes --dearmor -o /etc/apt/keyrings/docker.gpg" | Out-Null
        sb "chmod a+r /etc/apt/keyrings/docker.gpg" | Out-Null
        sb "echo `"deb [arch=`$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu `$(. /etc/os-release && echo `$VERSION_CODENAME) stable`" > /etc/apt/sources.list.d/docker.list" | Out-Null
        sb "export DEBIAN_FRONTEND=noninteractive && apt-get update -q" | Out-Null
        $di = sb "export DEBIAN_FRONTEND=noninteractive && apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin"
        if ($di.Code -ne 0) {
            Write-Host "ERR docker install: $($di.Err)" -ForegroundColor Red
            exit 1
        }
        Write-Host "OK Docker CE instalado" -ForegroundColor Green
    }

    # Anadir el usuario runner al grupo docker para poder lanzar containers
    # sin sudo. Idempotente: usermod -aG no duplica.
    sb "usermod -aG docker $usr" | Out-Null
    Write-Host "OK $usr anadido a grupo docker" -ForegroundColor Green

    # Asegurar que el daemon arranca en boot y esta activo ahora.
    sb "systemctl enable --now docker" | Out-Null
    $ds = (sb "systemctl is-active docker 2>/dev/null || echo unknown").Out.Trim()
    if ($ds -eq "active") {
        Write-Host "OK docker.service activo" -ForegroundColor Green
    } else {
        Write-Host "WARN docker.service estado: $ds" -ForegroundColor Yellow
    }

    # Smoke test rapido: pull + run hello-world. Confirma que el grupo y el
    # socket funcionan antes de seguir registrando runners.
    $hw = sb "su - $usr -c 'docker run --rm hello-world 2>&1'"
    if ($hw.Code -eq 0) {
        Write-Host "OK docker run hello-world" -ForegroundColor Green
    } else {
        Write-Host "WARN docker run hello-world fallo (puede requerir re-login del usuario): $($hw.Out)" -ForegroundColor Yellow
    }
}

Write-Host "[=== actions-runner ===]" -ForegroundColor Cyan
$relJson = sb "curl -s https://api.github.com/repos/actions/runner/releases/latest"
$ver     = ($relJson.Out | ConvertFrom-Json).tag_name.TrimStart('v')
$arch    = (sb "uname -m").Out.Trim()
if     ($arch -eq "x86_64")  { $a = "x64" }
elseif ($arch -eq "aarch64") { $a = "arm64" }
else   { Write-Host "Arquitectura no soportada: $arch" -ForegroundColor Red; exit 1 }

$pkg    = "actions-runner-linux-$a-$ver.tar.gz"
$urlPkg = "https://github.com/actions/runner/releases/download/v$ver/$pkg"
$tmpPkg = "/tmp/$pkg"

$existCheck = sb "test -f $tmpPkg && echo EXISTS || echo MISSING"
if ($existCheck.Out.Trim() -ne "EXISTS") {
    sb "curl -o $tmpPkg -L $urlPkg" | Out-Null
}
Write-Host "OK runner descargado v$ver" -ForegroundColor Green

$svcs = [System.Collections.Generic.List[string]]::new()
for ($i = 1; $i -le $Count; $i++) {
    $rn = "$BaseName-$i"
    $rd = "/home/$usr/actions-runner-$i"
    Write-Host "`n  -> Configurando $rn en $rd" -ForegroundColor White

    sb "rm -rf $rd; mkdir -p $rd && chown ${usr}:${usr} $rd" | Out-Null
    sb "tar xzf $tmpPkg -C $rd && chown -R ${usr}:${usr} $rd" | Out-Null

    $cfgCmd = "cd $rd && ./config.sh --url `"$Url`" --token `"$Token`" --name `"$rn`" --work _work --unattended --replace"
    $cr = sb "su - $usr -c '$cfgCmd'"
    if ($cr.Code -ne 0) {
        Write-Host "ERR config runner ${rn}: $($cr.Err)" -ForegroundColor Red
        continue
    }
    Write-Host "OK $rn registrado" -ForegroundColor Green

    # Instalar servicio systemd
    $svcInstall = sb "cd $rd && ./svc.sh install $usr 2>&1"
    if ($svcInstall.Code -ne 0) {
        Write-Host "WARN svc.sh install: $($svcInstall.Out)" -ForegroundColor Yellow
    }

    sb "systemctl daemon-reload" | Out-Null

    # El runner escribe el nombre exacto del servicio en $rd/.service tras instalar.
    $sn = (sb "cat '$rd/.service' 2>/dev/null").Out.Trim()

    # Fallback: buscar por glob en /etc/systemd/system/
    if ([string]::IsNullOrWhiteSpace($sn)) {
        $sn = (sb "ls /etc/systemd/system/actions.runner.*.$rn.service 2>/dev/null | head -1 | xargs -r basename").Out.Trim()
    }

    if ([string]::IsNullOrWhiteSpace($sn)) {
        Write-Host "WARN: no se detecto el servicio de $rn en systemd" -ForegroundColor Yellow
        Write-Host "      Salida de svc.sh: $($svcInstall.Out)" -ForegroundColor Gray
        continue
    }

    sb "systemctl daemon-reload && systemctl enable --now `"$sn`"" | Out-Null
    $st = (sb "systemctl is-active `"$sn`" 2>/dev/null || echo unknown").Out.Trim()
    $col = if ($st -eq "active") { "Green" } else { "Red" }
    Write-Host "OK servicio $sn -> " -NoNewline
    Write-Host $st -ForegroundColor $col

    $svcs.Add($sn)
}

sb "rm -f $tmpPkg" | Out-Null

if ($svcs.Count -eq 0) {
    Write-Host "`nERR: Ningun runner se registro correctamente." -ForegroundColor Red
    exit 1
}

Write-Host "`n[=== RESUMEN ===]" -ForegroundColor Cyan
foreach ($s in $svcs) {
    $st  = (sb "systemctl is-active `"$s`" 2>/dev/null || echo unknown").Out.Trim()
    $col = if ($st -eq "active") { "Green" } else { "Red" }
    Write-Host "  $s -> " -NoNewline
    Write-Host $st -ForegroundColor $col
}
'@

    $remoteScript = "/tmp/setup-runner-remote.ps1"
    if (-not (Send-FileToRemote -Content $setupScript -RemotePath $remoteScript)) { return }

    $skipDockerFlag = if (-not $installDocker) { "-SkipDocker" } else { "" }

    # El token se pasa via 'sudo env VAR=val' para que llegue al proceso pwsh
    # aunque sudoers tenga 'env_reset' activo (sudo -E se ignora en ese caso).
    $escapedToken = $token -replace "'", "'\'''"
    $remoteCmd = "sudo env RUNNER_TOKEN='$escapedToken' pwsh $remoteScript -Url `"$url`" -Count $count -BaseName `"$baseName`" $skipDockerFlag"

    Write-Host "Ejecutando instalacion remota (esto puede tardar varios minutos)..." -ForegroundColor Yellow
    $result = Invoke-Ssh -RemoteCommand $remoteCmd -Interactive -TimeoutSec 600

    if ($result -and $result.ExitCode -eq 0) {
        $cfg.RunnerBaseName = $baseName
        $cfg.RunnerCount    = $count
        $cfg.RepoUrl        = $url
        Save-RunnerConfig -Config $cfg
        Write-Host "`nInstalacion completada exitosamente." -ForegroundColor Green
        $names = 1..$count | ForEach-Object { "$baseName-$_" }
        Write-Host "Runners registrados: $($names -join ', ')" -ForegroundColor Green
    } else {
        Write-Host "`nLa instalacion finalizo con codigo $($result.ExitCode). Revisa los logs arriba." -ForegroundColor Yellow
    }
}

function Invoke-CmdStatus {
    Write-Host "`n=== Estado de los runners ===" -ForegroundColor Cyan
    $cfg = Get-RunnerConfig
    if (-not $cfg -or -not $cfg.RunnerCount -or [int]$cfg.RunnerCount -le 0) {
        Write-Host "No hay runners registrados en la configuracion local." -ForegroundColor Yellow
        return
    }

    $svcList = (Get-AllServiceNames -Cfg $cfg | ForEach-Object { "`"$_`"" }) -join " "
    $result  = Invoke-SshSudo -RemoteCommand "systemctl list-units --type=service $svcList --no-pager --plain" -TimeoutSec 30
    if ($result -and $result.StdOut) {
        Write-Host $result.StdOut
    } elseif ($result -and $result.StdErr) {
        Write-Host $result.StdErr -ForegroundColor DarkRed
    } else {
        # Fallback: estado uno a uno
        foreach ($svc in (Get-AllServiceNames -Cfg $cfg)) {
            $r = Invoke-SshSudo -RemoteCommand "systemctl is-active `"$svc`"" -TimeoutSec 15
            $st = if ($r) { $r.StdOut.Trim() } else { "unknown" }
            $col = if ($st -eq "active") { "Green" } else { "Red" }
            Write-Host "  $svc -> " -NoNewline; Write-Host $st -ForegroundColor $col
        }
    }
}

function Invoke-CmdStart {
    Write-Host "`nArrancando runners..." -ForegroundColor Yellow
    $cfg = Get-RunnerConfig
    Invoke-SshSudoExpanded -SystemctlAction "start" -Cfg $cfg
    Write-Host "Runners arrancados." -ForegroundColor Green
    Invoke-CmdStatus
}

function Invoke-CmdStop {
    Write-Host "`nDeteniendo runners..." -ForegroundColor Yellow
    $cfg = Get-RunnerConfig
    Invoke-SshSudoExpanded -SystemctlAction "stop" -Cfg $cfg
    Write-Host "Runners detenidos." -ForegroundColor Green
}

function Invoke-CmdRestart {
    Write-Host "`nReiniciando runners..." -ForegroundColor Yellow
    $cfg = Get-RunnerConfig
    Invoke-SshSudoExpanded -SystemctlAction "restart" -Cfg $cfg
    Write-Host "Runners reiniciados." -ForegroundColor Green
    Invoke-CmdStatus
}

function Invoke-CmdPause {
    Write-Host "`nPausando runners (sin desregistrar de GitHub)..." -ForegroundColor Yellow
    $cfg = Get-RunnerConfig
    Invoke-SshSudoExpanded -SystemctlAction "stop" -Cfg $cfg
    Write-Host "Runners pausados. Usa 'resume' para reanudarlos." -ForegroundColor Green
}

function Invoke-CmdResume {
    Write-Host "`nReanudando runners..." -ForegroundColor Yellow
    $cfg = Get-RunnerConfig
    Invoke-SshSudoExpanded -SystemctlAction "start" -Cfg $cfg
    Write-Host "Runners reanudados." -ForegroundColor Green
    Invoke-CmdStatus
}

function Invoke-CmdLogs {
    Write-Host "`n=== Logs en tiempo real (Ctrl+C para salir) ===" -ForegroundColor Cyan
    $cfg = Get-RunnerConfig
    if (-not $cfg) { Write-Host "Ejecuta: .\runners.ps1 open" -ForegroundColor Red; return }

    $baseArgs  = Get-SshBaseArgs -Cfg $cfg
    $target    = "$($cfg.User)@$($cfg.IP)"
    $pattern   = Get-RunnerServicePattern -Cfg $cfg

    $svc = Read-Host "Nombre del servicio o Enter para todos"
    $journalTarget = if ([string]::IsNullOrWhiteSpace($svc)) { "-u '$pattern'" } else { "-u '$svc'" }

    # Usar & en lugar de Start-Process para que Ctrl+C funcione correctamente
    # en la sesion interactiva del REPL
    $allArgs = $baseArgs + @("-t", $target, "sudo journalctl $journalTarget -f --no-pager")
    & ssh @allArgs
}

function Invoke-CmdList {
    Write-Host "`n=== Runners registrados ===" -ForegroundColor Cyan
    $cfg = Get-RunnerConfig
    if ($cfg -and [int]$cfg.RunnerCount -gt 0) {
        Write-Host "Runners en configuracion local ($($cfg.RunnerCount) total):" -ForegroundColor Yellow
        for ($i = 1; $i -le [int]$cfg.RunnerCount; $i++) {
            Write-Host "  - $($cfg.RunnerBaseName)-$i"
        }
    } else {
        Write-Host "No hay runners en configuracion local." -ForegroundColor Yellow
    }

    Write-Host "`nServicios systemd (activos e inactivos):" -ForegroundColor Yellow
    $pattern = Get-RunnerServicePattern -Cfg $cfg
    $result  = Invoke-SshSudo -RemoteCommand "systemctl list-units --type=service --all `"$pattern`" --no-pager --plain" -TimeoutSec 30
    if ($result -and $result.StdOut) {
        Write-Host $result.StdOut
    }
}

function Invoke-CmdShell {
    Write-Host "`nAbriendo shell SSH en la VM (exit para volver)..." -ForegroundColor Cyan
    $cfg = Get-RunnerConfig
    if (-not $cfg) { Write-Host "Ejecuta: .\runners.ps1 open" -ForegroundColor Red; return }
    $baseArgs = Get-SshBaseArgs -Cfg $cfg
    $target   = "$($cfg.User)@$($cfg.IP)"
    # & en lugar de Start-Process para integracion correcta con la consola actual
    & ssh @($baseArgs + @($target))
}

function Invoke-CmdRemove {
    Write-Host "`n=== Eliminar runners de la VM y de GitHub ===" -ForegroundColor Cyan
    $cfg = Get-RunnerConfig
    if (-not $cfg) { Write-Host "Ejecuta: .\runners.ps1 open" -ForegroundColor Red; return }
    if (-not $cfg.RunnerCount -or [int]$cfg.RunnerCount -le 0) {
        Write-Host "No hay runners registrados en la configuracion local." -ForegroundColor Yellow
        return
    }

    $baseName = $cfg.RunnerBaseName
    $count    = [int]$cfg.RunnerCount

    Write-Host "Runners en configuracion local:" -ForegroundColor Yellow
    for ($i = 1; $i -le $count; $i++) { Write-Host "  [$i] $baseName-$i" }
    Write-Host "  [A] Todos"
    Write-Host "  [C] Cancelar"

    $sel = Read-Host "Selecciona numero, 'A' para todos, o 'C' para cancelar"
    if ($sel.ToLower() -eq "c") { Write-Host "Cancelado." -ForegroundColor Yellow; return }

    $targets = @()
    if ($sel.ToLower() -eq "a") {
        $targets = 1..$count
    } else {
        try {
            $idx = [int]::Parse($sel)
            if ($idx -lt 1 -or $idx -gt $count) { Write-Host "Numero fuera de rango (1-$count)." -ForegroundColor Red; return }
            $targets = @($idx)
        }
        catch { Write-Host "Seleccion invalida." -ForegroundColor Red; return }
    }

    Write-Host "`nIMPORTANTE: Para eliminar un runner de GitHub necesitas un token de eliminacion." -ForegroundColor Yellow
    Write-Host "Obtelo en: Settings > Actions > Runners > [runner] > Remove" -ForegroundColor Gray
    $removeToken = Read-HostMasked "Token de eliminacion"
    if ([string]::IsNullOrWhiteSpace($removeToken)) { Write-Host "Token requerido." -ForegroundColor Red; return }

    foreach ($i in $targets) {
        $runnerName = "$baseName-$i"
        $runnerDir  = "/home/vmusr/actions-runner-$i"

        Write-Host "`nProcesando: $runnerName" -ForegroundColor Cyan

        # 1. Detener via svc.sh
        $r = Invoke-SshSudo -RemoteCommand "cd $runnerDir && ./svc.sh stop 2>&1" -TimeoutSec 30
        if ($r -and $r.ExitCode -eq 0) {
            Write-Host "  Servicio detenido" -ForegroundColor Green
        } else {
            $detail = if ($r -and $r.StdOut) { $r.StdOut.Trim() } else { "sin detalle" }
            Write-Host "  WARN svc.sh stop: $detail" -ForegroundColor Yellow
        }

        # 2. Desinstalar la unit de systemd via svc.sh
        #    DEBE ir antes de config.sh remove
        $r = Invoke-SshSudo -RemoteCommand "cd $runnerDir && ./svc.sh uninstall 2>&1" -TimeoutSec 30
        if ($r -and $r.ExitCode -eq 0) {
            Write-Host "  Unit de systemd eliminada" -ForegroundColor Green
        } else {
            $detail = if ($r -and $r.StdOut) { $r.StdOut.Trim() } else { "sin detalle" }
            Write-Host "  WARN svc.sh uninstall: $detail" -ForegroundColor Yellow
        }

        # 3. Desregistrar de GitHub (token via variable de entorno para no exponerlo)
        $escapedToken = $removeToken -replace "'", "'\'''"
        $deregCmd = "cd $runnerDir && RUNNER_REMOVE_TOKEN='$escapedToken' sudo -u vmusr env HOME=/home/vmusr RUNNER_REMOVE_TOKEN='$escapedToken' ./config.sh remove --token '$escapedToken'"
        $r = Invoke-SshSudo -RemoteCommand $deregCmd -TimeoutSec 60
        if ($r -and $r.ExitCode -eq 0) {
            Write-Host "  Runner desregistrado de GitHub" -ForegroundColor Green
        } else {
            $detail = if ($r -and $r.StdOut) { $r.StdOut.Trim() } else { "sin detalle" }
            Write-Host "  WARN: No se pudo desregistrar de GitHub: $detail" -ForegroundColor Yellow
        }

        # 4. Limpiar directorio
        Invoke-SshSudo -RemoteCommand "rm -rf $runnerDir" -TimeoutSec 30 | Out-Null
        Write-Host "  Directorio eliminado" -ForegroundColor Green
    }

    Invoke-SshSudo -RemoteCommand "systemctl daemon-reload" -TimeoutSec 30 | Out-Null

    # Limpiar config local si se eliminaron todos
    if ($targets.Count -eq $count) {
        $cfg.RunnerBaseName = $null
        $cfg.RunnerCount    = 0
        Save-RunnerConfig -Config $cfg
        Write-Host "`nEliminacion completada y config local limpiada." -ForegroundColor Green
    } else {
        Write-Host "`nEliminacion completada." -ForegroundColor Green
        Write-Host "NOTA: Actualiza manualmente RunnerCount en $ConfigFile si es necesario." -ForegroundColor Yellow
    }
}

function Invoke-CmdUpdate {
    Write-Host "`n=== Actualizar actions-runner ===" -ForegroundColor Cyan
    $cfg = Get-RunnerConfig
    if (-not $cfg) { Write-Host "Ejecuta: .\runners.ps1 open" -ForegroundColor Red; return }
    if (-not $cfg.RunnerCount -or [int]$cfg.RunnerCount -le 0) {
        Write-Host "No hay runners registrados en la configuracion local." -ForegroundColor Yellow
        return
    }

    $baseName = $cfg.RunnerBaseName
    $count    = [int]$cfg.RunnerCount

    Write-Host "Runners registrados: $count" -ForegroundColor Yellow
    $latest = Read-Host "Descargar ultima version automaticamente? [S/n]"
    $autoLatest = $latest.ToLower() -ne "n"

    $ver = ""
    if ($autoLatest) {
        Write-Host "Consultando ultima version..." -ForegroundColor Yellow
        try {
            $rel = Invoke-RestMethod -Uri "https://api.github.com/repos/actions/runner/releases/latest" -TimeoutSec 30
            $ver = $rel.tag_name.TrimStart('v')
            Write-Host "Ultima version disponible: $ver" -ForegroundColor Green
        } catch {
            Write-Host "No se pudo obtener la ultima version: $_" -ForegroundColor Red; return
        }
    } else {
        $ver = Read-Host "Introduce la version deseada (ej: 2.334.0)"
    }

    $confirm = Read-Host "Esto detendra los runners, actualizara los binarios y los rearrancara. Continuar? [S/n]"
    if ($confirm.ToLower() -eq "n") { Write-Host "Cancelado." -ForegroundColor Yellow; return }

    # Detectar arquitectura de la VM
    $archResult = Invoke-Ssh -RemoteCommand "uname -m"
    $arch = if ($archResult) { $archResult.StdOut.Trim() } else { "x86_64" }
    $a = if ($arch -eq "aarch64") { "arm64" } else { "x64" }

    $pkg    = "actions-runner-linux-$a-$ver.tar.gz"
    $urlPkg = "https://github.com/actions/runner/releases/download/v$ver/$pkg"
    $tmpPkg = "/tmp/$pkg"

    Write-Host "Descargando $pkg en la VM..." -ForegroundColor Yellow
    $dlResult = Invoke-SshSudo -RemoteCommand "curl -fSL -o $tmpPkg $urlPkg" -TimeoutSec 180
    if (-not $dlResult -or $dlResult.ExitCode -ne 0) {
        Write-Host "ERROR: No se pudo descargar el paquete." -ForegroundColor Red
        if ($dlResult -and $dlResult.StdOut) { Write-Host $dlResult.StdOut -ForegroundColor DarkRed }
        return
    }
    Write-Host "OK descargado" -ForegroundColor Green

    for ($i = 1; $i -le $count; $i++) {
        $runnerName = "$baseName-$i"
        $runnerDir  = "/home/vmusr/actions-runner-$i"
        $svc        = Get-RunnerServiceName -Cfg $cfg -RunnerName $runnerName

        Write-Host "`nActualizando: $runnerName" -ForegroundColor Cyan

        # Detener, extraer, ajustar permisos y arrancar
        $updateCmd = "systemctl stop `"$svc`" 2>/dev/null; tar xzf `"$tmpPkg`" -C `"$runnerDir`" && chown -R vmusr:vmusr `"$runnerDir`" && systemctl start `"$svc`""
        $r = Invoke-SshSudo -RemoteCommand $updateCmd -TimeoutSec 120
        if ($r -and $r.ExitCode -ne 0) {
            $detail = if ($r.StdOut) { $r.StdOut.Trim() } else { "sin detalle" }
            Write-Host "  WARN update: $detail" -ForegroundColor Yellow
        }

        # Verificar estado
        $stResult = Invoke-SshSudo -RemoteCommand "systemctl is-active `"$svc`"" -TimeoutSec 15
        $st = if ($stResult) { $stResult.StdOut.Trim() } else { "unknown" }
        $col = if ($st -eq "active") { "Green" } else { "Red" }
        Write-Host "  $runnerName -> " -NoNewline; Write-Host $st -ForegroundColor $col
    }

    Invoke-SshSudo -RemoteCommand "rm -f $tmpPkg" -TimeoutSec 30 | Out-Null
    Write-Host "`nActualizacion completada." -ForegroundColor Green
}

function Invoke-CmdHelp {
    $sudoReady = if (-not [string]::IsNullOrEmpty($env:RUNNERS_SUDO_PASS)) { "[sudo en memoria]" } else { "[sin sudo en memoria]" }
    $agentKeys = & ssh-add -l 2>&1
    $sshReady  = if ($agentKeys -notmatch "no identities|error") { "[clave SSH en agente]" } else { "[sin clave en agente]" }

    Write-Host @"

GitHub Actions Self-Hosted Runner Manager
==========================================

$sshReady  $sudoReady

FLUJO RECOMENDADO:
  1. .\runners.ps1 open     -> Punto de entrada unico.
                               Si no hay configuracion previa, la solicita automaticamente.
                               Configura ssh-agent + sudo en memoria y abre el REPL.

     Dentro del REPL escribe los comandos directamente, sin '.\runners.ps1':
       runners[sudo]> install   <- instala y registra los runners
       runners[sudo]> status    <- muestra el estado
       runners[sudo]> login     <- reconfigura la conexion sin salir de sesion
       runners[sudo]> exit      <- limpia sesion y detiene el ssh-agent

COMANDOS (dentro del REPL o como .\runners.ps1 <cmd>):
  open     Inicia sesion interactiva (ssh-agent + sudo en memoria + REPL)
  close    Limpia contrasena sudo, detiene ssh-agent si lo arranco este script
  install  Instala runners + entorno completo en la VM
  status   Muestra el estado de todos los servicios
  restart  Reinicia todos los servicios runner
  stop     Detiene todos los servicios runner
  start    Arranca todos los servicios runner
  pause    Detiene runners sin desregistrarlos de GitHub
  resume   Reanuda runners previamente pausados
  logs     Muestra logs en tiempo real
  list     Lista servicios runner en systemd
  remove   Elimina runners de la VM y los desregistra de GitHub
  update   Actualiza los binarios del actions-runner
  shell    Abre una sesion SSH interactiva en la VM
  login    Reconfigura la conexion SSH (IP, usuario, repo)
  help     Muestra esta ayuda

Configuracion: $ConfigFile
"@ -ForegroundColor White
}

#endregion

#region ── Entry point ──────────────────────────────────────────────────────────

switch ($Command) {
    "login"   { Invoke-CmdLogin }
    "open"    { Invoke-CmdOpen }
    "close"   { Invoke-CmdClose }
    "install" { Invoke-CmdInstall }
    "status"  { Invoke-CmdStatus }
    "restart" { Invoke-CmdRestart }
    "stop"    { Invoke-CmdStop }
    "start"   { Invoke-CmdStart }
    "pause"   { Invoke-CmdPause }
    "resume"  { Invoke-CmdResume }
    "logs"    { Invoke-CmdLogs }
    "list"    { Invoke-CmdList }
    "shell"   { Invoke-CmdShell }
    "remove"  { Invoke-CmdRemove }
    "update"  { Invoke-CmdUpdate }
    default   { Invoke-CmdHelp }
}

#endregion