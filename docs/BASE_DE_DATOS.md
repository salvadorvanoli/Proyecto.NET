# Guía para Revisar la Base de Datos

## Información de la Base de Datos

**Motor:** SQL Server LocalDB  
**Nombre de la Base de Datos:** `ProyectoNetDb`  
**Cadena de Conexión:** `Server=(localdb)\mssqllocaldb;Database=ProyectoNetDb;Trusted_Connection=true;MultipleActiveResultSets=true`

## Herramientas para Conectarte

### 1. SQL Server Management Studio (SSMS)
- **Descargar:** https://aka.ms/ssmsfullsetup
- **Server Name:** `(localdb)\mssqllocaldb`
- **Authentication:** Windows Authentication

### 2. Azure Data Studio (Recomendado)
- **Descargar:** https://aka.ms/azuredatastudio
- **Server:** `(localdb)\mssqllocaldb`
- **Authentication Type:** Windows Authentication
- **Ventajas:** Más ligero, moderno y multiplataforma

### 3. Desde Visual Studio
1. **View** → **SQL Server Object Explorer**
2. Conecta a `(localdb)\mssqllocaldb`
3. Expande **Databases** → **ProyectoNetDb**

### 4. Desde JetBrains Rider
1. **View** → **Tool Windows** → **Database**
2. Click en el **+** para agregar nueva conexión
3. Selecciona **Microsoft SQL Server**
4. Configura:
   - **Host:** `localhost`
   - **Instance:** `mssqllocaldb`
   - **Authentication:** Windows Authentication
   - **Database:** `ProyectoNetDb`

### 5. Línea de Comandos (sqlcmd)

```bash
# Conectarse a LocalDB
sqlcmd -S "(localdb)\mssqllocaldb" -d ProyectoNetDb

# Ver todas las tablas
SELECT name FROM sys.tables;
GO

# Ver usuarios
SELECT * FROM Users;
GO

# Salir
quit
```

## Tablas Principales del Sistema

Cuando ejecutes las migraciones, se crearán estas tablas:

### Tabla: **Tenants**
```sql
Id (int, PK)
Name (nvarchar)
Description (nvarchar)
CreatedAt (datetime2)
UpdatedAt (datetime2)
```

### Tabla: **Users**
```sql
Id (int, PK)
TenantId (int, FK)
Email (nvarchar)
PasswordHash (nvarchar)
PersonalData_FirstName (nvarchar) -- Owned Entity
PersonalData_LastName (nvarchar)  -- Owned Entity
PersonalData_BirthDate (date)     -- Owned Entity
CredentialId (int, FK, nullable)
CreatedAt (datetime2)
UpdatedAt (datetime2)
```

### Tabla: **Roles**
```sql
Id (int, PK)
TenantId (int, FK)
Name (nvarchar)
Description (nvarchar)
CreatedAt (datetime2)
UpdatedAt (datetime2)
```

### Tabla: **Credentials**
```sql
Id (int, PK)
TenantId (int, FK)
UserId (int, FK)
Code (nvarchar)
IsActive (bit)
ExpirationDate (datetime2)
CreatedAt (datetime2)
UpdatedAt (datetime2)
```

### Tabla: **Spaces**
```sql
Id (int, PK)
TenantId (int, FK)
Name (nvarchar)
Description (nvarchar)
SpaceTypeId (int, FK)
Location_Address (nvarchar)     -- Owned Entity
Location_Latitude (float)       -- Owned Entity
Location_Longitude (float)      -- Owned Entity
CreatedAt (datetime2)
UpdatedAt (datetime2)
```

### Tabla: **AccessEvents**
```sql
Id (int, PK)
TenantId (int, FK)
UserId (int, FK)
ControlPointId (int, FK)
Timestamp (datetime2)
Result (int) -- Enum: Granted, Denied, etc.
CreatedAt (datetime2)
UpdatedAt (datetime2)
```

Y más tablas para:
- **SpaceTypes** - Tipos de espacios
- **ControlPoints** - Puntos de control de acceso
- **AccessRules** - Reglas de acceso
- **News** - Noticias
- **Notifications** - Notificaciones
- **Consumptions** - Consumos
- **Benefits** - Beneficios
- **BenefitTypes** - Tipos de beneficios
- **Usages** - Usos

## Consultas Útiles

He creado un script SQL en `scripts/revisar_base_datos.sql` que puedes ejecutar para:
- Ver todas las tablas
- Ver la estructura de cada tabla
- Ver las relaciones (Foreign Keys)
- Ver datos de ejemplo

### Consultas Rápidas:

