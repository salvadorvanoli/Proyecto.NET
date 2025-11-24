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
            Console.WriteLine("🏢 Creando tenants...");

            var tenantIndigo = new Tenant(
                "Universidad Indigo",
                primaryColor: "#0A3D62",
                secondaryColor: "#1976D2",
                accentColor: "#F4C10F",
                logo: null
            );

            var tenantCoral = new Tenant(
                "Universidad Coral",
                primaryColor: "#D35400",
                secondaryColor: "#F39C12",
                accentColor: "#4A235A",
                logo: null
            );

            context.Tenants.AddRange(tenantIndigo, tenantCoral);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Tenant creado: {tenantIndigo.Name} (ID: {tenantIndigo.Id})");
            Console.WriteLine($"   Colores: {tenantIndigo.PrimaryColor}, {tenantIndigo.SecondaryColor}, {tenantIndigo.AccentColor}");
            Console.WriteLine($"✅ Tenant creado: {tenantCoral.Name} (ID: {tenantCoral.Id})");
            Console.WriteLine($"   Colores: {tenantCoral.PrimaryColor}, {tenantCoral.SecondaryColor}, {tenantCoral.AccentColor}");
        }

        // Seed Admin Users for BackOffice (one per tenant)
        var tenants = await context.Tenants.ToListAsync();

        foreach (var tenant in tenants)
        {
            var adminEmail = $"admin@{tenant.Name.Replace(" ", "").ToLower()}.com";
            var adminExists = await context.Users.AnyAsync(u => u.Email == adminEmail);

            if (!adminExists)
            {
                Console.WriteLine($"\n👤 Creando usuario admin para {tenant.Name}...");

                var passwordHash = passwordHasher.HashPassword("Admin123!");
                var personalData = new PersonalData(
                    "Administrador",
                    tenant.Name,
                    new DateOnly(1990, 1, 1)
                );

                var adminUser = new User(
                    tenant.Id,
                    adminEmail,
                    passwordHash,
                    personalData
                );

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                Console.WriteLine($"   Usuario creado: {adminEmail}");

                // Create and assign credential to admin user
                var credential = new Credential(
                    tenantId: tenant.Id,
                    userId: adminUser.Id,
                    issueDate: DateTime.UtcNow,
                    isActive: true
                );

                context.Credentials.Add(credential);
                await context.SaveChangesAsync();

                // Reload user and assign credential
                var savedUser = await context.Users.FirstAsync(u => u.Id == adminUser.Id);
                savedUser.AssignCredential(credential.Id);
                await context.SaveChangesAsync();

                Console.WriteLine($"   Credencial creada y asignada: ID={credential.Id}");

                // Seed Role for BackOffice
                var adminRole = new Role(tenant.Id, "AdministradorBackoffice");
                context.Roles.Add(adminRole);
                await context.SaveChangesAsync();

                // Reload admin user with ID
                var savedAdminUser = await context.Users
                    .FirstAsync(u => u.Email == adminEmail);

                savedAdminUser.AssignRole(adminRole);
                await context.SaveChangesAsync();

                Console.WriteLine($"   ✅ Usuario admin creado exitosamente para {tenant.Name}!");
            }
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
            var allTenants = await context.Tenants.ToListAsync();

            foreach (var tenant in allTenants)
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
                    quantity: 10,
                    validityPeriod: validityPeriod
                );

                benefits.Add(benefit);
            }

            context.Benefits.AddRange(benefits);
            await context.SaveChangesAsync();
            Console.WriteLine("Benefits created successfully!");
        }

        // Note: Usages and Consumptions are now created through the claim/redeem flow
        // No sample data is seeded for these entities

        // Seed Space Types
        if (!await context.SpaceTypes.AnyAsync())
        {
            Console.WriteLine("\n🏢 Creating space types...");

            var spaceTypes = new List<SpaceType>();
            var allTenants = await context.Tenants.ToListAsync();

            foreach (var tenant in allTenants)
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
                    spaces.Add(new Space(tenant.Id, "Entrada Principal", tenantSpaceTypes[0].Id));

                    if (tenantSpaceTypes.Count > 1)
                        spaces.Add(new Space(tenant.Id, "Estacionamiento Subterráneo", tenantSpaceTypes[1].Id));

                    if (tenantSpaceTypes.Count > 3)
                        spaces.Add(new Space(tenant.Id, "Laboratorio Seguro", tenantSpaceTypes[3].Id));
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

        // Seed NFC Testing User with Credential
        var nfcTestUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "nfctest@indigo.com");
        if (nfcTestUser == null)
        {
            Console.WriteLine("\n🔑 Creating NFC testing user...");

            var tenant = await context.Tenants.FirstAsync(); // Universidad Indigo (TenantId: 1)
            var passwordHash = passwordHasher.HashPassword("Test123!");
            var personalData = new PersonalData("Usuario", "NFC Testing", new DateOnly(1995, 5, 15));

            nfcTestUser = new User(tenant.Id, "nfctest@indigo.com", passwordHash, personalData);
            context.Users.Add(nfcTestUser);
            await context.SaveChangesAsync();

            // Assign role
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "AdministradorBackoffice" && r.TenantId == tenant.Id);
            nfcTestUser.AssignRole(adminRole);
            await context.SaveChangesAsync();

            Console.WriteLine($"   Usuario NFC creado: {nfcTestUser.Email}");
        }

        // Ensure user has active credential
        if (nfcTestUser.CredentialId == null)
        {
            Console.WriteLine("\n🆔 Creating credential for NFC testing user...");

            var credential = new Credential(
                tenantId: nfcTestUser.TenantId,
                userId: nfcTestUser.Id,
                issueDate: DateTime.UtcNow,
                isActive: true
            );

            context.Credentials.Add(credential);
            await context.SaveChangesAsync();

            // Reload user and assign credential
            nfcTestUser = await context.Users.FirstAsync(u => u.Id == nfcTestUser.Id);
            nfcTestUser.AssignCredential(credential.Id);
            await context.SaveChangesAsync();

            Console.WriteLine($"   Credencial creada: ID={credential.Id}, Activa={credential.IsActive}");
        }

        // Seed Regular User for Mobile App
        var regularUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "usuario1@mobile.com");
        if (regularUser == null)
        {
            Console.WriteLine("\n📱 Creating regular user for Mobile App...");

            var tenant = await context.Tenants.FirstAsync();
            var passwordHash = passwordHasher.HashPassword("User123!");
            var personalData = new PersonalData("Juan", "Pérez", new DateOnly(1995, 5, 15));

            regularUser = new User(tenant.Id, "usuario1@mobile.com", passwordHash, personalData);
            context.Users.Add(regularUser);
            await context.SaveChangesAsync();

            // Assign role Usuario
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Usuario" && r.TenantId == tenant.Id);
            if (userRole == null)
            {
                userRole = new Role(tenant.Id, "Usuario");
                context.Roles.Add(userRole);
                await context.SaveChangesAsync();
            }

            regularUser.AssignRole(userRole);
            await context.SaveChangesAsync();

            // Create and assign credential
            var credential = new Credential(
                tenantId: tenant.Id,
                userId: regularUser.Id,
                issueDate: DateTime.UtcNow,
                isActive: true
            );

            context.Credentials.Add(credential);
            await context.SaveChangesAsync();

            regularUser = await context.Users.FirstAsync(u => u.Id == regularUser.Id);
            regularUser.AssignCredential(credential.Id);
            await context.SaveChangesAsync();

            Console.WriteLine($"   Usuario Mobile creado: {regularUser.Email}");
            Console.WriteLine($"   Credencial asignada: ID={credential.Id}");
        }

        // Seed Access Rules for Control Points
        var allControlPointsWithRules = await context.ControlPoints.Include(cp => cp.AccessRules).ToListAsync();

        foreach (var controlPoint in allControlPointsWithRules)
        {
            if (!controlPoint.AccessRules.Any())
            {
                Console.WriteLine($"\n🔐 Creating access rule for control point: {controlPoint.Name}...");

                // Create AccessRule with ControlPointId (one-to-many relationship)
                var accessRule = new AccessRule(controlPoint.TenantId, controlPoint.Id);
                context.AccessRules.Add(accessRule);

                // Assign BOTH roles to this access rule
                var adminRole = await context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "AdministradorBackoffice" && r.TenantId == controlPoint.TenantId);

                var userRole = await context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "Usuario" && r.TenantId == controlPoint.TenantId);

                if (adminRole != null)
                {
                    accessRule.Roles.Add(adminRole);
                }

                if (userRole != null)
                {
                    accessRule.Roles.Add(userRole);
                }

                await context.SaveChangesAsync();
                Console.WriteLine($"   AccessRule creada con roles asignados al ControlPoint");
            }
        }

        Console.WriteLine("\n✅ NFC testing setup completed!");
        Console.WriteLine("===========================================");
        Console.WriteLine("MOBILE APP USER:");
        Console.WriteLine($"Email: usuario1@mobile.com");
        Console.WriteLine("Password: User123!");
        Console.WriteLine($"UserId: {regularUser.Id}");
        Console.WriteLine($"CredentialId: {regularUser.CredentialId}");
        Console.WriteLine("===========================================");
        Console.WriteLine("NFC TESTING USER (Tenant 1 - Universidad Indigo):");
        Console.WriteLine($"Email: nfctest@indigo.com");
        Console.WriteLine("Password: Test123!");
        Console.WriteLine($"UserId: {nfcTestUser.Id}");
        Console.WriteLine($"CredentialId: {nfcTestUser.CredentialId}");
        Console.WriteLine($"TenantId: {nfcTestUser.TenantId}");
        Console.WriteLine("===========================================");
    }
}
