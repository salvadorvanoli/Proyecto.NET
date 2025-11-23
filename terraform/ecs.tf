# ECS Cluster
resource "aws_ecs_cluster" "main" {
  name = "${var.project_name}-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = {
    Name        = "${var.project_name}-cluster"
    Environment = var.environment
  }
}

# CloudWatch Log Group
resource "aws_cloudwatch_log_group" "backoffice" {
  name              = "/ecs/${var.project_name}-backoffice"
  retention_in_days = 7

  tags = {
    Name        = "${var.project_name}-backoffice-logs"
    Environment = var.environment
  }
}

resource "aws_cloudwatch_log_group" "api" {
  name              = "/ecs/${var.project_name}-api"
  retention_in_days = 7

  tags = {
    Name        = "${var.project_name}-api-logs"
    Environment = var.environment
  }
}

resource "aws_cloudwatch_log_group" "frontoffice" {
  name              = "/ecs/${var.project_name}-frontoffice"
  retention_in_days = 7

  tags = {
    Name        = "${var.project_name}-frontoffice-logs"
    Environment = var.environment
  }
}

# Usar el LabRole existente del AWS Learner Lab
# El Learner Lab no permite crear roles IAM, pero proporciona un LabRole con permisos suficientes
data "aws_iam_role" "lab_role" {
  name = "LabRole"
}

# ECS Task Definition - API
resource "aws_ecs_task_definition" "api" {
  family                   = "${var.project_name}-api"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.api_cpu
  memory                   = var.api_memory
  execution_role_arn       = data.aws_iam_role.lab_role.arn
  task_role_arn            = data.aws_iam_role.lab_role.arn

  container_definitions = jsonencode([
    {
      name      = "api"
      image     = "${aws_ecr_repository.api.repository_url}:latest"
      essential = true

      portMappings = [
        {
          containerPort = 8080
          protocol      = "tcp"
        }
      ]

      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = "Production"
        },
        {
          name  = "ASPNETCORE_URLS"
          value = "http://+:8080"
        },
        {
          name  = "PATH_BASE"
          value = "/api"
        },
        {
          name  = "SEED_DATABASE"
          value = "true"
        },
        {
          name  = "RECREATE_DATABASE"
          value = "false"
        },
        {
          name  = "ConnectionStrings__DefaultConnection"
          value = "Server=${aws_db_instance.sqlserver.address},1433;Database=${var.db_name};User Id=${var.db_username};Password=${var.db_password};TrustServerCertificate=True;MultipleActiveResultSets=true"
        },
        {
          name  = "Jwt__Secret"
          value = var.jwt_secret
        },
        {
          name  = "Jwt__Issuer"
          value = var.jwt_issuer
        },
        {
          name  = "Jwt__Audience"
          value = var.jwt_audience
        },
        {
          name  = "Jwt__LifetimeMinutes"
          value = tostring(var.jwt_lifetime_minutes)
        },
        {
          name  = "CORS_ALLOWED_ORIGINS"
          value = length(var.cors_allowed_origins) > 0 ? join(",", var.cors_allowed_origins) : "http://${aws_lb.main.dns_name}"
        },
        {
          name  = "ConnectionStrings__Redis"
          value = var.redis_enabled ? "${aws_elasticache_cluster.redis[0].cache_nodes[0].address}:6379,abortConnect=false" : ""
        },
        {
          name  = "Redis__Enabled"
          value = tostring(var.redis_enabled)
        },
        {
          name  = "Redis__DefaultTtlMinutes"
          value = tostring(var.redis_default_ttl_minutes)
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.api.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }

      healthCheck = {
        command     = ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"]
        interval    = 30
        timeout     = 5
        retries     = 3
        startPeriod = 60
      }
    }
  ])

  tags = {
    Name        = "${var.project_name}-api-task"
    Environment = var.environment
  }
}

