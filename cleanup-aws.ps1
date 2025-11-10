# 🧹 Script de Limpieza Completa de AWS

# Este script elimina TODOS los recursos de AWS para ahorrar créditos del Learner Lab

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Limpieza de Recursos de AWS" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "ADVERTENCIA: Este script eliminara TODOS los recursos de AWS" -ForegroundColor Red
Write-Host "Esto incluye:" -ForegroundColor Yellow
Write-Host "  - Load Balancer" -ForegroundColor Gray
Write-Host "  - Servicios ECS" -ForegroundColor Gray
Write-Host "  - Base de datos RDS (PERDERAS TODOS LOS DATOS)" -ForegroundColor Gray
Write-Host "  - VPC y subnets" -ForegroundColor Gray
Write-Host "  - Repositorios ECR (imagenes Docker)" -ForegroundColor Gray
Write-Host ""

$confirm = Read-Host "¿Estas seguro de que deseas continuar? (escribe 'SI' para confirmar)"

if ($confirm -ne "SI") {
    Write-Host "Operacion cancelada." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Iniciando limpieza..." -ForegroundColor Yellow
Write-Host ""

# Verificar credenciales
Write-Host "[1/2] Verificando credenciales..." -ForegroundColor Yellow
try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    Write-Host "OK - Credenciales validas" -ForegroundColor Green
} catch {
    Write-Host "Error: Credenciales invalidas o expiradas" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Ejecutar Terraform Destroy
Write-Host "[2/2] Eliminando recursos con Terraform..." -ForegroundColor Yellow
Write-Host "Esto tomara 10-15 minutos..." -ForegroundColor Gray
Write-Host ""

Set-Location terraform

terraform destroy -auto-approve

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Hubo errores al eliminar algunos recursos" -ForegroundColor Yellow
    Write-Host "Puedes revisar manualmente en la consola de AWS" -ForegroundColor Yellow
    Set-Location ..
    exit 1
}

Set-Location ..

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Limpieza Completada!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Todos los recursos han sido eliminados." -ForegroundColor Green
Write-Host ""
Write-Host "Cuando necesites volver a desplegar, ejecuta:" -ForegroundColor Yellow
Write-Host "  .\deploy-aws.ps1" -ForegroundColor White
Write-Host ""

