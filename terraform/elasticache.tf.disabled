# ElastiCache Redis Cluster
# Este archivo configura un cluster de Redis usando ElastiCache para caching distribuido

# Security Group para ElastiCache
resource "aws_security_group" "elasticache" {
  count = var.redis_enabled ? 1 : 0

  name        = "${var.project_name}-elasticache-sg"
  description = "Security group for ElastiCache Redis"
  vpc_id      = aws_vpc.main.id

  ingress {
    description     = "Redis from ECS tasks"
    from_port       = 6379
    to_port         = 6379
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs_tasks.id]
  }

  egress {
    description = "Allow all outbound"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.project_name}-elasticache-sg"
    Environment = var.environment
  }
}

# ElastiCache Subnet Group
resource "aws_elasticache_subnet_group" "main" {
  count = var.redis_enabled ? 1 : 0

  name       = "${var.project_name}-elasticache-subnet"
  subnet_ids = aws_subnet.private[*].id

  tags = {
    Name        = "${var.project_name}-elasticache-subnet"
    Environment = var.environment
  }
}

# ElastiCache Redis Cluster
resource "aws_elasticache_cluster" "redis" {
  count = var.redis_enabled ? 1 : 0

  cluster_id           = "${var.project_name}-redis"
  engine               = "redis"
  engine_version       = "7.0"
  node_type            = var.redis_node_type
  num_cache_nodes      = 1
  parameter_group_name = aws_elasticache_parameter_group.redis[0].name
  subnet_group_name    = aws_elasticache_subnet_group.main[0].name
  security_group_ids   = [aws_security_group.elasticache[0].id]
  port                 = 6379

  # Snapshots y mantenimiento
  snapshot_retention_limit = 1
  snapshot_window          = "03:00-05:00"
  maintenance_window       = "sun:05:00-sun:07:00"

  # Para reducir costos en Learner Lab, deshabilitamos auto minor version upgrade
  auto_minor_version_upgrade = false

  tags = {
    Name        = "${var.project_name}-redis"
    Environment = var.environment
  }
}

# Parameter Group para Redis
resource "aws_elasticache_parameter_group" "redis" {
  count = var.redis_enabled ? 1 : 0

  name   = "${var.project_name}-redis-params"
  family = "redis7"

  # Configuración de memoria y eviction
  parameter {
    name  = "maxmemory-policy"
    value = "allkeys-lru"
  }

  # Timeout para conexiones idle
  parameter {
    name  = "timeout"
    value = "300"
  }

  tags = {
    Name        = "${var.project_name}-redis-params"
    Environment = var.environment
  }
}
