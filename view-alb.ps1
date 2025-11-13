# Script para abrir el balanceador de carga en la consola de AWS
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "AWS Load Balancer - Quick Access" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$region = "us-east-1"
$projectName = "proyectonet"

Write-Host "¿Qué deseas ver?" -ForegroundColor Yellow
Write-Host ""
Write-Host "  [1] Balanceador de Carga (ALB)" -ForegroundColor Green
Write-Host "  [2] Target Groups (grupos de destinos)" -ForegroundColor White
Write-Host "  [3] Métricas y Monitoreo del ALB" -ForegroundColor Cyan
Write-Host "  [4] Logs de Acceso del ALB" -ForegroundColor White
Write-Host "  [5] Todo (abrir múltiples pestañas)" -ForegroundColor Yellow
Write-Host ""

$option = Read-Host "Opcion (1-5)"

Write-Host ""
Write-Host "Obteniendo información del ALB..." -ForegroundColor Gray

# Obtener el ARN del ALB
try {
    $albInfo = aws elbv2 describe-load-balancers --region $region --query "LoadBalancers[?contains(LoadBalancerName,'$projectName')].[LoadBalancerArn,DNSName]" --output json | ConvertFrom-Json

    if ($albInfo) {
        $albArn = $albInfo[0][0]
        $albDns = $albInfo[0][1]
        $albName = $albArn.Split('/')[-3]

        Write-Host "✅ ALB encontrado: $albName" -ForegroundColor Green
        Write-Host "   DNS: $albDns" -ForegroundColor Gray
        Write-Host ""
    } else {
        Write-Host "⚠️  No se encontró el ALB. Usando URLs genéricas..." -ForegroundColor Yellow
        Write-Host ""
    }
} catch {
    Write-Host "⚠️  Error al obtener info del ALB. Usando URLs genéricas..." -ForegroundColor Yellow
    Write-Host ""
}

switch ($option) {
    "1" {
        Write-Host "Abriendo consola del Load Balancer..." -ForegroundColor Green
        $url = "https://console.aws.amazon.com/ec2/home?region=$region#LoadBalancers:"
        Start-Process $url

        Write-Host ""
        Write-Host "En la consola, busca: $projectName-alb" -ForegroundColor Cyan
    }
    "2" {
        Write-Host "Abriendo Target Groups..." -ForegroundColor Green
        $url = "https://console.aws.amazon.com/ec2/home?region=$region#TargetGroups:"
        Start-Process $url

        Write-Host ""
        Write-Host "En la consola, busca:" -ForegroundColor Cyan
        Write-Host "  - $projectName-api-tg" -ForegroundColor White
        Write-Host "  - $projectName-backoffice-tg" -ForegroundColor White
    }
    "3" {
        Write-Host "Abriendo métricas de CloudWatch..." -ForegroundColor Green
        $url = "https://console.aws.amazon.com/cloudwatch/home?region=$region#metricsV2:graph=~();namespace=AWS/ApplicationELB"
        Start-Process $url

        Write-Host ""
        Write-Host "Métricas útiles a buscar:" -ForegroundColor Cyan
        Write-Host "  - RequestCount (número de peticiones)" -ForegroundColor White
        Write-Host "  - TargetResponseTime (tiempo de respuesta)" -ForegroundColor White
        Write-Host "  - HealthyHostCount (instancias saludables)" -ForegroundColor White
        Write-Host "  - UnHealthyHostCount (instancias con problemas)" -ForegroundColor White
    }
    "4" {
        Write-Host "Abriendo logs del ALB..." -ForegroundColor Green
        $url = "https://console.aws.amazon.com/cloudwatch/home?region=$region#logsV2:logs-insights"
        Start-Process $url

        Write-Host ""
        Write-Host "Los logs de acceso del ALB se guardan en S3 (si están habilitados)" -ForegroundColor Yellow
        Write-Host "Para ver tráfico en tiempo real, usa los logs de las aplicaciones:" -ForegroundColor Cyan
        Write-Host "  - /ecs/$projectName-api" -ForegroundColor White
        Write-Host "  - /ecs/$projectName-backoffice" -ForegroundColor White
    }
    "5" {
        Write-Host "Abriendo múltiples pestañas..." -ForegroundColor Green

        Start-Sleep -Milliseconds 500
        Start-Process "https://console.aws.amazon.com/ec2/home?region=$region#LoadBalancers:"

        Start-Sleep -Milliseconds 500
        Start-Process "https://console.aws.amazon.com/ec2/home?region=$region#TargetGroups:"

        Start-Sleep -Milliseconds 500
        Start-Process "https://console.aws.amazon.com/cloudwatch/home?region=$region#metricsV2:graph=~();namespace=AWS/ApplicationELB"

        Write-Host ""
        Write-Host "✅ Se abrieron 3 pestañas:" -ForegroundColor Green
        Write-Host "   1. Load Balancers" -ForegroundColor White
        Write-Host "   2. Target Groups" -ForegroundColor White
        Write-Host "   3. Métricas CloudWatch" -ForegroundColor White
    }
    default {
        Write-Host "Opción inválida" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Información de tu ALB:" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Nombre: $projectName-alb" -ForegroundColor White
Write-Host "URL: http://$albDns" -ForegroundColor Green
Write-Host ""
Write-Host "Target Groups:" -ForegroundColor Yellow
Write-Host "  - API: $projectName-api-tg" -ForegroundColor White
Write-Host "  - BackOffice: $projectName-backoffice-tg" -ForegroundColor White
Write-Host ""
Write-Host "Para ver estado en tiempo real desde CLI:" -ForegroundColor Cyan
Write-Host '  aws elbv2 describe-target-health --target-group-arn <ARN>' -ForegroundColor Gray
Write-Host ""

