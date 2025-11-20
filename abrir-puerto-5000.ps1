# Script para abrir puerto 5000 en el Firewall de Windows
# Ejecutar como ADMINISTRADOR

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Abriendo puerto 5000 en Firewall de Windows" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si se está ejecutando como administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "❌ ERROR: Este script debe ejecutarse como ADMINISTRADOR" -ForegroundColor Red
    Write-Host ""
    Write-Host "Haz clic derecho en PowerShell y selecciona 'Ejecutar como administrador'" -ForegroundColor Yellow
    Write-Host "Luego ejecuta:" -ForegroundColor Yellow
    Write-Host "  cd c:\Nadia\.NET\Proyecto.NET" -ForegroundColor White
    Write-Host "  .\abrir-puerto-5000.ps1" -ForegroundColor White
    Write-Host ""
    pause
    exit 1
}

Write-Host "✅ Ejecutando como administrador" -ForegroundColor Green
Write-Host ""

# Eliminar regla existente si existe
Write-Host "Verificando reglas existentes..." -ForegroundColor Yellow
$existingRule = Get-NetFirewallRule -DisplayName "DotNet Backend Port 5000" -ErrorAction SilentlyContinue

if ($existingRule) {
    Write-Host "Eliminando regla anterior..." -ForegroundColor Yellow
    Remove-NetFirewallRule -DisplayName "DotNet Backend Port 5000"
}

# Crear nueva regla
Write-Host "Creando regla de firewall para puerto 5000..." -ForegroundColor Yellow
New-NetFirewallRule -DisplayName "DotNet Backend Port 5000" `
                    -Direction Inbound `
                    -Protocol TCP `
                    -LocalPort 5000 `
                    -Action Allow `
                    -Profile Any `
                    -Description "Permite conexiones entrantes al backend .NET en puerto 5000"

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  ✅ Puerto 5000 abierto correctamente" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Ahora puedes conectarte desde el dispositivo Android a:" -ForegroundColor Cyan
Write-Host "  http://192.168.1.28:5000" -ForegroundColor White
Write-Host ""
Write-Host "Presiona Enter para cerrar..." -ForegroundColor Gray
Read-Host
