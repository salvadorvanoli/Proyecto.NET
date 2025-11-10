# 🚀 Script de Despliegue Rápido para AWS

# Este script automatiza el despliegue completo desde cero en AWS Learner Lab

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Despliegue Completo en AWS" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que estamos en el directorio correcto
if (-not (Test-Path "terraform")) {
    Write-Host "Error: Debes ejecutar este script desde el directorio raíz del proyecto" -ForegroundColor Red
    exit 1
}

# Paso 1: Verificar credenciales
Write-Host "[1/4] Verificando credenciales de AWS..." -ForegroundColor Yellow
try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    Write-Host "OK - Credenciales validas (Account: $($identity.Account))" -ForegroundColor Green
} catch {
    Write-Host "Error: Credenciales invalidas o expiradas" -ForegroundColor Red
    Write-Host "Ejecuta: .\scripts\validate-env.ps1 para configurar credenciales" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Paso 2: Verificar configuración de Terraform
Write-Host "[2/4] Verificando configuracion de Terraform..." -ForegroundColor Yellow
if (-not (Test-Path "terraform\terraform.tfvars")) {
    Write-Host "Advertencia: No existe terraform.tfvars" -ForegroundColor Yellow
    Write-Host "Copiando desde terraform.tfvars.example..." -ForegroundColor Yellow
    Copy-Item "terraform\terraform.tfvars.example" "terraform\terraform.tfvars"
    Write-Host ""
    Write-Host "IMPORTANTE: Edita terraform\terraform.tfvars antes de continuar" -ForegroundColor Red
    Write-Host "Especialmente la contraseña de la base de datos (db_password)" -ForegroundColor Red
    Write-Host ""
    $continue = Read-Host "¿Ya editaste el archivo? (S/N)"
    if ($continue -ne "S" -and $continue -ne "s") {
        Write-Host "Cancelando despliegue. Edita el archivo y vuelve a ejecutar este script." -ForegroundColor Yellow
        exit 0
    }
}
Write-Host "OK - Archivo de configuracion encontrado" -ForegroundColor Green
Write-Host ""

# Paso 3: Desplegar infraestructura con Terraform
Write-Host "[3/4] Desplegando infraestructura con Terraform..." -ForegroundColor Yellow
Write-Host "Esto tomara 10-15 minutos..." -ForegroundColor Gray
Write-Host ""

Set-Location terraform

# Inicializar Terraform si es necesario
if (-not (Test-Path ".terraform")) {
    Write-Host "Inicializando Terraform..." -ForegroundColor Yellow
    terraform init
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al inicializar Terraform" -ForegroundColor Red
        Set-Location ..
        exit 1
    }
}

# Aplicar Terraform
Write-Host "Aplicando configuracion de Terraform..." -ForegroundColor Yellow
terraform apply -auto-approve

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al aplicar Terraform" -ForegroundColor Red
    Set-Location ..
    exit 1
}

# Obtener outputs
$albUrl = terraform output -raw alb_url
$apiUrl = terraform output -raw api_url

Set-Location ..

Write-Host ""
Write-Host "OK - Infraestructura desplegada exitosamente" -ForegroundColor Green
Write-Host "URL del Load Balancer: $albUrl" -ForegroundColor White
Write-Host ""

# Paso 4: Construir y subir imagenes Docker
Write-Host "[4/4] Construyendo y subiendo imagenes Docker..." -ForegroundColor Yellow
Write-Host ""

# Preguntar sobre RECREATE_DATABASE
Write-Host "¿Es el PRIMER despliegue? (se creara la base de datos)" -ForegroundColor Cyan
Write-Host "  [S] Si - Es el primer despliegue" -ForegroundColor Green
Write-Host "  [N] No - La base de datos ya existe" -ForegroundColor Yellow
$firstDeploy = Read-Host "Opcion (S/N)"
$recreateDb = if ($firstDeploy -eq "S" -or $firstDeploy -eq "s") { "S" } else { "N" }

Write-Host ""
Write-Host "Ejecutando script de subida a ECR..." -ForegroundColor Yellow
Write-Host ""

# Crear un archivo temporal con las respuestas
$answersFile = [System.IO.Path]::GetTempFileName()
"$recreateDb`n3`n" | Out-File -FilePath $answersFile -Encoding ASCII

# Ejecutar el script de subida (pasándole las respuestas)
Get-Content $answersFile | .\upload-to-ecr.ps1

# Limpiar archivo temporal
Remove-Item $answersFile -Force

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Hubo un error al subir las imagenes" -ForegroundColor Red
    Write-Host "Puedes intentar ejecutar manualmente: .\upload-to-ecr.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Despliegue Completado!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Tu aplicacion estara disponible en:" -ForegroundColor Yellow
Write-Host "  BackOffice: $albUrl" -ForegroundColor White
Write-Host "  API: $apiUrl" -ForegroundColor White
Write-Host ""

if ($recreateDb -eq "S" -or $recreateDb -eq "s") {
    Write-Host "Credenciales de acceso:" -ForegroundColor Yellow
    Write-Host "  Email: admin1@backoffice.com" -ForegroundColor White
    Write-Host "  Password: Admin123!" -ForegroundColor White
    Write-Host ""
}

Write-Host "Nota: Puede tomar 2-3 minutos hasta que los contenedores esten listos." -ForegroundColor Gray
Write-Host ""
Write-Host "Para verificar el estado:" -ForegroundColor Yellow
Write-Host "  aws ecs describe-services --cluster proyectonet-cluster --services proyectonet-api-service proyectonet-backoffice-service --region us-east-1" -ForegroundColor Gray
Write-Host ""
Write-Host "Para ver logs:" -ForegroundColor Yellow
Write-Host "  aws logs tail /ecs/proyectonet-api --since 5m --region us-east-1 --format short" -ForegroundColor Gray
Write-Host "  aws logs tail /ecs/proyectonet-backoffice --since 5m --region us-east-1 --format short" -ForegroundColor Gray
Write-Host ""

