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

        // Seed Benefit Types
        if (!await context.BenefitTypes.AnyAsync())
        {
            Console.WriteLine("\n🎁 Creating benefit types...");
            
            var benefitTypes = new List<BenefitType>();
            var tenants = await context.Tenants.ToListAsync();

            foreach (var tenant in tenants)
            {
                benefitTypes.AddRange(new[]
                {
                    new BenefitType(tenant.Id, "Descuento Gimnasio", "20% de descuento en todas las membresías mensuales del gimnasio"),
                    new BenefitType(tenant.Id, "Estacionamiento Gratuito", "Acceso gratuito al estacionamiento durante horarios laborales"),
                    new BenefitType(tenant.Id, "Cafetería VIP", "Bebida gratis por día en la cafetería del edificio"),
                    new BenefitType(tenant.Id, "Día Libre Extra", "Día libre adicional al año por antigüedad")
                });
            }

            context.BenefitTypes.AddRange(benefitTypes);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Benefit types created successfully!");
        }

        // Seed Benefits
        if (!await context.Benefits.AnyAsync())
        {
            Console.WriteLine("\n🎁 Creating benefits...");
            
            var benefits = new List<Benefit>();
            var benefitTypes = await context.BenefitTypes.ToListAsync();

            foreach (var benefitType in benefitTypes)
            {
                var validityPeriod = new DateRange(
                    new DateTime(2025, 1, 1),
                    new DateTime(2025, 12, 31)
                );

                var benefit = new Benefit(
                    benefitType.TenantId,
                    benefitType.Id,
                    quotas: 50,
                    validityPeriod: validityPeriod
                );

                benefits.Add(benefit);
            }

            context.Benefits.AddRange(benefits);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Benefits created successfully!");
        }

        // Seed Consumptions (sample data for some users)
        if (!await context.Consumptions.AnyAsync())
        {
            Console.WriteLine("\n📊 Creating sample consumptions...");
            
            var allUsers = await context.Users.ToListAsync();
            var allBenefits = await context.Benefits.Include(b => b.BenefitType).ToListAsync();

            var consumptions = new List<Consumption>();

            // Create some sample consumptions for the first user of each tenant
            foreach (var tenant in await context.Tenants.ToListAsync())
            {
                var firstUser = allUsers.FirstOrDefault(u => u.TenantId == tenant.Id);
                if (firstUser == null) continue;

                var tenantBenefits = allBenefits.Where(b => b.TenantId == tenant.Id).ToList();
                
                // Add consumption for first 2 benefits
                for (int i = 0; i < Math.Min(2, tenantBenefits.Count); i++)
                {
                    var benefit = tenantBenefits[i];
                    var consumption = new Consumption(
                        tenant.Id,
                        amount: 1,
                        benefitId: benefit.Id,
                        userId: firstUser.Id
                    );

                    consumptions.Add(consumption);
                }
            }

            context.Consumptions.AddRange(consumptions);
            await context.SaveChangesAsync();

            // Add usages for consumptions
            var savedConsumptions = await context.Consumptions.ToListAsync();
            var usages = new List<Usage>();

            foreach (var consumption in savedConsumptions)
            {
                // Add 1-3 usages per consumption
                var usageCount = new Random().Next(1, 4);
                for (int i = 0; i < usageCount; i++)
                {
                    var usage = new Usage(
                        consumption.TenantId,
                        consumption.Id,
                        DateTime.UtcNow.AddDays(-new Random().Next(1, 30))
                    );
                    usages.Add(usage);
                }
            }

            context.Usages.AddRange(usages);
            await context.SaveChangesAsync();
            
            Console.WriteLine("✅ Sample consumptions created successfully!");
        }

        // Seed Space Types
        if (!await context.SpaceTypes.AnyAsync())
        {
            Console.WriteLine("\n🏢 Creating space types...");
            
            var spaceTypes = new List<SpaceType>();
            var tenants = await context.Tenants.ToListAsync();

            foreach (var tenant in tenants)
            {
                spaceTypes.AddRange(new[]
                {
                    new SpaceType(tenant.Id, "Oficina"),
                    new SpaceType(tenant.Id, "Estacionamiento"),
                    new SpaceType(tenant.Id, "Área Común"),
                    new SpaceType(tenant.Id, "Área Restringida")
                });
            }

            context.SpaceTypes.AddRange(spaceTypes);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Space types created successfully!");
        }

        // Seed Spaces
        if (!await context.Spaces.AnyAsync())
        {
            Console.WriteLine("\n🏢 Creating spaces...");
            
            var spaces = new List<Space>();
            var spaceTypes = await context.SpaceTypes.ToListAsync();

            foreach (var tenant in await context.Tenants.ToListAsync())
            {
                var tenantSpaceTypes = spaceTypes.Where(st => st.TenantId == tenant.Id).ToList();
                
                if (tenantSpaceTypes.Any())
                {
                    var location = new Domain.DataTypes.Location("Av. Principal", "100", "Montevideo", "Uruguay");
                    spaces.Add(new Space(tenant.Id, "Entrada Principal", location, tenantSpaceTypes[0].Id));
                    
                    var parkingLocation = new Domain.DataTypes.Location("Av. Principal", "100-B", "Montevideo", "Uruguay");
                    spaces.Add(new Space(tenant.Id, "Estacionamiento Subterráneo", parkingLocation, tenantSpaceTypes[1].Id));
                    
                    var restrictedLocation = new Domain.DataTypes.Location("Av. Principal", "100-C", "Montevideo", "Uruguay");
                    spaces.Add(new Space(tenant.Id, "Laboratorio Seguro", restrictedLocation, tenantSpaceTypes[3].Id));
                }
            }

            context.Spaces.AddRange(spaces);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Spaces created successfully!");
        }

        // Seed Control Points
        if (!await context.ControlPoints.AnyAsync())
        {
            Console.WriteLine("\n🚪 Creating control points...");
            
            var controlPoints = new List<ControlPoint>();
            var spaces = await context.Spaces.ToListAsync();

            foreach (var space in spaces)
            {
                if (space.Name.Contains("Entrada"))
                {
                    controlPoints.Add(new ControlPoint(space.TenantId, "Entrada Principal", space.Id));
                    controlPoints.Add(new ControlPoint(space.TenantId, "Salida Principal", space.Id));
                }
                else if (space.Name.Contains("Estacionamiento"))
                {
                    controlPoints.Add(new ControlPoint(space.TenantId, "Entrada Estacionamiento", space.Id));
                    controlPoints.Add(new ControlPoint(space.TenantId, "Salida Estacionamiento", space.Id));
                }
                else if (space.Name.Contains("Laboratorio"))
                {
                    controlPoints.Add(new ControlPoint(space.TenantId, "Área Restringida", space.Id));
                }
            }

            context.ControlPoints.AddRange(controlPoints);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Control points created successfully!");
        }

        // Seed Access Events
        if (!await context.AccessEvents.AnyAsync())
        {
            Console.WriteLine("\n🔐 Creating sample access events...");
            
            var allUsers = await context.Users.ToListAsync();
            var allControlPoints = await context.ControlPoints.ToListAsync();
            var accessEvents = new List<AccessEvent>();
            var random = new Random();

            foreach (var tenant in await context.Tenants.ToListAsync())
            {
                var firstUser = allUsers.FirstOrDefault(u => u.TenantId == tenant.Id);
                if (firstUser == null) continue;

                var tenantControlPoints = allControlPoints.Where(cp => cp.TenantId == tenant.Id).ToList();
                
                // Create 10 access events for each user
                for (int i = 0; i < 10; i++)
                {
                    var controlPoint = tenantControlPoints[random.Next(tenantControlPoints.Count)];
                    var result = controlPoint.Name.Contains("Restringida") && random.Next(3) == 0 
                        ? Domain.Enums.AccessResult.Denied 
                        : Domain.Enums.AccessResult.Granted;
                    
                    var eventDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)).AddHours(random.Next(8, 20));
                    
                    accessEvents.Add(new AccessEvent(
                        tenant.Id,
                        eventDate,
                        result,
                        controlPoint.Id,
                        firstUser.Id
                    ));
                }
            }

            context.AccessEvents.AddRange(accessEvents);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Sample access events created successfully!");
        }
    }
}
