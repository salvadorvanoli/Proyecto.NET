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
        // Only seed in development
        if (!environment.IsDevelopment())
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Tenants
        if (!await context.Tenants.AnyAsync())
        {
            var tenants = new[]
            {
                new Tenant("Empresa Demo 1"),
                new Tenant("Empresa Demo 2"),
                new Tenant("Empresa Demo 3")
            };

            context.Tenants.AddRange(tenants);
            await context.SaveChangesAsync();
        }

        // Seed Admin Users for BackOffice
        // Check if BackOffice admin users already exist
        var backofficeAdminExists = await context.Users
            .AnyAsync(u => u.Email.StartsWith("admin") && u.Email.EndsWith("@backoffice.com"));

        if (!backofficeAdminExists)
        {
            Console.WriteLine("🔧 Creating BackOffice admin users...");

            var tenants = await context.Tenants.ToListAsync();
            var adminUsers = new List<User>();
            var tenantNames = new[] { "Uno", "Dos", "Tres" };

            for (int i = 0; i < tenants.Count; i++)
            {
                var tenant = tenants[i];
                var passwordHash = passwordHasher.HashPassword("Admin123!");
                var personalData = new PersonalData(
                    "Administrador",
                    $"BackOffice {tenantNames[i]}", // Usar nombre sin números
                    new DateOnly(1990, 1, 1)
                );

                var adminUser = new User(
                    tenant.Id,
                    $"admin{tenant.Id}@backoffice.com",
                    passwordHash,
                    personalData
                );

                adminUsers.Add(adminUser);
                Console.WriteLine($"   Creating user: admin{tenant.Id}@backoffice.com");
            }

            context.Users.AddRange(adminUsers);
            await context.SaveChangesAsync();

            // Seed Roles for BackOffice (one per tenant)
            var adminRoles = new List<Role>();
            foreach (var tenant in tenants)
            {
                // Check if role already exists
                var existingRole = await context.Roles
                    .FirstOrDefaultAsync(r => r.TenantId == tenant.Id && r.Name == "AdministradorBackoffice");

                if (existingRole == null)
                {
                    var adminRole = new Role(tenant.Id, "AdministradorBackoffice");
                    adminRoles.Add(adminRole);
                    context.Roles.Add(adminRole);
                }
                else
                {
                    adminRoles.Add(existingRole);
                }
            }

            await context.SaveChangesAsync();

            // Reload admin users with their IDs
            var savedAdminUsers = await context.Users
                .Where(u => u.Email.StartsWith("admin") && u.Email.EndsWith("@backoffice.com"))
                .OrderBy(u => u.TenantId)
                .ToListAsync();

            // Assign admin role to corresponding admin users
            for (int i = 0; i < savedAdminUsers.Count && i < adminRoles.Count; i++)
            {
                var user = savedAdminUsers[i];
                var role = adminRoles.FirstOrDefault(r => r.TenantId == user.TenantId);

                if (role != null)
                {
                    user.AssignRole(role);
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("✅ BackOffice admin users created successfully!");
        }
        else
        {
            Console.WriteLine("ℹ️  BackOffice admin users already exist, skipping creation.");
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

        Console.WriteLine("\n📋 Otros usuarios en el sistema:");
        var otherUsers = await context.Users
            .Include(u => u.Roles)
            .Where(u => !u.Email.Contains("@backoffice.com"))
            .ToListAsync();

        foreach (var user in otherUsers)
        {
            Console.WriteLine($"   Email: {user.Email} | TenantId: {user.TenantId}");
        }
    }
}
