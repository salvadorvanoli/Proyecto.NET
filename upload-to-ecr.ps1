# Script PowerShell para login en ECR y subida de imágenes
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Subida de imagenes a AWS ECR" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Verificar credenciales y obtener Account ID
Write-Host "[1/9] Verificando credenciales de AWS..." -ForegroundColor Yellow
try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    $accountId = $identity.Account
    $region = "us-east-1"
    Write-Host "OK - Credenciales validas (Account: $accountId)" -ForegroundColor Green
} catch {
    Write-Host "Error: Credenciales invalidas o expiradas" -ForegroundColor Red
    Write-Host "Configura las credenciales del Learner Lab primero" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Preguntar sobre configuraciones de despliegue
Write-Host "[2/9] Configuracion de despliegue..." -ForegroundColor Yellow
Write-Host ""
Write-Host "¿Deseas RECREAR la base de datos? (se eliminaran todos los datos existentes)" -ForegroundColor Cyan
Write-Host "  [S] Si - Recrear base de datos (CUIDADO: elimina datos)" -ForegroundColor Yellow
Write-Host "  [N] No - Usar base de datos existente (recomendado)" -ForegroundColor Green
$recreateDb = Read-Host "Opcion (S/N)"
$recreateDbValue = if ($recreateDb -eq "S" -or $recreateDb -eq "s") { "true" } else { "false" }

Write-Host ""
Write-Host "¿Que servicios deseas desplegar?" -ForegroundColor Cyan
Write-Host "  [1] Solo API" -ForegroundColor White
Write-Host "  [2] Solo BackOffice" -ForegroundColor White
Write-Host "  [3] Ambos (API + BackOffice)" -ForegroundColor Green
$deployOption = Read-Host "Opcion (1/2/3)"

$deployApi = $deployOption -eq "1" -or $deployOption -eq "3"
$deployBackOffice = $deployOption -eq "2" -or $deployOption -eq "3"

Write-Host ""
Write-Host "Configuracion seleccionada:" -ForegroundColor Cyan
Write-Host "  - Recrear DB: $recreateDbValue" -ForegroundColor White
Write-Host "  - Desplegar API: $deployApi" -ForegroundColor White
Write-Host "  - Desplegar BackOffice: $deployBackOffice" -ForegroundColor White
Write-Host ""

# Hacer login en ECR usando el método recomendado por AWS
Write-Host "[3/9] Haciendo login en ECR..." -ForegroundColor Yellow
try {
    # Usar el método recomendado por AWS CLI v2
    $loginCommand = "aws ecr get-login-password --region $region | docker login --username AWS --password-stdin $accountId.dkr.ecr.$region.amazonaws.com"

    Invoke-Expression $loginCommand

    if ($LASTEXITCODE -ne 0) {
        throw "Login failed with exit code $LASTEXITCODE"
    }

    Write-Host "OK - Login exitoso en ECR" -ForegroundColor Green
} catch {
    Write-Host "Error al hacer login en ECR" -ForegroundColor Red
    Write-Host "Detalles: $_" -ForegroundColor Gray
    Write-Host "" -ForegroundColor Gray
    Write-Host "Intentando metodo alternativo..." -ForegroundColor Yellow

    try {
        # Método alternativo para PowerShell
        $password = aws ecr get-login-password --region $region
        $password | docker login --username AWS --password-stdin "$accountId.dkr.ecr.$region.amazonaws.com"

        if ($LASTEXITCODE -ne 0) {
            throw "Login alternativo failed"
        }

        Write-Host "OK - Login exitoso con metodo alternativo" -ForegroundColor Green
    } catch {
        Write-Host "Error: No se pudo hacer login en ECR" -ForegroundColor Red
        Write-Host "Verifica que Docker este corriendo y que tengas permisos de ECR" -ForegroundColor Yellow
        exit 1
    }
}
Write-Host ""

# Obtener nombres de repositorios ECR
Write-Host "[4/9] Detectando repositorios ECR..." -ForegroundColor Yellow
try {
    $repositories = aws ecr describe-repositories --region $region --output json | ConvertFrom-Json
    $apiRepo = ($repositories.repositories | Where-Object { $_.repositoryName -like "*api*" -and $_.repositoryName -notlike "*backoffice*" }).repositoryName
    $backofficeRepo = ($repositories.repositories | Where-Object { $_.repositoryName -like "*backoffice*" }).repositoryName

    if ([string]::IsNullOrEmpty($apiRepo)) { $apiRepo = "proyectonet-api" }
    if ([string]::IsNullOrEmpty($backofficeRepo)) { $backofficeRepo = "proyectonet-backoffice" }

    Write-Host "OK - Repositorios detectados:" -ForegroundColor Green
    Write-Host "  API: $apiRepo" -ForegroundColor White
    Write-Host "  BackOffice: $backofficeRepo" -ForegroundColor White
} catch {
    Write-Host "Advertencia: No se pudieron detectar repositorios, usando nombres por defecto" -ForegroundColor Yellow
    $apiRepo = "proyectonet-api"
    $backofficeRepo = "proyectonet-backoffice"
}
Write-Host ""

# Desplegar BackOffice si es necesario
if ($deployBackOffice) {
    Write-Host "[5/9] Construyendo imagen BackOffice..." -ForegroundColor Yellow
    docker build -t $backofficeRepo --target final-backoffice -f Dockerfile .
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al construir BackOffice" -ForegroundColor Red
        exit 1
    }
    Write-Host "OK - BackOffice construido" -ForegroundColor Green
    Write-Host ""

    Write-Host "[6/9] Etiquetando y subiendo BackOffice a ECR..." -ForegroundColor Yellow
    Write-Host "(Esto puede tomar varios minutos dependiendo de tu conexion)" -ForegroundColor Gray
    docker tag "${backofficeRepo}:latest" "${accountId}.dkr.ecr.${region}.amazonaws.com/${backofficeRepo}:latest"
    docker push "${accountId}.dkr.ecr.${region}.amazonaws.com/${backofficeRepo}:latest"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al subir BackOffice" -ForegroundColor Red
        exit 1
    }
    Write-Host "OK - BackOffice subido exitosamente" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "[5/9] Omitiendo BackOffice..." -ForegroundColor Gray
    Write-Host "[6/9] Omitiendo BackOffice..." -ForegroundColor Gray
    Write-Host ""
}

