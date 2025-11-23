output "alb_dns_name" {
  description = "DNS del Application Load Balancer"
  value       = aws_lb.main.dns_name
}

output "alb_url" {
  description = "URL del Application Load Balancer"
  value       = "http://${aws_lb.main.dns_name}"
}

output "backoffice_url" {
  description = "URL del BackOffice"
  value       = "http://${aws_lb.main.dns_name}"
}

output "frontoffice_url" {
  description = "URL del FrontOffice"
  value       = "http://${aws_lb.main.dns_name}/frontoffice"
}

output "api_url" {
  description = "URL de la API"
  value       = "http://${aws_lb.main.dns_name}/api"
}

output "rds_endpoint" {
  description = "Endpoint de la base de datos RDS"
  value       = aws_db_instance.sqlserver.endpoint
}

output "ecr_repository_backoffice_url" {
  description = "URL del repositorio ECR para BackOffice"
  value       = aws_ecr_repository.backoffice.repository_url
}

output "ecr_repository_api_url" {
  description = "URL del repositorio ECR para API"
  value       = aws_ecr_repository.api.repository_url
}

output "ecr_repository_frontoffice_url" {
  description = "URL del repositorio ECR para FrontOffice"
  value       = aws_ecr_repository.frontoffice.repository_url
}

output "ecs_cluster_name" {
  description = "Nombre del cluster ECS"
  value       = aws_ecs_cluster.main.name
}

output "vpc_id" {
  description = "ID de la VPC"
  value       = aws_vpc.main.id
}

# Security Configuration Outputs
output "cors_configuration" {
  description = "Configuración de CORS aplicada"
  value = {
    specified_origins = var.cors_allowed_origins
    effective_origin  = length(var.cors_allowed_origins) > 0 ? join(", ", var.cors_allowed_origins) : "http://${aws_lb.main.dns_name}"
    note             = "Si no especificas cors_allowed_origins, se usa automáticamente el DNS del ALB"
  }
}

output "jwt_configuration" {
  description = "Configuración de JWT (sin secretos)"
  value = {
    issuer           = var.jwt_issuer
    audience         = var.jwt_audience
    lifetime_minutes = var.jwt_lifetime_minutes
  }
  sensitive = false
}

output "redis_endpoint" {
  description = "Endpoint de ElastiCache Redis"
  value       = var.redis_enabled ? aws_elasticache_cluster.redis[0].cache_nodes[0].address : "Redis deshabilitado"
}

output "redis_configuration" {
  description = "Configuración de Redis/ElastiCache"
  value = {
    enabled              = var.redis_enabled
    endpoint             = var.redis_enabled ? "${aws_elasticache_cluster.redis[0].cache_nodes[0].address}:6379" : "N/A"
    node_type            = var.redis_node_type
    default_ttl_minutes  = var.redis_default_ttl_minutes
    note                 = var.redis_enabled ? "ElastiCache habilitado y configurado" : "ElastiCache deshabilitado - usando memoria local"
  }
}

output "security_notes" {
  description = "Notas importantes de seguridad"
  value = <<-EOT
    IMPORTANTE - SEGURIDAD:

    1. JWT Secret: Configurado vía variable sensible (no visible en outputs)
    2. DB Password: Configurado vía variable sensible (no visible en outputs)
    3. Redis: ${var.redis_enabled ? "ElastiCache habilitado" : "Deshabilitado (usando memoria local)"}
    4. HTTPS: Actualmente usando HTTP. Para producción:
       - Solicita certificado SSL/TLS en AWS Certificate Manager
       - Configura listener HTTPS en puerto 443
       - Habilita redirección HTTP → HTTPS
    5. CORS: Configurado para: ${length(var.cors_allowed_origins) > 0 ? join(", ", var.cors_allowed_origins) : "ALB DNS (auto)"}
    6. Rate Limiting: Configurado en la aplicación (5 login/min, 200 req/min)
    7. Security Headers: Configurados en la aplicación (HSTS, CSP, etc.)

    Para conectar BackOffice/FrontOffice a la API, usa:
       API URL: http://${aws_lb.main.dns_name}
       BackOffice URL: http://${aws_lb.main.dns_name}
  EOT
}
