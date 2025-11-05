# Credencial Digital - .NET 8

## Descripción general
Sistema integral para la gestión de **credenciales digitales** en entornos académicos o empresariales.
Permite a estudiantes y funcionarios acceder a servicios institucionales (accesos, comedor, biblioteca, beneficios) mediante una aplicación web y móvil integrada.

Arquitectura basada en **Clean Architecture + .NET 8**, con separación clara de responsabilidades.

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
- **SQLite / SQL Server**
- **Serilog + Swagger + FluentValidation**

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
1. Clonar el repositorio.
2. Restaurar dependencias:
   ```bash
   dotnet restore
