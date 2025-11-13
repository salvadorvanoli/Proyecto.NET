# 🚀 Guía Completa de Despliegue en AWS Learner Lab

Esta guía te permitirá desplegar completamente tu aplicación .NET en AWS desde cero, incluso si pierdes los créditos del Learner Lab y necesitas empezar de nuevo.

---

## 📋 Tabla de Contenidos

1. [Pre-requisitos](#pre-requisitos)
2. [Paso 1: Configurar Credenciales de AWS](#paso-1-configurar-credenciales-de-aws)
3. [Paso 2: Desplegar Infraestructura con Terraform](#paso-2-desplegar-infraestructura-con-terraform)
4. [Paso 3: Construir y Subir Imágenes Docker](#paso-3-construir-y-subir-imágenes-docker)
5. [Paso 4: Verificar el Despliegue](#paso-4-verificar-el-despliegue)
6. [Paso 5: Actualizar la Aplicación](#paso-5-actualizar-la-aplicación)
7. [Troubleshooting](#troubleshooting)
8. [Limpieza y Destrucción](#limpieza-y-destrucción)

---

## Pre-requisitos

### Software Requerido

- ✅ **AWS CLI** instalado y configurado
- ✅ **Terraform** >= 1.0
- ✅ **Docker Desktop** instalado y corriendo
- ✅ **PowerShell** (viene con Windows)
- ✅ **.NET SDK 8.0** o superior

### Verificar Instalaciones

```powershell
# Verificar AWS CLI
aws --version

# Verificar Terraform
terraform --version

# Verificar Docker
docker --version

# Verificar .NET
dotnet --version
```

---

## Paso 1: Configurar Credenciales de AWS

### 1.1 Obtener Credenciales del Learner Lab

1. Ingresa a tu **AWS Academy Learner Lab**
2. Click en **"Start Lab"** (espera a que el círculo se ponga verde)
3. Click en **"AWS Details"**
4. Click en **"Show"** al lado de **"AWS CLI"**
5. Copia **TODO** el contenido que aparece

### 1.2 Configurar Credenciales en tu Computadora

**Opción A - Configuración Rápida (Recomendada):**

```powershell
# Navega al proyecto
cd C:\Users\salva\RiderProjects\Proyecto.NET

# Ejecuta el script de validación de credenciales
.\scripts\validate-env.ps1
```

Este script te pedirá pegar las credenciales del Learner Lab.

**Opción B - Configuración Manual:**

1. Abre el archivo: `C:\Users\TU_USUARIO\.aws\credentials`
2. Reemplaza el contenido con las credenciales del Learner Lab:

```ini
[default]
aws_access_key_id=ASIA...
aws_secret_access_key=...
aws_session_token=...
```

### 1.3 Verificar Credenciales

```powershell
aws sts get-caller-identity
```

Deberías ver tu Account ID y el ARN del usuario.

---

## Paso 2: Desplegar Infraestructura con Terraform

### 2.1 Configurar Variables de Terraform

1. Navega al directorio de Terraform:

```powershell
cd terraform
```

2. Copia el archivo de ejemplo y edítalo:

```powershell
copy terraform.tfvars.example terraform.tfvars
```

3. Edita `terraform.tfvars` con tus valores:

```hcl
# Configuración básica
project_name = "proyectonet"
environment  = "production"
aws_region   = "us-east-1"

# Base de datos RDS
db_username = "admin"
db_password = "TuPasswordSegura123!"  # CAMBIA ESTO
db_name     = "ProyectoNetDb"

# Recursos ECS
api_cpu            = "512"
api_memory         = "1024"
api_desired_count  = 1

backoffice_cpu            = "256"
backoffice_memory         = "512"
backoffice_desired_count  = 1
```

### 2.2 Inicializar y Desplegar con Terraform

```powershell
# Inicializar Terraform (solo la primera vez)
terraform init

# Ver qué recursos se van a crear
terraform plan

# Crear la infraestructura
terraform apply -auto-approve
```

⏱️ **Tiempo estimado:** 10-15 minutos

### 2.3 Guardar las URLs de Salida

Al finalizar, Terraform mostrará las URLs importantes:

```
Outputs:

alb_url = "http://proyectonet-alb-XXXXXXXXX.us-east-1.elb.amazonaws.com"
api_url = "http://proyectonet-alb-XXXXXXXXX.us-east-1.elb.amazonaws.com/api"
backoffice_url = "http://proyectonet-alb-XXXXXXXXX.us-east-1.elb.amazonaws.com"
```

**⚠️ IMPORTANTE:** Guarda estas URLs en un lugar seguro.

---

## Paso 3: Construir y Subir Imágenes Docker

### 3.1 Usar el Script Automatizado (Recomendado)

```powershell
# Volver al directorio raíz
cd ..

# Ejecutar el script de despliegue
.\upload-to-ecr.ps1
```

El script te hará las siguientes preguntas:

**Pregunta 1: ¿Deseas RECREAR la base de datos?**
- Primera vez: Responde **S** (Sí)
- Despliegues posteriores: Responde **N** (No)

**Pregunta 2: ¿Qué servicios deseas desplegar?**
- Primera vez: Opción **3** (Ambos)
- Actualizaciones: 
  - Solo cambios en API: Opción **1**
  - Solo cambios en BackOffice: Opción **2**
  - Cambios en ambos: Opción **3**

⏱️ **Tiempo estimado:** 10-20 minutos (dependiendo de tu conexión)

### 3.2 Proceso Manual (Si el script falla)

Si por alguna razón el script automático falla, puedes hacerlo manualmente:

#### 3.2.1 Login en ECR

```powershell
# Obtener token de ECR y hacer login
$ecrToken = aws ecr get-authorization-token --region us-east-1 --output json | ConvertFrom-Json
$authData = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($ecrToken.authorizationData[0].authorizationToken))
$username = $authData.Split(':')[0]
$password = $authData.Split(':')[1]

# Guardar password en archivo temporal
$tempFile = [System.IO.Path]::GetTempFileName()
$password | Out-File -FilePath $tempFile -Encoding ASCII -NoNewline

# Login
Get-Content $tempFile | docker login --username $username --password-stdin $ecrToken.authorizationData[0].proxyEndpoint

# Limpiar
Remove-Item $tempFile -Force
```

#### 3.2.2 Obtener Account ID

```powershell
$accountId = aws sts get-caller-identity --query Account --output text
$region = "us-east-1"
```

#### 3.2.3 Construir y Subir API

```powershell
# Construir imagen
docker build -t proyectonet-api --target final-api -f Dockerfile .

# Etiquetar
docker tag proyectonet-api:latest ${accountId}.dkr.ecr.${region}.amazonaws.com/proyectonet-api:latest

# Subir
docker push ${accountId}.dkr.ecr.${region}.amazonaws.com/proyectonet-api:latest
```

#### 3.2.4 Construir y Subir BackOffice

```powershell
# Construir imagen
docker build -t proyectonet-backoffice --target final-backoffice -f Dockerfile .

# Etiquetar
docker tag proyectonet-backoffice:latest ${accountId}.dkr.ecr.${region}.amazonaws.com/proyectonet-backoffice:latest

# Subir
docker push ${accountId}.dkr.ecr.${region}.amazonaws.com/proyectonet-backoffice:latest
```

#### 3.2.5 Forzar Redespliegue de Servicios

```powershell
# Redesplegar API
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-api-service --force-new-deployment --region us-east-1

# Redesplegar BackOffice
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-backoffice-service --force-new-deployment --region us-east-1
```

---

## Paso 4: Verificar el Despliegue

### 4.1 Verificar Estado de los Servicios

```powershell
# Ver estado de servicios ECS
aws ecs describe-services --cluster proyectonet-cluster --services proyectonet-api-service proyectonet-backoffice-service --region us-east-1 --query "services[*].[serviceName,runningCount,desiredCount]" --output table
```

Deberías ver que `runningCount` == `desiredCount` para ambos servicios.

### 4.2 Verificar Logs

**Logs del API:**
```powershell
aws logs tail /ecs/proyectonet-api --since 5m --region us-east-1 --format short
```

**Logs del BackOffice:**
```powershell
aws logs tail /ecs/proyectonet-backoffice --since 5m --region us-east-1 --format short
```

**Buscar errores:**
```powershell
aws logs tail /ecs/proyectonet-api --since 10m --region us-east-1 --format short | findstr /i "error exception fail"
```

### 4.3 Verificar Base de Datos

Busca en los logs del API el mensaje de seeding exitoso:

```powershell
aws logs tail /ecs/proyectonet-api --since 30m --region us-east-1 --format short | findstr "CREDENCIALES"
```

Deberías ver:
```
CREDENCIALES DE ACCESO:
Email: admin1@backoffice.com
Password: Admin123!
```

### 4.4 Probar el Acceso

1. **Desde tu computadora:**
   - Abre el navegador
   - Ve a: `http://proyectonet-alb-XXXXXXXXX.us-east-1.elb.amazonaws.com/`
   - Deberías ver la página de login del BackOffice

2. **Desde tu celular:**
   - Asegúrate de escribir **http://** (no https://)
   - URL: `http://proyectonet-alb-XXXXXXXXX.us-east-1.elb.amazonaws.com/`

3. **Iniciar sesión:**
   - Email: `admin1@backoffice.com`
   - Password: `Admin123!`

### 4.5 Probar el API

**Desde navegador o Postman:**
```
GET http://proyectonet-alb-XXXXXXXXX.us-east-1.elb.amazonaws.com/api/health
```

Deberías recibir un status `200 OK`.

---

## Paso 5: Actualizar la Aplicación

### 5.1 Después de Hacer Cambios en el Código

1. **Guarda todos tus cambios** en el código

2. **Ejecuta el script de actualización:**

```powershell
.\upload-to-ecr.ps1
```

3. **Responde las preguntas del script:**
   - RECREAR base de datos: **N** (No) - a menos que necesites resetear datos
   - Servicios a desplegar: 
     - Si solo cambiaste el BackOffice: **2**
     - Si solo cambiaste el API: **1**
     - Si cambiaste ambos: **3**

4. **Espera 2-3 minutos** para que los nuevos contenedores se desplieguen

5. **Verifica los logs** para confirmar que todo está bien

### 5.2 Actualización Manual (Sin Script)

Si prefieres más control, puedes actualizar servicios específicos:

**Solo API:**
```powershell
docker build -t proyectonet-api --target final-api -f Dockerfile .
docker tag proyectonet-api:latest $accountId.dkr.ecr.us-east-1.amazonaws.com/proyectonet-api:latest
docker push $accountId.dkr.ecr.us-east-1.amazonaws.com/proyectonet-api:latest
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-api-service --force-new-deployment --region us-east-1
```

**Solo BackOffice:**
```powershell
docker build -t proyectonet-backoffice --target final-backoffice -f Dockerfile .
docker tag proyectonet-backoffice:latest $accountId.dkr.ecr.us-east-1.amazonaws.com/proyectonet-backoffice:latest
docker push $accountId.dkr.ecr.us-east-1.amazonaws.com/proyectonet-backoffice:latest
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-backoffice-service --force-new-deployment --region us-east-1
```

---

## Troubleshooting

### Problema: "Credenciales inválidas o expiradas"

**Causa:** Las credenciales del Learner Lab expiran después de 4 horas.

**Solución:**
1. Ve al Learner Lab en AWS Academy
2. Si el círculo está rojo, click en "Start Lab"
3. Obtén las nuevas credenciales (AWS Details → Show)
4. Actualiza el archivo `~/.aws/credentials`
5. Vuelve a intentar

### Problema: "Cannot drop database because it is currently in use"

**Causa:** Hay múltiples contenedores intentando recrear la base de datos al mismo tiempo.

**Solución:**
```powershell
# 1. Detener servicios temporalmente
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-backoffice-service --desired-count 0 --region us-east-1
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-api-service --desired-count 0 --region us-east-1

# 2. Esperar 30 segundos
timeout /t 30

# 3. Volver a levantar solo el API
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-api-service --desired-count 1 --force-new-deployment --region us-east-1

# 4. Esperar 2 minutos para que recree la DB
timeout /t 120

# 5. Levantar el BackOffice
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-backoffice-service --desired-count 1 --region us-east-1
```

### Problema: "Error 404 Not Found" en el BackOffice

**Causa:** Estás intentando acceder a `/backoffice` en lugar de la raíz.

**Solución:**
- URL correcta: `http://proyectonet-alb-XXXXXXXXX.us-east-1.elb.amazonaws.com/`
- URL incorrecta: `http://proyectonet-alb-XXXXXXXXX.us-east-1.elb.amazonaws.com/backoffice`

### Problema: "Login failed with status code: NotFound"

**Causa:** El BackOffice no puede comunicarse con el API (URL incorrecta).

**Solución:**
Verifica los logs del BackOffice:
```powershell
aws logs tail /ecs/proyectonet-backoffice --since 5m --region us-east-1 --format short | findstr "Configuring"
```

Deberías ver:
```
Configuring BackOffice to use API at: http://proyectonet-alb-XXXXXXXXX.us-east-1.elb.amazonaws.com/
```

Si ves `/api/api/` duplicado, el problema está en la configuración.

### Problema: "AWS rechazó la conexión" desde el celular

**Causa:** El navegador está forzando HTTPS.

**Solución:**
1. Escribe explícitamente `http://` en la URL (no `https://`)
2. Usa modo incógnito/privado en el navegador
3. Desactiva "HTTPS automático" en la configuración del navegador
4. O usa un navegador alternativo (Firefox, Edge)

### Problema: Servicios en estado "DRAINING" o "PENDING"

**Causa:** Los contenedores no están pasando los health checks.

**Solución:**
```powershell
# Ver detalles de las tareas
aws ecs list-tasks --cluster proyectonet-cluster --service proyectonet-api-service --region us-east-1

# Ver logs de una tarea específica
aws ecs describe-tasks --cluster proyectonet-cluster --tasks TASK_ID --region us-east-1
```

### Problema: "Error building Docker image"

**Causa:** Problema con el Dockerfile o dependencias.

**Solución:**
1. Verifica que Docker Desktop esté corriendo
2. Intenta construir localmente para ver el error:
   ```powershell
   docker build -t test-api --target final-api -f Dockerfile .
   ```
3. Revisa los mensajes de error específicos

---

## Limpieza y Destrucción

### Cuando Termines de Usar el Lab

**Opción 1 - Destruir Todo (Recomendado para ahorrar créditos):**

```powershell
cd terraform
terraform destroy -auto-approve
```

Esto eliminará:
- ✅ Load Balancer
- ✅ Servicios ECS y tareas
- ✅ Cluster ECS
- ✅ Base de datos RDS (y todos los datos)
- ✅ VPC y subnets
- ✅ Security Groups
- ✅ Repositorios ECR (imágenes Docker)

⏱️ **Tiempo estimado:** 10-15 minutos

**Opción 2 - Solo Detener Servicios (Para reanudar más tarde):**

```powershell
# Detener servicios ECS
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-api-service --desired-count 0 --region us-east-1
aws ecs update-service --cluster proyectonet-cluster --service proyectonet-backoffice-service --desired-count 0 --region us-east-1

# Detener base de datos RDS (no soportado en Learner Lab, pero puedes intentar)
aws rds stop-db-instance --db-instance-identifier proyectonet-sqlserver --region us-east-1
```

**Opción 3 - Limpiar Manualmente en la Consola de AWS:**

1. Ve a **ECS** → Clusters → `proyectonet-cluster`
   - Elimina los servicios
   - Elimina el cluster
2. Ve a **EC2** → Load Balancers
   - Elimina el ALB
3. Ve a **RDS** → Databases
   - Elimina la instancia (sin snapshot final)
4. Ve a **ECR** → Repositories
   - Elimina los repositorios
5. Ve a **VPC**
   - Elimina la VPC (esto eliminará subnets, route tables, etc.)

---

## 📝 Checklist de Despliegue Completo

Usa este checklist cuando necesites desplegar desde cero:

- [ ] AWS Learner Lab iniciado (círculo verde)
- [ ] Credenciales de AWS configuradas
- [ ] Variables de Terraform configuradas (`terraform.tfvars`)
- [ ] `terraform init` ejecutado
- [ ] `terraform apply` completado exitosamente
- [ ] URLs del ALB guardadas
- [ ] Login en ECR exitoso
- [ ] Imagen del API construida y subida
- [ ] Imagen del BackOffice construida y subida
- [ ] Servicios ECS desplegados
- [ ] Logs del API verificados (sin errores)
- [ ] Logs del BackOffice verificados (URL del API correcta)
- [ ] Base de datos creada y seeded
- [ ] Acceso desde navegador web funcionando
- [ ] Login con credenciales de admin funcionando
- [ ] Acceso desde celular funcionando

---

## 🎓 Consejos para Maximizar Créditos del Learner Lab

1. **Destruye recursos cuando no los uses:**
   ```powershell
   terraform destroy -auto-approve
   ```

2. **Usa instancias más pequeñas en desarrollo:**
   - API: `cpu = 256, memory = 512`
   - BackOffice: `cpu = 256, memory = 512`
   - RDS: `db.t3.micro` (ya está configurado)

3. **Reduce el número de réplicas:**
   - `api_desired_count = 1`
   - `backoffice_desired_count = 1`

4. **Elimina logs antiguos:**
   ```powershell
   aws logs delete-log-group --log-group-name /ecs/proyectonet-api --region us-east-1
   aws logs delete-log-group --log-group-name /ecs/proyectonet-backoffice --region us-east-1
   ```

5. **Monitorea tu uso de créditos:**
   - Ve al dashboard del Learner Lab
   - Click en "AWS Details" para ver créditos restantes

---

## 📚 Recursos Adicionales

- **Documentación del Proyecto:** `README.md`
- **Documentación de Docker:** `docs/DOCKER.md`
- **Documentación de Base de Datos:** `docs/BASE_DE_DATOS.md`
- **Documentación de Despliegue AWS:** `DESPLIEGUE_AWS.md`

---

## 🆘 Soporte

Si tienes problemas que no están cubiertos en esta guía:

1. **Revisa los logs:**
   ```powershell
   aws logs tail /ecs/proyectonet-api --since 10m --region us-east-1 --format short
   ```

2. **Verifica el estado de los servicios:**
   ```powershell
   aws ecs describe-services --cluster proyectonet-cluster --services proyectonet-api-service --region us-east-1
   ```

3. **Consulta la documentación oficial:**
   - [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
   - [Terraform AWS Provider](https://registry.terraform.io/providers/hashicorp/aws/latest/docs)

---

**¡Listo! Con esta guía deberías poder desplegar tu aplicación en AWS desde cero tantas veces como necesites.** 🚀

