variable "aws_region" {
  description = "AWS region donde desplegar la infraestructura"
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Nombre del proyecto"
  type        = string
  default     = "proyectonet"
}

variable "environment" {
  description = "Ambiente de despliegue"
  type        = string
  default     = "production"
}

# VPC Configuration
variable "vpc_cidr" {
  description = "CIDR block para la VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "public_subnet_cidrs" {
  description = "CIDR blocks para subnets públicas"
  type        = list(string)
  default     = ["10.0.1.0/24", "10.0.2.0/24"]
}

variable "private_subnet_cidrs" {
  description = "CIDR blocks para subnets privadas"
  type        = list(string)
  default     = ["10.0.11.0/24", "10.0.12.0/24"]
}

# Database Configuration
variable "db_username" {
  description = "Usuario master de la base de datos"
  type        = string
  default     = "dbadmin"
  sensitive   = true
}

variable "db_password" {
  description = "Password master de la base de datos"
  type        = string
  sensitive   = true
}

variable "db_name" {
  description = "Nombre de la base de datos"
  type        = string
  default     = "ProyectoNetDb"
}

variable "db_instance_class" {
  description = "Clase de instancia RDS"
  type        = string
  default     = "db.t3.micro"
}

# ECS Configuration - API
variable "api_cpu" {
  description = "CPU para el contenedor API (1024 = 1 vCPU)"
  type        = string
  default     = "256"
}

variable "api_memory" {
  description = "Memoria para el contenedor API en MB"
  type        = string
  default     = "512"
}

variable "api_desired_count" {
  description = "Número deseado de tareas API"
  type        = number
  default     = 1
}

# ECS Configuration - BackOffice
variable "backoffice_cpu" {
  description = "CPU para el contenedor BackOffice (1024 = 1 vCPU)"
  type        = string
  default     = "256"
}

variable "backoffice_memory" {
  description = "Memoria para el contenedor BackOffice en MB"
  type        = string
  default     = "512"
}

variable "backoffice_desired_count" {
  description = "Número deseado de tareas BackOffice"
  type        = number
  default     = 1
}

# JWT Configuration
variable "jwt_secret" {
  description = "Secret key para firmar JWT tokens (mínimo 32 caracteres)"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "Emisor de los JWT tokens"
  type        = string
  default     = "ProyectoNet"
}

variable "jwt_audience" {
  description = "Audiencia de los JWT tokens"
  type        = string
  default     = "ProyectoNetClients"
}

variable "jwt_lifetime_minutes" {
  description = "Tiempo de vida de los tokens JWT en minutos"
  type        = number
  default     = 60
}

