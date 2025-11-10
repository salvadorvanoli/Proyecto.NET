# Guía de Despliegue en AWS

Esta guía te ayudará a desplegar tu aplicación Proyecto.NET en AWS usando Terraform.

## 📋 Pre-requisitos

1. ✅ Terraform instalado (>= 1.0)
2. ✅ AWS CLI configurado
3. ✅ Docker Desktop instalado y ejecutándose
4. ✅ Acceso al AWS Learner Lab

## 🚀 Paso 1: Configurar AWS CLI

### Obtener credenciales del Learner Lab

1. Ingresa a tu AWS Learner Lab
2. Haz clic en "AWS Details"
3. Haz clic en "Show" en AWS CLI credentials
4. Copia las credenciales

### Configurar en Windows

Opción A - Usando variables de entorno (recomendado para Learner Lab):

```cmd
set AWS_ACCESS_KEY_ID=tu_access_key_aqui
set AWS_SECRET_ACCESS_KEY=tu_secret_key_aqui
set AWS_SESSION_TOKEN=tu_session_token_aqui
set AWS_DEFAULT_REGION=us-east-1
```

Opción B - Usando AWS CLI configure:

```cmd
aws configure
```

### Verificar configuración

```cmd
aws sts get-caller-identity
```

Deberías ver tu información de cuenta.

## 🔧 Paso 2: Configurar Terraform

### Crear archivo terraform.tfvars

1. Ve al directorio terraform:
```cmd
cd terraform
```

2. Copia el archivo de ejemplo:
```cmd
copy terraform.tfvars.example terraform.tfvars
```

3. Edita `terraform.tfvars` con un editor de texto y configura:

```hcl
aws_region   = "us-east-1"
project_name = "proyectonet"
environment  = "production"

# Cambia estas credenciales por unas seguras
db_username = "dbadmin"
db_password = "CambiaEstaPassword123!"
db_name     = "ProyectoNetDb"

# Configuración de recursos (puedes ajustar según necesidad)
db_instance_class = "db.t3.micro"

api_cpu              = "256"
api_memory           = "512"
api_desired_count    = 1

backoffice_cpu           = "256"
backoffice_memory        = "512"
backoffice_desired_count = 1
```

**IMPORTANTE**: Cambia el `db_password` por una contraseña segura.

## 🎯 Paso 3: Desplegar (Opción Automática)

La forma más fácil es usar el script automatizado:

```cmd
cd ..
deploy-aws.bat
```

Este script hará:
1. Inicializar Terraform
2. Crear toda la infraestructura en AWS
3. Construir las imágenes Docker
4. Subirlas a ECR
5. Desplegar los servicios

⏱️ **Tiempo estimado**: 10-15 minutos

## 🔨 Paso 4: Desplegar (Opción Manual)

Si prefieres hacerlo paso a paso:

### 4.1 Inicializar Terraform

```cmd
cd terraform
terraform init
```

### 4.2 Ver el plan

```cmd
terraform plan
```

Revisa que todo se vea bien.

### 4.3 Aplicar la infraestructura

```cmd
terraform apply
```

Escribe `yes` cuando se solicite.

### 4.4 Obtener URLs de ECR

```cmd
terraform output ecr_repository_backoffice_url
terraform output ecr_repository_api_url
```

Guarda estas URLs.

### 4.5 Construir y subir imágenes Docker

```cmd
cd ..

REM Hacer login en ECR (reemplaza con tu región y account ID)
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com

REM Construir BackOffice
docker build -t proyectonet-backoffice --target final-backoffice .
docker tag proyectonet-backoffice:latest ECR_BACKOFFICE_URL:latest
docker push ECR_BACKOFFICE_URL:latest

REM Construir API
docker build -t proyectonet-api --target final-api .
docker tag proyectonet-api:latest ECR_API_URL:latest
docker push ECR_API_URL:latest
```

### 4.6 Forzar despliegue en ECS

```cmd
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-backoffice-service --force-new-deployment
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-api-service --force-new-deployment
```

## 🌐 Paso 5: Acceder a la aplicación

Obtén la URL:

```cmd
cd terraform
terraform output alb_url
```

Abre esa URL en tu navegador. 

**Nota**: Puede tomar 2-5 minutos hasta que los servicios estén completamente operativos.

## 📊 Monitoreo

### Ver estado de los servicios

```cmd
check-aws-status.bat
```

### Ver logs en tiempo real

```cmd
REM Logs del BackOffice
aws logs tail /ecs/proyectonet-backoffice --follow

REM Logs de la API
aws logs tail /ecs/proyectonet-api --follow
```

### Consola de AWS

- **ECS Services**: https://console.aws.amazon.com/ecs/
- **CloudWatch Logs**: https://console.aws.amazon.com/cloudwatch/
- **RDS**: https://console.aws.amazon.com/rds/
- **Load Balancers**: https://console.aws.amazon.com/ec2/v2/home#LoadBalancers

## 🔄 Actualizar la aplicación

Cuando hagas cambios en el código:

```cmd
update-aws.bat
```

Este script:
1. Construye las nuevas imágenes
2. Las sube a ECR
3. Fuerza un nuevo despliegue

## 🧹 Limpiar recursos

**ADVERTENCIA**: Esto eliminará TODO, incluyendo la base de datos.

```cmd
destroy-aws.bat
```

O manualmente:

```cmd
cd terraform
terraform destroy
```

## ❗ Troubleshooting

### Los servicios no inician

1. Revisa los logs en CloudWatch
2. Verifica que las imágenes estén en ECR:
   ```cmd
   aws ecr describe-images --repository-name proyectonet-backoffice
   ```
3. Revisa el estado de las tareas:
   ```cmd
   aws ecs describe-tasks --cluster proyectonet-cluster --tasks TASK_ARN
   ```

### Error 503 en el Load Balancer

- Los servicios aún están iniciando. Espera 2-5 minutos.
- Verifica los health checks en el Target Group

### No puedo conectar a la base de datos

- Verifica que el security group permita tráfico desde ECS
- Revisa los logs de las tareas para ver el error exacto

### Sesión de Learner Lab expirada

1. Inicia una nueva sesión
2. Obtén las nuevas credenciales
3. Configura AWS CLI nuevamente
4. Ejecuta `terraform refresh` para sincronizar el estado

## 💰 Costos estimados

Para el Learner Lab (100 USD de crédito):

- RDS SQL Server Express (db.t3.micro): ~$0.50/día
- ECS Fargate (2 tareas): ~$0.30/día
- ALB: ~$0.65/día
- Otros: ~$0.10/día

**Total**: ~$1.55/día o ~$46/mes

## 📝 Notas importantes

1. **Learner Lab limitations**:
   - La sesión expira después de 4 horas
   - Los recursos se mantienen, pero necesitas nuevas credenciales
   - Algunos servicios pueden no estar disponibles

2. **Seguridad**:
   - Cambia las contraseñas de base de datos
   - No subas `terraform.tfvars` a Git
   - Usa secrets manager para producción real

3. **Performance**:
   - Los recursos están configurados para el tier gratuito/bajo costo
   - Para producción, aumenta CPU/memoria según necesidad

## 🆘 Soporte

Si tienes problemas:

1. Revisa los logs de CloudWatch
2. Verifica el estado de los servicios ECS
3. Usa `check-aws-status.bat` para diagnóstico
4. Revisa la documentación de Terraform AWS provider

## 📚 Referencias

- [Terraform AWS Provider](https://registry.terraform.io/providers/hashicorp/aws/latest/docs)
- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [AWS RDS SQL Server](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/CHAP_SQLServer.html)