# ECS Task Definition - BackOffice
resource "aws_ecs_task_definition" "backoffice" {
  family                   = "${var.project_name}-backoffice"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.backoffice_cpu
  memory                   = var.backoffice_memory
  execution_role_arn       = data.aws_iam_role.lab_role.arn
  task_role_arn            = data.aws_iam_role.lab_role.arn

  container_definitions = jsonencode([
    {
      name      = "backoffice"
      image     = "${aws_ecr_repository.backoffice.repository_url}:latest"
      essential = true

      portMappings = [
        {
          containerPort = 8080
          protocol      = "tcp"
        }
      ]

      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = "Production"
        },
        {
          name  = "ASPNETCORE_URLS"
          value = "http://+:8080"
        },
        {
          name  = "API_BASE_URL"
          value = "http://${aws_lb.main.dns_name}"
        },
        {
          name  = "ConnectionStrings__DefaultConnection"
          value = "Server=${aws_db_instance.sqlserver.address},1433;Database=${var.db_name};User Id=${var.db_username};Password=${var.db_password};TrustServerCertificate=True;MultipleActiveResultSets=true"
        },
        {
          name  = "ConnectionStrings__Redis"
          value = var.redis_enabled ? "${aws_elasticache_cluster.redis[0].cache_nodes[0].address}:6379,abortConnect=false" : ""
        },
        {
          name  = "Redis__Enabled"
          value = tostring(var.redis_enabled)
        },
        {
          name  = "Redis__DefaultTtlMinutes"
          value = tostring(var.redis_default_ttl_minutes)
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.backoffice.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }

      healthCheck = {
        command     = ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"]
        interval    = 30
        timeout     = 5
        retries     = 3
        startPeriod = 60
      }
    }
  ])

  tags = {
    Name        = "${var.project_name}-backoffice-task"
    Environment = var.environment
  }
}

# ECS Task Definition - FrontOffice
resource "aws_ecs_task_definition" "frontoffice" {
  family                   = "${var.project_name}-frontoffice"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.frontoffice_cpu
  memory                   = var.frontoffice_memory
  execution_role_arn       = data.aws_iam_role.lab_role.arn
  task_role_arn            = data.aws_iam_role.lab_role.arn

  container_definitions = jsonencode([
    {
      name      = "frontoffice"
      image     = "${aws_ecr_repository.frontoffice.repository_url}:latest"
      essential = true

      portMappings = [
        {
          containerPort = 8080
          protocol      = "tcp"
        }
      ]

      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = "Production"
        },
        {
          name  = "ASPNETCORE_URLS"
          value = "http://+:8080"
        },
        {
          name  = "PATH_BASE"
          value = "/frontoffice"
        },
        {
          name  = "API_BASE_URL"
          value = "http://${aws_lb.main.dns_name}"
        },
        {
          name  = "ConnectionStrings__Redis"
          value = var.redis_enabled ? "${aws_elasticache_cluster.redis[0].cache_nodes[0].address}:6379,abortConnect=false" : ""
        },
        {
          name  = "Redis__Enabled"
          value = tostring(var.redis_enabled)
        },
        {
          name  = "Redis__DefaultTtlMinutes"
          value = tostring(var.redis_default_ttl_minutes)
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.frontoffice.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }

      healthCheck = {
        command     = ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"]
        interval    = 30
        timeout     = 5
        retries     = 3
        startPeriod = 60
      }
    }
  ])

  tags = {
    Name        = "${var.project_name}-frontoffice-task"
    Environment = var.environment
  }
}

# ECS Service - API
resource "aws_ecs_service" "api" {
  name            = "${var.project_name}-api-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.api.arn
  desired_count   = var.api_desired_count
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = aws_subnet.public[*].id
    security_groups  = [aws_security_group.ecs_tasks.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.api.arn
    container_name   = "api"
    container_port   = 8080
  }

  depends_on = [aws_lb_listener.main]

  tags = {
    Name        = "${var.project_name}-api-service"
    Environment = var.environment
  }
}

# ECS Service - BackOffice
resource "aws_ecs_service" "backoffice" {
  name            = "${var.project_name}-backoffice-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.backoffice.arn
  desired_count   = var.backoffice_desired_count
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = aws_subnet.public[*].id
    security_groups  = [aws_security_group.ecs_tasks.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.backoffice.arn
    container_name   = "backoffice"
    container_port   = 8080
  }

  depends_on = [aws_lb_listener.main]

  tags = {
    Name        = "${var.project_name}-backoffice-service"
    Environment = var.environment
  }
}

# ECS Service - FrontOffice
resource "aws_ecs_service" "frontoffice" {
  name            = "${var.project_name}-frontoffice-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.frontoffice.arn
  desired_count   = var.frontoffice_desired_count
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = aws_subnet.public[*].id
    security_groups  = [aws_security_group.ecs_tasks.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.frontoffice.arn
    container_name   = "frontoffice"
    container_port   = 8080
  }

  depends_on = [aws_lb_listener.main]

  tags = {
    Name        = "${var.project_name}-frontoffice-service"
    Environment = var.environment
  }
}