# Desplegar API si es necesario
if ($deployApi) {
    Write-Host "[7/9] Construyendo imagen API..." -ForegroundColor Yellow
    Write-Host "(Esto puede tomar varios minutos)" -ForegroundColor Gray

    # Crear archivo .env temporal con la configuración
    $envContent = "RECREATE_DATABASE=$recreateDbValue"
    $envFile = ".env.build"
    $envContent | Out-File -FilePath $envFile -Encoding UTF8 -NoNewline

    docker build -t $apiRepo --target final-api --build-arg RECREATE_DATABASE=$recreateDbValue -f Dockerfile .

    # Limpiar archivo temporal
    if (Test-Path $envFile) {
        Remove-Item $envFile -Force
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al construir API" -ForegroundColor Red
        exit 1
    }
    Write-Host "OK - API construida" -ForegroundColor Green
    Write-Host ""

    Write-Host "[8/9] Etiquetando y subiendo API a ECR..." -ForegroundColor Yellow
    docker tag "${apiRepo}:latest" "${accountId}.dkr.ecr.${region}.amazonaws.com/${apiRepo}:latest"
    docker push "${accountId}.dkr.ecr.${region}.amazonaws.com/${apiRepo}:latest"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al subir API" -ForegroundColor Red
        exit 1
    }
    Write-Host "OK - API subida exitosamente" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "[7/9] Omitiendo API..." -ForegroundColor Gray
    Write-Host "[8/9] Omitiendo API..." -ForegroundColor Gray
    Write-Host ""
}

