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

output "ecs_cluster_name" {
  description = "Nombre del cluster ECS"
  value       = aws_ecs_cluster.main.name
}

output "vpc_id" {
  description = "ID de la VPC"
  value       = aws_vpc.main.id
}
