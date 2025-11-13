# Script para crear un Dashboard de CloudWatch con métricas de observabilidad
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "CloudWatch Dashboard - Observabilidad" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$region = "us-east-1"
$projectName = "proyectonet"
$dashboardName = "${projectName}-observability-dashboard"

Write-Host "Este script creará un dashboard de CloudWatch con:" -ForegroundColor Yellow
Write-Host "  ✅ Tiempo medio de respuesta" -ForegroundColor White
Write-Host "  ✅ Percentiles de latencia (P50, P95, P99)" -ForegroundColor White
Write-Host "  ✅ Tasa de errores" -ForegroundColor White
Write-Host "  ✅ Peticiones por minuto" -ForegroundColor White
Write-Host "  ✅ Estado de instancias (Healthy/Unhealthy)" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "¿Deseas continuar? (S/N)"
if ($confirm -ne "S" -and $confirm -ne "s") {
    Write-Host "Operación cancelada" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Obteniendo información de la infraestructura..." -ForegroundColor Gray

# Obtener ARNs de los recursos
try {
    $albInfo = aws elbv2 describe-load-balancers --region $region --query "LoadBalancers[?contains(LoadBalancerName,'$projectName')].[LoadBalancerArn,DNSName]" --output json | ConvertFrom-Json
    $albArn = $albInfo[0][0]
    $albFullName = $albArn.Split(':')[-1].Replace('loadbalancer/', '')

    $tgInfo = aws elbv2 describe-target-groups --region $region --query "TargetGroups[?contains(TargetGroupName,'$projectName')].[TargetGroupArn,TargetGroupName]" --output json | ConvertFrom-Json
    $apiTgArn = ($tgInfo | Where-Object { $_[1] -like "*api*" -and $_[1] -notlike "*backoffice*" })[0]
    $apiTgFullName = $apiTgArn.Split(':')[-1]

    $backofficeTgArn = ($tgInfo | Where-Object { $_[1] -like "*backoffice*" })[0]
    $backofficeTgFullName = $backofficeTgArn.Split(':')[-1]

    Write-Host "✅ Recursos encontrados:" -ForegroundColor Green
    Write-Host "   ALB: $albFullName" -ForegroundColor Gray
    Write-Host "   API TG: $apiTgFullName" -ForegroundColor Gray
    Write-Host "   BackOffice TG: $backofficeTgFullName" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "❌ Error al obtener información de recursos" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Gray
    exit 1
}

# Definir el dashboard JSON como objeto PowerShell con dimensiones específicas
$dashboardObject = @{
    widgets = @(
        @{
            type = "metric"
            properties = @{
                metrics = @(
                    ,@("AWS/ApplicationELB", "TargetResponseTime", "LoadBalancer", $albFullName, @{ stat = "Average"; label = "Avg Latency" })
                )
                view = "timeSeries"
                stacked = $false
                region = $region
                title = "Tiempo Medio de Respuesta (ms)"
                period = 300
                yAxis = @{
                    left = @{
                        label = "Milliseconds"
                        showUnits = $false
                    }
                }
            }
            width = 12
            height = 6
            x = 0
            y = 0
        },
        @{
            type = "metric"
            properties = @{
                metrics = @(
                    ,@("AWS/ApplicationELB", "TargetResponseTime", "LoadBalancer", $albFullName, @{ stat = "p50"; label = "P50" })
                    ,@("AWS/ApplicationELB", "TargetResponseTime", "LoadBalancer", $albFullName, @{ stat = "p95"; label = "P95" })
                    ,@("AWS/ApplicationELB", "TargetResponseTime", "LoadBalancer", $albFullName, @{ stat = "p99"; label = "P99" })
                )
                view = "timeSeries"
                stacked = $false
                region = $region
                title = "Percentiles de Latencia (P50, P95, P99)"
                period = 300
                yAxis = @{
                    left = @{
                        label = "Milliseconds"
                        showUnits = $false
                    }
                }
            }
            width = 12
            height = 6
            x = 12
            y = 0
        },
        @{
            type = "metric"
            properties = @{
                metrics = @(
                    ,@(@{ expression = "(m1 / m2) * 100"; label = "Error Rate %"; id = "e1" })
                    ,@("AWS/ApplicationELB", "HTTPCode_Target_5XX_Count", "LoadBalancer", $albFullName, @{ id = "m1"; visible = $false; stat = "Sum" })
                    ,@("AWS/ApplicationELB", "RequestCount", "LoadBalancer", $albFullName, @{ id = "m2"; visible = $false; stat = "Sum" })
                )
                view = "timeSeries"
                stacked = $false
                region = $region
                title = "Tasa de Errores (%)"
                period = 300
                yAxis = @{
                    left = @{
                        label = "Percentage"
                        showUnits = $false
                        min = 0
                    }
                }
            }
            width = 12
            height = 6
            x = 0
            y = 6
        },
        @{
            type = "metric"
            properties = @{
                metrics = @(
                    ,@("AWS/ApplicationELB", "RequestCount", "LoadBalancer", $albFullName, @{ stat = "Sum"; label = "Requests/min" })
                )
                view = "timeSeries"
                stacked = $false
                region = $region
                title = "Peticiones por Minuto"
                period = 60
                yAxis = @{
                    left = @{
                        label = "Count"
                        showUnits = $false
                        min = 0
                    }
                }
            }
            width = 12
            height = 6
            x = 12
            y = 6
        },
        @{
            type = "metric"
            properties = @{
                metrics = @(
                    ,@("AWS/ApplicationELB", "HealthyHostCount", "TargetGroup", $apiTgFullName, "LoadBalancer", $albFullName, @{ stat = "Average"; label = "API Healthy" })
                    ,@("AWS/ApplicationELB", "UnHealthyHostCount", "TargetGroup", $apiTgFullName, "LoadBalancer", $albFullName, @{ stat = "Average"; label = "API Unhealthy" })
                    ,@("AWS/ApplicationELB", "HealthyHostCount", "TargetGroup", $backofficeTgFullName, "LoadBalancer", $albFullName, @{ stat = "Average"; label = "BackOffice Healthy" })
                    ,@("AWS/ApplicationELB", "UnHealthyHostCount", "TargetGroup", $backofficeTgFullName, "LoadBalancer", $albFullName, @{ stat = "Average"; label = "BackOffice Unhealthy" })
                )
                view = "timeSeries"
                stacked = $false
                region = $region
                title = "Estado de Instancias"
                period = 60
                yAxis = @{
                    left = @{
                        label = "Count"
                        showUnits = $false
                        min = 0
                    }
                }
            }
            width = 12
            height = 6
            x = 0
            y = 12
        },
        @{
            type = "metric"
            properties = @{
                metrics = @(
                    ,@("AWS/ApplicationELB", "HTTPCode_Target_2XX_Count", "LoadBalancer", $albFullName, @{ stat = "Sum"; label = "2xx Success" })
                    ,@("AWS/ApplicationELB", "HTTPCode_Target_4XX_Count", "LoadBalancer", $albFullName, @{ stat = "Sum"; label = "4xx Client Error" })
                    ,@("AWS/ApplicationELB", "HTTPCode_Target_5XX_Count", "LoadBalancer", $albFullName, @{ stat = "Sum"; label = "5xx Server Error" })
                )
                view = "timeSeries"
                stacked = $false
                region = $region
                title = "Codigos de Respuesta HTTP"
                period = 300
                yAxis = @{
                    left = @{
                        label = "Count"
                        showUnits = $false
                        min = 0
                    }
                }
            }
            width = 12
            height = 6
            x = 12
            y = 12
        }
    )
}

# Convertir a JSON y guardar en archivo para pasar correctamente a AWS CLI
$dashboardJson = $dashboardObject | ConvertTo-Json -Depth 10 -Compress
$tempFile = Join-Path $env:TEMP "dashboard-cw.json"
$dashboardJson | Out-File -FilePath $tempFile -Encoding ASCII -NoNewline

Write-Host "Creando dashboard en CloudWatch..." -ForegroundColor Yellow

try {
    # Crear el dashboard usando AWS CLI con archivo temporal
    $result = aws cloudwatch put-dashboard `
        --dashboard-name $dashboardName `
        --dashboard-body "file://$tempFile" `
        --region $region 2>&1

    # Limpiar archivo temporal
    if (Test-Path $tempFile) {
        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    }

    # Verificar si hubo error
    if ($LASTEXITCODE -ne 0) {
        throw "Error al crear el dashboard: $result"
    }

    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host "✅ Dashboard creado exitosamente!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Nombre del dashboard: $dashboardName" -ForegroundColor White
    Write-Host ""
    Write-Host "Para ver el dashboard:" -ForegroundColor Cyan
    Write-Host "  URL: https://console.aws.amazon.com/cloudwatch/home?region=$region#dashboards:name=$dashboardName" -ForegroundColor White
    Write-Host ""

    $openBrowser = Read-Host "¿Deseas abrir el dashboard en el navegador? (S/N)"
    if ($openBrowser -eq "S" -or $openBrowser -eq "s") {
        Start-Process "https://console.aws.amazon.com/cloudwatch/home?region=$region#dashboards:name=$dashboardName"
    }

} catch {
    Write-Host ""
    Write-Host "❌ Error al crear el dashboard" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Gray

    # Limpiar archivo temporal en caso de error
    if (Test-Path $tempFile) {
        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    }

    exit 1
}

Write-Host ""
Write-Host "Indicadores incluidos en el dashboard:" -ForegroundColor Yellow
Write-Host "  ✅ Tiempo medio de respuesta (Average Latency)" -ForegroundColor White
Write-Host "  ✅ Percentiles P50, P95, P99" -ForegroundColor White
Write-Host "  ✅ Tasa de errores (%)" -ForegroundColor White
Write-Host "  ✅ Peticiones por minuto" -ForegroundColor White
Write-Host "  ✅ Estado de instancias (Healthy/Unhealthy)" -ForegroundColor White
Write-Host "  ✅ Códigos HTTP (2xx, 4xx, 5xx)" -ForegroundColor White
Write-Host ""
