# Script para probar el endpoint de login
Write-Host "🧪 Probando endpoint de login..." -ForegroundColor Cyan
Write-Host ""

$url = "http://192.168.1.23:5000/api/auth/login"

# Credenciales de prueba
$credentials = @{
    Email = "admin1@backoffice.com"
    Password = "Admin123!"
}

$json = $credentials | ConvertTo-Json

Write-Host "URL: $url" -ForegroundColor Yellow
Write-Host "Body: $json" -ForegroundColor Yellow
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri $url -Method POST -Body $json -ContentType "application/json" -ErrorAction Stop
    
    Write-Host "✅ Login exitoso!" -ForegroundColor Green
    Write-Host "UserId: $($response.userId)" -ForegroundColor White
    Write-Host "Email: $($response.email)" -ForegroundColor White
    Write-Host "FullName: $($response.fullName)" -ForegroundColor White
    Write-Host "Roles: $($response.roles -join ', ')" -ForegroundColor White
}
catch {
    Write-Host "❌ Error en el login:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    # Intentar obtener más detalles del error
    if ($_.ErrorDetails) {
        Write-Host "Detalles: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
    }
}
