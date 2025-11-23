# Script para abrir los logs de CloudWatch en el navegador
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "CloudWatch Logs - Quick Access" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$region = "us-east-1"
$projectName = "proyectonet"

Write-Host "¿Qué logs deseas ver?" -ForegroundColor Yellow
Write-Host ""
Write-Host "  [1] API Logs (en consola)" -ForegroundColor White
Write-Host "  [2] BackOffice Logs (en consola)" -ForegroundColor White
Write-Host "  [3] FrontOffice Logs (en consola)" -ForegroundColor White
Write-Host "  [4] API Logs (abrir en navegador)" -ForegroundColor Green
Write-Host "  [5] BackOffice Logs (abrir en navegador)" -ForegroundColor Green
Write-Host "  [6] FrontOffice Logs (abrir en navegador)" -ForegroundColor Green
Write-Host "  [7] Todos (en consola, separados)" -ForegroundColor White
Write-Host "  [8] Ver logs con filtro personalizado" -ForegroundColor Cyan
Write-Host ""

$option = Read-Host "Opcion (1-8)"

switch ($option) {
    "1" {
        Write-Host ""
        Write-Host "Mostrando logs del API (últimos 5 minutos)..." -ForegroundColor Green
        Write-Host "Presiona Ctrl+C para detener" -ForegroundColor Gray
        Write-Host ""
        aws logs tail /ecs/${projectName}-api --follow --since 5m --region $region
    }
    "2" {
        Write-Host ""
        Write-Host "Mostrando logs del BackOffice (últimos 5 minutos)..." -ForegroundColor Green
        Write-Host "Presiona Ctrl+C para detener" -ForegroundColor Gray
        Write-Host ""
        aws logs tail /ecs/${projectName}-backoffice --follow --since 5m --region $region
    }
    "3" {
        Write-Host ""
        Write-Host "Mostrando logs del FrontOffice (últimos 5 minutos)..." -ForegroundColor Green
        Write-Host "Presiona Ctrl+C para detener" -ForegroundColor Gray
        Write-Host ""
        aws logs tail /ecs/${projectName}-frontoffice --follow --since 5m --region $region
    }
    "4" {
        $url = "https://$region.console.aws.amazon.com/cloudwatch/home?region=$region#logsV2:log-groups/log-group/`$252Fecs`$252F${projectName}-api"
        Write-Host ""
        Write-Host "Abriendo logs del API en el navegador..." -ForegroundColor Green
        Start-Process $url
    }
    "5" {
        $url = "https://$region.console.aws.amazon.com/cloudwatch/home?region=$region#logsV2:log-groups/log-group/`$252Fecs`$252F${projectName}-backoffice"
        Write-Host ""
        Write-Host "Abriendo logs del BackOffice en el navegador..." -ForegroundColor Green
        Start-Process $url
    }
    "6" {
        $url = "https://$region.console.aws.amazon.com/cloudwatch/home?region=$region#logsV2:log-groups/log-group/`$252Fecs`$252F${projectName}-frontoffice"
        Write-Host ""
        Write-Host "Abriendo logs del FrontOffice en el navegador..." -ForegroundColor Green
        Start-Process $url
    }
    "7" {
        Write-Host ""
        Write-Host "=== API LOGS ===" -ForegroundColor Cyan
        aws logs tail /ecs/${projectName}-api --since 5m --region $region
        Write-Host ""
        Write-Host "=== BACKOFFICE LOGS ===" -ForegroundColor Cyan
        aws logs tail /ecs/${projectName}-backoffice --since 5m --region $region
        Write-Host ""
        Write-Host "=== FRONTOFFICE LOGS ===" -ForegroundColor Cyan
        aws logs tail /ecs/${projectName}-frontoffice --since 5m --region $region
    }
    "8" {
        Write-Host ""
        Write-Host "¿De qué servicio?" -ForegroundColor Yellow
        Write-Host "  [1] API" -ForegroundColor White
        Write-Host "  [2] BackOffice" -ForegroundColor White
        Write-Host "  [3] FrontOffice" -ForegroundColor White
        $service = Read-Host "Opcion (1/2/3)"

        $logGroup = switch ($service) {
            "1" { "/ecs/${projectName}-api" }
            "2" { "/ecs/${projectName}-backoffice" }
            "3" { "/ecs/${projectName}-frontoffice" }
        }

        Write-Host ""
        Write-Host "Filtros comunes:" -ForegroundColor Yellow
        Write-Host "  [1] Solo errores (ERROR)" -ForegroundColor Red
        Write-Host "  [2] Solo warnings (WARN)" -ForegroundColor Yellow
        Write-Host "  [3] Excepciones (Exception)" -ForegroundColor Red
        Write-Host "  [4] Health checks" -ForegroundColor Green
        Write-Host "  [5] Filtro personalizado" -ForegroundColor Cyan
        $filter = Read-Host "Opcion (1-5)"

        $filterPattern = switch ($filter) {
            "1" { "ERROR" }
            "2" { "WARN" }
            "3" { "Exception" }
            "4" { "health" }
            "5" {
                Write-Host "Ingresa el patrón de búsqueda:" -ForegroundColor Cyan
                Read-Host
            }
        }

        Write-Host ""
        Write-Host "Mostrando logs con filtro: $filterPattern" -ForegroundColor Green
        Write-Host "Presiona Ctrl+C para detener" -ForegroundColor Gray
        Write-Host ""
        aws logs tail $logGroup --follow --filter-pattern $filterPattern --region $region
    }
    default {
        Write-Host "Opción inválida" -ForegroundColor Red
    }
}

Write-Host ""