```sql
-- Ver todos los tenants
SELECT * FROM Tenants;

-- Ver todos los usuarios con su tenant
SELECT 
    u.Id,
    u.Email,
    t.Name AS TenantName,
    u.CreatedAt
FROM Users u
INNER JOIN Tenants t ON u.TenantId = t.Id;

-- Contar usuarios por tenant
SELECT 
    t.Name AS TenantName,
    COUNT(u.Id) AS TotalUsuarios
FROM Tenants t
LEFT JOIN Users u ON t.Id = u.TenantId
GROUP BY t.Name;

-- Ver la estructura de una tabla específica
EXEC sp_help 'Users';

-- Ver todas las columnas de Users
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users';
```

## Comandos para Gestionar Migraciones

```bash
# Ver migraciones aplicadas
dotnet ef migrations list --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj

# Ver el script SQL que se aplicará
dotnet ef migrations script --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj

# Revertir la última migración
dotnet ef database update [MigrationName] --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj

# Eliminar la base de datos (CUIDADO!)
dotnet ef database drop --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj
```

## Ubicación de LocalDB

LocalDB guarda las bases de datos en:
```
C:\Users\[TuUsuario]\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\mssqllocaldb
```

## Verificar que LocalDB esté corriendo

```bash
# Ver todas las instancias de LocalDB
sqllocaldb info

# Ver info de la instancia mssqllocaldb
sqllocaldb info mssqllocaldb

# Iniciar LocalDB si no está corriendo
sqllocaldb start mssqllocaldb

# Detener LocalDB
sqllocaldb stop mssqllocaldb
```

## Tips

1. **Ver datos en tiempo real:** Usa Azure Data Studio o SSMS para explorar las tablas mientras desarrollas
2. **Entity Framework Core Tools:** Puedes generar un diagrama de la base de datos con herramientas como EF Core Power Tools
3. **Seed Data:** Considera crear un script de seed data para tener datos de prueba automáticamente
4. **Backups:** LocalDB es para desarrollo, para producción usa SQL Server completo o Azure SQL

## Diagrama ER (Conceptual)

```
Tenants (1) ──< Users (N)
              │
              └──< AccessEvents (N)
              │
              └──< Credentials (N)
              │
              └──< Roles (N)

Tenants (1) ──< Spaces (N) ──< ControlPoints (N)
              │
              └──< AccessEvents (N)
```

¡Ya tienes todo para explorar tu base de datos! 🎉
-- Script para revisar la estructura de la base de datos ProyectoNetDb
-- Ejecuta esto en SQL Server Management Studio o Azure Data Studio

USE ProyectoNetDb;
GO

-- ============================================
-- Ver todas las tablas de la base de datos
-- ============================================
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

-- ============================================
-- Ver la estructura de la tabla Tenants
-- ============================================
EXEC sp_help 'Tenants';
GO

-- ============================================
-- Ver la estructura de la tabla Users
-- ============================================
EXEC sp_help 'Users';
GO

-- ============================================
-- Ver todas las columnas de todas las tablas
-- ============================================
SELECT 
    t.TABLE_NAME,
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.IS_NULLABLE,
    c.COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.TABLES t
INNER JOIN INFORMATION_SCHEMA.COLUMNS c 
    ON t.TABLE_NAME = c.TABLE_NAME
WHERE t.TABLE_TYPE = 'BASE TABLE'
ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION;
GO

-- ============================================
-- Ver todas las relaciones (Foreign Keys)
-- ============================================
SELECT 
    FK.name AS ForeignKey_Name,
    OBJECT_NAME(FK.parent_object_id) AS Table_Name,
    COL_NAME(FKC.parent_object_id, FKC.parent_column_id) AS Column_Name,
    OBJECT_NAME(FK.referenced_object_id) AS Referenced_Table,
    COL_NAME(FKC.referenced_object_id, FKC.referenced_column_id) AS Referenced_Column
FROM sys.foreign_keys AS FK
INNER JOIN sys.foreign_key_columns AS FKC 
    ON FK.object_id = FKC.constraint_object_id
ORDER BY Table_Name, ForeignKey_Name;
GO

-- ============================================
-- Ver datos de ejemplo de Tenants
-- ============================================
SELECT * FROM Tenants;
GO

-- ============================================
-- Ver datos de ejemplo de Users
-- ============================================
SELECT * FROM Users;
GO

-- ============================================
-- Consulta útil: Ver usuarios con su tenant
-- ============================================
SELECT 
    u.Id,
    u.Email,
    u.CreatedAt,
    u.UpdatedAt,
    t.Id AS TenantId,
    t.Name AS TenantName
FROM Users u
INNER JOIN Tenants t ON u.TenantId = t.Id
ORDER BY t.Name, u.Email;
GO

