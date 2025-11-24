# Redis como contenedor ECS
# Configuración de Redis como servicio standalone en ECS Fargate
# Esta solución es compatible con AWS Learner Labs (ElastiCache no disponible)

# CloudWatch Log Group para Redis
resource "aws_cloudwatch_log_group" "redis" {
  count             = var.redis_enabled ? 1 : 0
  name              = "/ecs/${var.project_name}-redis"
  retention_in_days = 7

  tags = {
    Name        = "${var.project_name}-redis-logs"
    Environment = var.environment
  }
}

# ECS Task Definition - Redis
resource "aws_ecs_task_definition" "redis" {
  count                    = var.redis_enabled ? 1 : 0
  family                   = "${var.project_name}-redis"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "256" # 0.25 vCPU
  memory                   = "512" # 512 MB
  execution_role_arn       = data.aws_iam_role.lab_role.arn
  task_role_arn            = data.aws_iam_role.lab_role.arn

  container_definitions = jsonencode([
    {
      name      = "redis"
      image     = "redis:7.2-alpine"
      essential = true

      portMappings = [
        {
          containerPort = 6379
          protocol      = "tcp"
        }
      ]

      command = [
        "redis-server",
        "--appendonly", "yes",
        "--requirepass", var.redis_password,
        "--maxmemory", "256mb",
        "--maxmemory-policy", "allkeys-lru"
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.redis[0].name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }

      healthCheck = {
        command     = ["CMD-SHELL", "redis-cli --raw incr ping || exit 1"]
        interval    = 30
        timeout     = 5
        retries     = 3
        startPeriod = 30
      }
    }
  ])

  tags = {
    Name        = "${var.project_name}-redis-task"
    Environment = var.environment
  }
}

# Network Load Balancer interno para Redis
resource "aws_lb" "redis" {
  count              = var.redis_enabled ? 1 : 0
  name               = "${var.project_name}-redis-nlb"
  internal           = true
  load_balancer_type = "network"
  subnets            = aws_subnet.private[*].id

  enable_deletion_protection = false

  tags = {
    Name        = "${var.project_name}-redis-nlb"
    Environment = var.environment
  }
}

# Target Group para Redis
resource "aws_lb_target_group" "redis" {
  count       = var.redis_enabled ? 1 : 0
  name        = "${var.project_name}-redis-tg"
  port        = 6379
  protocol    = "TCP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled  = true
    protocol = "TCP"
    port     = 6379
    interval = 30
  }

  tags = {
    Name        = "${var.project_name}-redis-tg"
    Environment = var.environment
  }
}

# Listener para Redis NLB
resource "aws_lb_listener" "redis" {
  count             = var.redis_enabled ? 1 : 0
  load_balancer_arn = aws_lb.redis[0].arn
  port              = 6379
  protocol          = "TCP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.redis[0].arn
  }
}

# ECS Service - Redis
resource "aws_ecs_service" "redis" {
  count           = var.redis_enabled ? 1 : 0
  name            = "${var.project_name}-redis-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.redis[0].arn
  desired_count   = 1
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = aws_subnet.private[*].id
    security_groups  = [aws_security_group.redis_ecs[0].id]
    assign_public_ip = false
  }

  # Registrar con NLB en lugar de Service Discovery
  load_balancer {
    target_group_arn = aws_lb_target_group.redis[0].arn
    container_name   = "redis"
    container_port   = 6379
  }

  depends_on = [aws_lb_listener.redis]

  tags = {
    Name        = "${var.project_name}-redis-service"
    Environment = var.environment
  }
}

# Security Group para Redis ECS
resource "aws_security_group" "redis_ecs" {
  count       = var.redis_enabled ? 1 : 0
  name        = "${var.project_name}-redis-ecs-sg"
  description = "Security group for Redis ECS service"
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
    Name        = "${var.project_name}-redis-ecs-sg"
    Environment = var.environment
  }
}
