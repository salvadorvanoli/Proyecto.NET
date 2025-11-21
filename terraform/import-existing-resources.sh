#!/bin/bash
# Script para importar recursos existentes a Terraform

set -e

echo "Importando recursos existentes a Terraform..."

# Obtener la región de AWS
AWS_REGION="${AWS_REGION:-us-east-1}"
echo "Usando región: $AWS_REGION"

# Importar ECR Repositories
echo "Importando repositorios ECR..."
terraform import aws_ecr_repository.api proyectonet-api || true
terraform import aws_ecr_repository.backoffice proyectonet-backoffice || true
terraform import aws_ecr_repository.frontoffice proyectonet-frontoffice || true

# Importar CloudWatch Log Groups
echo "Importando Log Groups..."
terraform import aws_cloudwatch_log_group.api /ecs/proyectonet-api || true
terraform import aws_cloudwatch_log_group.backoffice /ecs/proyectonet-backoffice || true
terraform import aws_cloudwatch_log_group.frontoffice /ecs/proyectonet-frontoffice || true

# Importar ALB
echo "Importando Application Load Balancer..."
ALB_ARN=$(aws elbv2 describe-load-balancers --names proyectonet-alb --region $AWS_REGION --query 'LoadBalancers[0].LoadBalancerArn' --output text 2>/dev/null || echo "")
if [ -n "$ALB_ARN" ] && [ "$ALB_ARN" != "None" ]; then
    terraform import aws_lb.main "$ALB_ARN" || true
fi

# Importar Target Groups
echo "Importando Target Groups..."
API_TG_ARN=$(aws elbv2 describe-target-groups --names proyectonet-api-tg --region $AWS_REGION --query 'TargetGroups[0].TargetGroupArn' --output text 2>/dev/null || echo "")
if [ -n "$API_TG_ARN" ] && [ "$API_TG_ARN" != "None" ]; then
    terraform import aws_lb_target_group.api "$API_TG_ARN" || true
fi

BACKOFFICE_TG_ARN=$(aws elbv2 describe-target-groups --names proyectonet-backoffice-tg --region $AWS_REGION --query 'TargetGroups[0].TargetGroupArn' --output text 2>/dev/null || echo "")
if [ -n "$BACKOFFICE_TG_ARN" ] && [ "$BACKOFFICE_TG_ARN" != "None" ]; then
    terraform import aws_lb_target_group.backoffice "$BACKOFFICE_TG_ARN" || true
fi

FRONTOFFICE_TG_ARN=$(aws elbv2 describe-target-groups --names proyectonet-frontoffice-tg --region $AWS_REGION --query 'TargetGroups[0].TargetGroupArn' --output text 2>/dev/null || echo "")
if [ -n "$FRONTOFFICE_TG_ARN" ] && [ "$FRONTOFFICE_TG_ARN" != "None" ]; then
    terraform import aws_lb_target_group.frontoffice "$FRONTOFFICE_TG_ARN" || true
fi

# Importar DB Subnet Group
echo "Importando DB Subnet Group..."
terraform import aws_db_subnet_group.main proyectonet-db-subnet-group || true

echo "✅ Importación completada!"
echo "Ahora puedes ejecutar 'terraform plan' para ver qué cambios se aplicarán."

