#!/usr/bin/env pwsh

param([switch]$Production)

$ErrorActionPreference = "Stop"

Write-Host "Validando variables de entorno..." -ForegroundColor Cyan
Write-Host ""

$envFile = ".env"
if (Test-Path $envFile) {
    Write-Host "✓ Archivo .env encontrado" -ForegroundColor Green
    Get-Content $envFile | ForEach-Object {
        if ($_ -match '^([^#][^=]+)=(.*)$') {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            [Environment]::SetEnvironmentVariable($name, $value, "Process")
        }
    }
} else {
    Write-Host "⚠ Archivo .env no encontrado. Creándolo desde .env.example..." -ForegroundColor Yellow
    if (Test-Path ".env.example") {
        Copy-Item ".env.example" ".env"
        Write-Host "✓ Archivo .env creado. Por favor, configura los valores apropiados." -ForegroundColor Green
        Write-Host ""
        exit 1
    } else {
        Write-Host "ERROR: Archivo .env.example no encontrado" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Verificando variables requeridas..." -ForegroundColor Cyan
Write-Host ""

$requiredVars = @("SQL_SERVER_PASSWORD", "ASPNETCORE_ENVIRONMENT", "DB_NAME")
$productionVars = @("CORS_ORIGIN_1")
$errors = @()

foreach ($var in $requiredVars) {
    $value = [Environment]::GetEnvironmentVariable($var, "Process")
    if ([string]::IsNullOrWhiteSpace($value)) {
        $errors += "Variable requerida no definida: $var"
    } else {
        Write-Host "✓ $var = " -NoNewline -ForegroundColor Green
        if ($var -like "*PASSWORD*" -or $var -like "*SECRET*") {
            Write-Host "********" -ForegroundColor Gray
        } else {
            Write-Host "$value" -ForegroundColor Gray
        }
    }
}

if ($Production) {
    Write-Host ""
    Write-Host "Validaciones adicionales para producción..." -ForegroundColor Cyan
    Write-Host ""
    
    foreach ($var in $productionVars) {
        $value = [Environment]::GetEnvironmentVariable($var, "Process")
        if ([string]::IsNullOrWhiteSpace($value)) {
            $errors += "⚠ Variable recomendada para producción: $var"
        } else {
            Write-Host "✓ $var = $value" -ForegroundColor Green
        }
    }
    
    $password = [Environment]::GetEnvironmentVariable("SQL_SERVER_PASSWORD", "Process")
    if ($password -eq "DevPassword123!") {
        $errors += "⚠ ADVERTENCIA: Usando contraseña por defecto en producción. ¡CAMBIAR!"
    }
    
    $env = [Environment]::GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Process")
    if ($env -eq "Development") {
        $errors += "⚠ ADVERTENCIA: ASPNETCORE_ENVIRONMENT está en Development. Debería ser Production."
    }
}

Write-Host ""

if ($errors.Count -gt 0) {
    Write-Host "⚠ Se encontraron problemas:" -ForegroundColor Yellow
    Write-Host ""
    foreach ($err in $errors) {
        Write-Host $err -ForegroundColor Yellow
    }
    Write-Host ""
    
    if ($Production) {
        Write-Host "No se puede continuar en modo producción con estos problemas." -ForegroundColor Red
        exit 1
    } else {
        Write-Host "⚠ Continuar de todas formas en modo desarrollo..." -ForegroundColor Yellow
    }
} else {
    Write-Host "Todas las validaciones pasaron exitosamente" -ForegroundColor Green
}

Write-Host ""
Write-Host "Listo para ejecutar docker-compose" -ForegroundColor Green
Write-Host ""
