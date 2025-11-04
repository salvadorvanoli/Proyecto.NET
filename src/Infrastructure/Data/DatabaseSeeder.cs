using Application.Common.Interfaces;
using Domain.DataTypes;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Data;

/// <summary>
/// Seeds initial data for development environment only.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        // Seed in development OR if explicitly enabled via environment variable
        var shouldSeed = environment.IsDevelopment() || 
                        Environment.GetEnvironmentVariable("SEED_DATABASE") == "true";
        
        if (!shouldSeed)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Database should already be migrated by Program.cs
        // Just ensure it exists
        await context.Database.EnsureCreatedAsync();

        // Seed Tenants
        if (!await context.Tenants.AnyAsync())
        {
            var tenant = new Tenant("Tenant Demo");
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
            
            Console.WriteLine($"Tenant creado: {tenant.Name} (ID: {tenant.Id})");
        }

        // Seed Admin Users for BackOffice
        var backofficeAdminExists = await context.Users
            .AnyAsync(u => u.Email.StartsWith("admin") && u.Email.EndsWith("@backoffice.com"));

        if (!backofficeAdminExists)
        {
            Console.WriteLine("Creando usuario admin para BackOffice...");

            var tenant = await context.Tenants.FirstAsync();
            var passwordHash = passwordHasher.HashPassword("Admin123!");
            var personalData = new PersonalData(
                "Administrador",
                "BackOffice",
                new DateOnly(1990, 1, 1)
            );

            var adminUser = new User(
                tenant.Id,
                $"admin@backoffice.com",
                passwordHash,
                personalData
            );

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            Console.WriteLine($"   Usuario creado: admin@backoffice.com");

            // Seed Role for BackOffice
            var adminRole = new Role(tenant.Id, "AdministradorBackoffice");
            context.Roles.Add(adminRole);
            await context.SaveChangesAsync();

            // Reload admin user with ID
            var savedAdminUser = await context.Users
                .FirstAsync(u => u.Email == "admin@backoffice.com");

            savedAdminUser.AssignRole(adminRole);
            await context.SaveChangesAsync();

            Console.WriteLine("Usuario admin de BackOffice creado exitosamente!");
        }
        else
        {
            Console.WriteLine("Usuario admin de BackOffice ya existe, omitiendo creacion.");
        }

        Console.WriteLine("\n📋 Usuarios de BackOffice disponibles:");
        var backofficeUsers = await context.Users
            .Include(u => u.Roles)
            .Where(u => u.Email.Contains("@backoffice.com"))
            .ToListAsync();

        foreach (var user in backofficeUsers)
        {
            var roles = string.Join(", ", user.Roles.Select(r => r.Name));
            Console.WriteLine($"   Email: {user.Email} | Password: Admin123! | TenantId: {user.TenantId} | Roles: {roles}");
        }

        var otherUsersCount = await context.Users
            .Where(u => !u.Email.Contains("@backoffice.com"))
            .CountAsync();
            
        if (otherUsersCount > 0)
        {
            Console.WriteLine($"\nOtros usuarios en el sistema: {otherUsersCount}");
        }
    }
}
