# Application Load Balancer
resource "aws_lb" "main" {
  name               = "${var.project_name}-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = aws_subnet.public[*].id

  enable_deletion_protection = false
  enable_http2              = true

  tags = {
    Name        = "${var.project_name}-alb"
    Environment = var.environment
  }
}

# Target Group - API
resource "aws_lb_target_group" "api" {
  name        = "${var.project_name}-api-tg"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = "/health"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 3
  }

  deregistration_delay = 30

  stickiness {
    type            = "lb_cookie"
    cookie_duration = 28800  # 8 horas - necesario para SignalR WebSockets
    enabled         = true
  }

  tags = {
    Name        = "${var.project_name}-api-tg"
    Environment = var.environment
  }
}

# Target Group - BackOffice
resource "aws_lb_target_group" "backoffice" {
  name        = "${var.project_name}-backoffice-tg"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = "/health"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 3
  }

  deregistration_delay = 30

  stickiness {
    type            = "lb_cookie"
    cookie_duration = 28800  # 8 horas (igual que la sesión de autenticación)
    enabled         = true
  }

  tags = {
    Name        = "${var.project_name}-backoffice-tg"
    Environment = var.environment
  }
}

# Target Group - FrontOffice
resource "aws_lb_target_group" "frontoffice" {
  name        = "${var.project_name}-frontoffice-tg"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = "/health"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 3
  }

  deregistration_delay = 30

  stickiness {
    type            = "lb_cookie"
    cookie_duration = 28800  # 8 horas
    enabled         = true
  }

  tags = {
    Name        = "${var.project_name}-frontoffice-tg"
    Environment = var.environment
  }
}

# ALB Listener
# IMPORTANTE: En producción, deberías configurar HTTPS (puerto 443) con un certificado SSL/TLS
# Para desarrollo/learner lab, se usa HTTP (puerto 80)
# Para habilitar HTTPS:
# 1. Solicita un certificado en AWS Certificate Manager (ACM)
# 2. Agrega un listener en puerto 443 con el ARN del certificado
# 3. Configura redirección de HTTP a HTTPS
resource "aws_lb_listener" "main" {
  load_balancer_arn = aws_lb.main.arn
  port              = "80"
  protocol          = "HTTP"

  default_action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = "Not Found"
      status_code  = "404"
    }
  }
}

# Ejemplo de listener HTTPS (comentado - requiere certificado ACM):
# resource "aws_lb_listener" "https" {
#   load_balancer_arn = aws_lb.main.arn
#   port              = "443"
#   protocol          = "HTTPS"
#   ssl_policy        = "ELBSecurityPolicy-TLS-1-2-2017-01"
#   certificate_arn   = "arn:aws:acm:region:account-id:certificate/certificate-id"
#
#   default_action {
#     type = "fixed-response"
#     fixed_response {
#       content_type = "text/plain"
#       message_body = "Not Found"
#       status_code  = "404"
#     }
#   }
# }
#
# # Redirección HTTP a HTTPS
# resource "aws_lb_listener" "http_redirect" {
#   load_balancer_arn = aws_lb.main.arn
#   port              = "80"
#   protocol          = "HTTP"
#
#   default_action {
#     type = "redirect"
#     redirect {
#       port        = "443"
#       protocol    = "HTTPS"
#       status_code = "HTTP_301"
#     }
#   }
# }

# Listener Rule - API
resource "aws_lb_listener_rule" "api" {
  listener_arn = aws_lb_listener.main.arn
  priority     = 50

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api.arn
  }

  condition {
    path_pattern {
      values = ["/api", "/api/*"]
    }
  }
}

# Listener Rule - FrontOffice
resource "aws_lb_listener_rule" "frontoffice" {
  listener_arn = aws_lb_listener.main.arn
  priority     = 75

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.frontoffice.arn
  }

  condition {
    path_pattern {
      values = ["/frontoffice", "/frontoffice/*"]
    }
  }
}

# Listener Rule - BackOffice (default)
resource "aws_lb_listener_rule" "backoffice" {
  listener_arn = aws_lb_listener.main.arn
  priority     = 100

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.backoffice.arn
  }

  condition {
    path_pattern {
      values = ["/*"]
    }
  }
}
