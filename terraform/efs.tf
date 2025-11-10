# NOTA: EFS deshabilitado debido a restricciones de permisos en AWS Academy/voclabs
# El usuario no tiene permisos para: elasticfilesystem:DescribeMountTargets
#
# ALTERNATIVA IMPLEMENTADA:
# - Data Protection Keys se almacenan localmente en cada instancia
# - Para mantener sesiones consistentes, se usa Sticky Sessions en el ALB
# - Las cookies del BackOffice se configuran con el nombre del ALB para sticky sessions

# Si tienes permisos completos de AWS, descomenta el siguiente código:

/*
# EFS para Data Protection Keys (compartir entre instancias del BackOffice)
resource "aws_efs_file_system" "data_protection" {
  creation_token = "${var.project_name}-data-protection-keys"
  encrypted      = true

  lifecycle_policy {
    transition_to_ia = "AFTER_30_DAYS"
  }

  tags = {
    Name        = "${var.project_name}-data-protection-keys"
    Environment = var.environment
  }
}

# Mount targets en cada subnet para acceso desde ECS
resource "aws_efs_mount_target" "data_protection" {
  count           = 2
  file_system_id  = aws_efs_file_system.data_protection.id
  subnet_id       = aws_subnet.public[count.index].id
  security_groups = [aws_security_group.efs.id]
}

# Security Group para EFS
resource "aws_security_group" "efs" {
  name        = "${var.project_name}-efs-sg"
  description = "Security group for EFS file system"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port       = 2049
    to_port         = 2049
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs_tasks.id]
    description     = "Allow NFS from ECS tasks"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
    description = "Allow all outbound"
  }

  tags = {
    Name        = "${var.project_name}-efs-sg"
    Environment = var.environment
  }
}

# Access Point para Data Protection Keys
resource "aws_efs_access_point" "data_protection" {
  file_system_id = aws_efs_file_system.data_protection.id

  posix_user {
    gid = 1000
    uid = 1000
  }

  root_directory {
    path = "/data-protection-keys"
    creation_info {
      owner_gid   = 1000
      owner_uid   = 1000
      permissions = "755"
    }
  }

  tags = {
    Name        = "${var.project_name}-data-protection-ap"
    Environment = var.environment
  }
}
*/
