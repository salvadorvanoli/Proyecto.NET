﻿using Application.Common.Interfaces;
using Domain.DataTypes;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

/// <summary>
/// Seeds initial data into the database for development and production environments.
/// </summary>
public class DbSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DbSeeder> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public DbSeeder(ApplicationDbContext context, ILogger<DbSeeder> logger, IPasswordHasher passwordHasher)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Seeds the database with initial data if it's empty.
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Verificar si ya hay datos
            if (await _context.Tenants.AnyAsync())
            {
                _logger.LogInformation("Database already contains data. Skipping seed.");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            // Crear Tenant por defecto
            var tenant = new Tenant("Tenant Principal");
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created default tenant: {TenantName} (ID: {TenantId})", tenant.Name, tenant.Id);

            // Crear Roles
            var adminRole = new Role(tenant.Id, "AdministradorBackoffice");
            var userRole = new Role(tenant.Id, "Usuario");

            _context.Roles.Add(adminRole);
            _context.Roles.Add(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created roles: AdministradorBackoffice, Usuario");

            // Crear Usuario Administrador
            var personalData = new PersonalData(
                firstName: "Administrador",
                lastName: "Sistema",
                birthDate: new DateOnly(1990, 1, 1)
            );

            // Hash de la contraseña "Admin123!" usando el PasswordHasher del sistema
            var passwordHash = _passwordHasher.HashPassword("Admin123!");

            var adminUser = new User(
                tenantId: tenant.Id,
                email: "admin1@backoffice.com",
                passwordHash: passwordHash,
                personalData: personalData
            );

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            // Asignar rol de administrador al usuario
            adminUser.AssignRole(adminRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created admin user: {Email}", adminUser.Email);

            // Crear noticia de bienvenida
            var welcomeNews = new News(
                tenantId: tenant.Id,
                title: "¡Bienvenido al Sistema!",
                content: "Este es el sistema de gestión. Has iniciado sesión exitosamente en el BackOffice.",
                publishDate: DateTime.UtcNow,
                imageUrl: null
            );

            _context.News.Add(welcomeNews);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created welcome news article");

            _logger.LogInformation("Database seeding completed successfully!");
            _logger.LogInformation("===========================================");
            _logger.LogInformation("CREDENCIALES DE ACCESO:");
            _logger.LogInformation("Email: admin1@backoffice.com");
            _logger.LogInformation("Password: Admin123!");
            _logger.LogInformation("===========================================");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    /// <summary>
    /// Applies pending migrations to the database.
    /// </summary>
    public async Task MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Checking for pending migrations...");

            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Migrations applied successfully");
            }
            else
            {
                _logger.LogInformation("Database is up to date. No migrations needed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }
}