# Actualizar servicios ECS
Write-Host "[9/9] Actualizando servicios ECS..." -ForegroundColor Yellow
try {
    # Detectar cluster ECS
    $clusters = aws ecs list-clusters --region $region --output json | ConvertFrom-Json
    $clusterArn = ($clusters.clusterArns | Where-Object { $_ -like "*proyectonet*" -or $_ -like "*cluster*" } | Select-Object -First 1)

    if ([string]::IsNullOrEmpty($clusterArn)) {
        Write-Host "Advertencia: No se encontro cluster ECS" -ForegroundColor Yellow
        Write-Host "Omitiendo actualizacion de servicios" -ForegroundColor Gray
    } else {
        $clusterName = $clusterArn.Split('/')[-1]
        Write-Host "Cluster detectado: $clusterName" -ForegroundColor White

        # Obtener servicios del cluster
        $services = aws ecs list-services --cluster $clusterName --region $region --output json | ConvertFrom-Json

        if ($deployBackOffice) {
            $backofficeService = ($services.serviceArns | Where-Object { $_ -like "*backoffice*" } | Select-Object -First 1)
            if (-not [string]::IsNullOrEmpty($backofficeService)) {
                $serviceName = $backofficeService.Split('/')[-1]
                Write-Host "Actualizando servicio BackOffice: $serviceName" -ForegroundColor White
                aws ecs update-service --cluster $clusterName --service $serviceName --force-new-deployment --region $region | Out-Null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "OK - Servicio BackOffice actualizado" -ForegroundColor Green
                }
            }
        }

        if ($deployApi) {
            $apiService = ($services.serviceArns | Where-Object { $_ -like "*api*" -and $_ -notlike "*backoffice*" } | Select-Object -First 1)
            if (-not [string]::IsNullOrEmpty($apiService)) {
                $serviceName = $apiService.Split('/')[-1]
                Write-Host "Actualizando servicio API: $serviceName" -ForegroundColor White
                aws ecs update-service --cluster $clusterName --service $serviceName --force-new-deployment --region $region | Out-Null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "OK - Servicio API actualizado" -ForegroundColor Green
                }
            }
        }
    }
} catch {
    Write-Host "Advertencia: Error al actualizar servicios ECS" -ForegroundColor Yellow
    Write-Host "Detalles: $_" -ForegroundColor Gray
}
Write-Host ""

# Obtener URL del ALB
Write-Host "Obteniendo URL del Load Balancer..." -ForegroundColor Yellow
try {
    if (Test-Path "terraform") {
        Set-Location terraform
        $albUrl = terraform output -raw alb_url 2>$null
        Set-Location ..
    }

    if ([string]::IsNullOrEmpty($albUrl)) {
        $albs = aws elbv2 describe-load-balancers --region $region --output json | ConvertFrom-Json
        $alb = ($albs.LoadBalancers | Where-Object { $_.LoadBalancerName -like "*proyectonet*" } | Select-Object -First 1)
        if ($alb) {
            $albUrl = "http://$($alb.DNSName)"
        }
    }
} catch {
    Write-Host "No se pudo obtener URL del Load Balancer" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Despliegue completado exitosamente!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

if (-not [string]::IsNullOrEmpty($albUrl)) {
    Write-Host "Tu aplicacion estara disponible en:" -ForegroundColor Yellow
    Write-Host "  $albUrl" -ForegroundColor White
    if ($deployApi) {
        Write-Host "  API: $albUrl/api" -ForegroundColor White
    }
    if ($deployBackOffice) {
        Write-Host "  BackOffice: $albUrl/backoffice" -ForegroundColor White
    }
    Write-Host ""
}

if ($recreateDbValue -eq "true") {
    Write-Host "Base de datos recreada con credenciales por defecto:" -ForegroundColor Yellow
    Write-Host "  Email: admin1@backoffice.com" -ForegroundColor White
    Write-Host "  Password: Admin123!" -ForegroundColor White
    Write-Host ""
}

Write-Host "Nota: Puede tomar 2-3 minutos hasta que los contenedores esten listos." -ForegroundColor Gray
Write-Host "Revisa el estado en: https://console.aws.amazon.com/ecs/" -ForegroundColor Gray
Write-Host ""
