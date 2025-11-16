# Script para iniciar el backend local
Write-Host "🚀 Iniciando Backend Local..." -ForegroundColor Cyan
Write-Host ""

# Navegar al directorio del API
Set-Location "c:\Nadia\.NET\Proyecto.NET\src\Web.Api"

# Configurar entorno de desarrollo
$env:ASPNETCORE_ENVIRONMENT = "Development"

Write-Host "📍 Backend URL: http://192.168.1.23:5000" -ForegroundColor Green
Write-Host "📧 Credenciales de prueba:" -ForegroundColor Yellow
Write-Host "   Email: admin1@backoffice.com" -ForegroundColor White
Write-Host "   Password: Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "Presiona Ctrl+C para detener el backend" -ForegroundColor Gray
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Iniciar el backend
dotnet run --no-build
