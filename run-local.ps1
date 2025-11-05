param(
    [switch]$Clean,
    [switch]$NoBuild,
    [switch]$Foreground
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "`n[$(Get-Date -Format 'HH:mm:ss')] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "  $Message" -ForegroundColor Green
}

function Write-Error-Message {
    param([string]$Message)
    Write-Host "  ERROR: $Message" -ForegroundColor Red
}

function Test-DockerRunning {
    try {
        $null = docker info 2>&1
        return $true
    }
    catch {
        return $false
    }
}

function Test-EnvFileExists {
    return Test-Path -Path ".env"
}

function Initialize-Environment {
    if (-not (Test-EnvFileExists)) {
        Write-Step "Creando archivo .env desde plantilla..."
        Copy-Item ".env.example" ".env"
        Write-Success "Archivo .env creado"
        Write-Host "`n  IMPORTANTE: Edita el archivo .env y configura SQL_SERVER_PASSWORD" -ForegroundColor Yellow
        Write-Host "  Presiona Enter cuando hayas configurado la contraseña..." -ForegroundColor Yellow
        Read-Host
    }
}

function Invoke-Validation {
    Write-Step "Validando configuracion..."
    
    try {
        & ".\scripts\validate-env.ps1"
        Write-Success "Validacion exitosa"
    }
    catch {
        Write-Error-Message "Validacion fallida"
        throw
    }
}

function Invoke-Cleanup {
    Write-Step "Limpiando contenedores y volumenes existentes..."
    
    docker-compose down -v 2>&1 | Out-Null
    Write-Success "Limpieza completada"
}

function Invoke-Build {
    Write-Step "Construyendo imagenes Docker..."
    
    $buildOutput = docker-compose build 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Message "Fallo en la construccion de imagenes"
        $buildOutput | Select-Object -Last 20 | ForEach-Object { Write-Host "  $_" }
        throw "Build failed"
    }
    
    Write-Success "Imagenes construidas correctamente"
}

function Start-Services {
    param([bool]$Detached)
    
    Write-Step "Iniciando servicios..."
    
    if ($Detached) {
        docker-compose up -d
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Message "Fallo al iniciar servicios"
            throw "Failed to start services"
        }
    }
    else {
        Write-Success "Servicios iniciandose en modo foreground (Ctrl+C para detener)"
        Write-Host ""
        docker-compose up
        return
    }
    
    Write-Success "Servicios iniciados"
}

function Wait-ForHealthy {
    Write-Step "Esperando a que los servicios esten listos..."
    
    $maxAttempts = 60
    $attempt = 0
    $services = @("sqlserver", "web-api", "web-backoffice", "web-frontoffice")
    $healthy = @{}
    
    foreach ($service in $services) {
        $healthy[$service] = $false
    }
    
    while ($attempt -lt $maxAttempts) {
        $attempt++
        $allHealthy = $true
        
        foreach ($service in $services) {
            if (-not $healthy[$service]) {
                $status = docker inspect --format='{{.State.Health.Status}}' "proyectonet-$service" 2>$null
                
                if ($status -eq "healthy") {
                    $healthy[$service] = $true
                    Write-Success "$service esta listo"
                }
                else {
                    $allHealthy = $false
                }
            }
        }
        
        if ($allHealthy) {
            Write-Success "Todos los servicios estan listos"
            return $true
        }
        
        Start-Sleep -Seconds 2
    }
    
    Write-Host "`n  ADVERTENCIA: Timeout esperando a que los servicios esten listos" -ForegroundColor Yellow
    Write-Host "  Verifica el estado con: docker-compose ps" -ForegroundColor Yellow
    Write-Host "  Verifica los logs con: docker-compose logs -f" -ForegroundColor Yellow
    return $false
}

function Show-AccessInfo {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "  SERVICIOS DISPONIBLES" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    
    $envVars = Get-Content ".env" | Where-Object { $_ -match '=' } | ConvertFrom-StringData
    
    $apiPort = if ($envVars.API_HTTP_PORT) { $envVars.API_HTTP_PORT } else { "5000" }
    $backofficePort = if ($envVars.BACKOFFICE_HTTP_PORT) { $envVars.BACKOFFICE_HTTP_PORT } else { "5001" }
    $frontofficePort = if ($envVars.FRONTOFFICE_HTTP_PORT) { $envVars.FRONTOFFICE_HTTP_PORT } else { "5002" }
    
    Write-Host "`n  API REST:"
    Write-Host "    http://localhost:$apiPort" -ForegroundColor White
    Write-Host "    http://localhost:$apiPort/swagger (Swagger UI)" -ForegroundColor White
    Write-Host "    http://localhost:$apiPort/health (Health Check)" -ForegroundColor White
    
    Write-Host "`n  BackOffice:"
    Write-Host "    http://localhost:$backofficePort" -ForegroundColor White
    
    Write-Host "`n  FrontOffice:"
    Write-Host "    http://localhost:$frontofficePort" -ForegroundColor White
    
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "`n  Comandos utiles:" -ForegroundColor Cyan
    Write-Host "    docker-compose ps              Ver estado"
    Write-Host "    docker-compose logs -f         Ver logs"
    Write-Host "    docker-compose down            Detener"
    Write-Host "    docker-compose restart         Reiniciar"
    Write-Host "`n========================================`n" -ForegroundColor Green
}

try {
    Write-Host "`n╔═════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║    Proyecto.NET - Inicio Desarrollo     ║" -ForegroundColor Cyan
    Write-Host "╚═════════════════════════════════════════╝" -ForegroundColor Cyan
    
    Write-Step "Verificando Docker..."
    if (-not (Test-DockerRunning)) {
        Write-Error-Message "Docker no esta en ejecucion"
        Write-Host "  Inicia Docker Desktop y ejecuta este script nuevamente" -ForegroundColor Yellow
        exit 1
    }
    Write-Success "Docker esta disponible"
    
    Initialize-Environment
    Invoke-Validation
    
    if ($Clean) {
        Invoke-Cleanup
    }
    
    if (-not $NoBuild) {
        Invoke-Build
    }
    
    $detached = -not $Foreground
    Start-Services -Detached $detached
    
    if ($detached) {
        Wait-ForHealthy
        Show-AccessInfo
    }
}
catch {
    Write-Host "`n" -NoNewline
    Write-Error-Message "Error durante la inicializacion: $_"
    Write-Host "`n  Para mas detalles ejecuta: docker-compose logs -f`n" -ForegroundColor Yellow
    exit 1
}
