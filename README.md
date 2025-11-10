# Credencial Digital - .NET 8

## Descripción general
Sistema integral para la gestión de **credenciales digitales** en entornos académicos o empresariales.
Permite a estudiantes y funcionarios acceder a servicios institucionales (accesos, comedor, biblioteca, beneficios) mediante una aplicación web y móvil integrada.

Arquitectura basada en **Clean Architecture + .NET 8**, con separación clara de responsabilidades.

---

## 📚 Documentación

- **[Guía de Despliegue en AWS](GUIA_DESPLIEGUE_AWS.md)** - Guía completa paso a paso para AWS Learner Lab
- **[Guía de Inicialización](GUIA_INICIALIZACION.md)** - Configuración del proyecto
- **[Documentación de Docker](docs/DOCKER.md)** - Uso de Docker y Docker Compose
- **[Base de Datos](docs/BASE_DE_DATOS.md)** - Estructura y migraciones
- **[Despliegue AWS (Resumen)](DESPLIEGUE_AWS.md)** - Instrucciones de despliegue

---

## 🚀 Scripts de Despliegue Rápido

### Desplegar en AWS (desde cero)
```powershell
.\deploy-aws.ps1
```
Este script automatiza TODO el proceso de despliegue en AWS Learner Lab.

### Actualizar aplicación (después de cambios en el código)
```powershell
.\upload-to-ecr.ps1
```
Construye y sube las imágenes Docker, luego redesplega los servicios.

### Limpiar recursos de AWS (ahorrar créditos)
```powershell
.\cleanup-aws.ps1
```
Elimina TODOS los recursos de AWS para liberar créditos del Learner Lab.

---

## Estructura de proyectos

| Proyecto | Propósito | Dependencias |
|-----------|------------|---------------|
| **Domain** | Núcleo del negocio: entidades, enums, lógica pura. | — |
| **Application** | Casos de uso, validaciones, interfaces de servicios. | Domain |
| **Infrastructure** | Persistencia (EF Core), Identity, logs, servicios externos. | Application, Domain |
| **Shared** | DTOs, contratos y enums comunes entre cliente y servidor. | (Opcional) Domain |
| **Web.FrontOffice** | Frontend Blazor Web App (.NET 8) para usuarios finales. | Shared |
| **Web.BackOffice** | Razor Pages (admin, gestión de usuarios, beneficios, reportes). | Application, Infrastructure, Domain, Shared |
| **Mobile** | App .NET MAUI para credenciales digitales y validación offline. | Shared |
| **Web.Api** | API REST para FrontOffice y Mobile (autenticación JWT, endpoints). | Application, Infrastructure, Domain, Shared |

---

## Tecnologías principales
- **.NET 8 LTS**
- **Entity Framework Core 8**
- **ASP.NET Core Identity + JWT**
- **Blazor Web App (Full Stack)**
- **Razor Pages**
- **.NET MAUI (Android/iOS)**
- **SQL Server / SQLite**
- **Serilog + Swagger + FluentValidation**
- **Docker + Docker Compose**
- **AWS ECS + RDS + ALB** (Infraestructura cloud)
- **Terraform** (Infrastructure as Code)

---

## Flujo general
1. **Web.Api** expone endpoints REST para autenticación, usuarios, credenciales y beneficios.
2. **Web.FrontOffice** (Blazor) consume estos endpoints y brinda la interfaz al usuario.
3. **Mobile** (MAUI) consume la misma API y sincroniza datos localmente en modo offline.
4. **Web.BackOffice** permite a los administradores gestionar entidades del sistema.
5. **Infrastructure** maneja la persistencia y configuración técnica.
6. **Application** contiene la lógica de aplicación que orquesta las operaciones.
7. **Domain** define las entidades y reglas del negocio.

---

## Configuración inicial

### Desarrollo Local
1. Clonar el repositorio.
2. Restaurar dependencias:
   ```bash
   dotnet restore
   ```
3. Crear archivo `.env` a partir de `.env.example` y configurar valores locales.
4. Ejecutar migraciones pendientes:
   ```bash
   dotnet ef database update --project ./Infrastructure/Persistencia
   ```
5. Iniciar la aplicación:
   ```bash
   dotnet run --project ./Web.Api
   ```
6. Acceder a la UI en `http://localhost:5000` (FrontOffice) o `http://localhost:5001` (BackOffice).

### Producción en AWS
- Seguir la **[Guía de Despliegue en AWS](GUIA_DESPLIEGUE_AWS.md)** para configurar el entorno en la nube.
- Usar los scripts de PowerShell para un despliegue rápido y eficiente.

---

## Notas
- Asegúrese de tener instalado **Docker** y **AWS CLI** configurado para el despliegue en AWS.
- Para desarrollo móvil, abrir la solución en **Visual Studio 2022** o superior con soporte para .NET MAUI.
- Consultar la documentación específica de cada tecnología para optimizar el desarrollo y despliegue.

---

¡Bienvenido al proyecto de Credencial Digital! 🚀
