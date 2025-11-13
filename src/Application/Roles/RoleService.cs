using Application.Common.Interfaces;
using Shared.DTOs.Roles;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Roles;

/// <summary>
/// Implementation of role service for managing role operations.
/// </summary>
public class RoleService : IRoleService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public RoleService(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<RoleResponse> CreateRoleAsync(RoleRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Verify tenant exists
        var tenantExists = await _context.Tenants
            .AnyAsync(t => t.Id == tenantId, cancellationToken);

        if (!tenantExists)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} does not exist.");
        }

        // Check if role with same name already exists in this tenant
        var existingRole = await _context.Roles
            .Where(r => r.TenantId == tenantId && r.Name.ToLower() == request.Name.ToLower())
            .FirstOrDefaultAsync(cancellationToken);

        if (existingRole != null)
        {
            throw new InvalidOperationException($"Role with name '{request.Name}' already exists in this tenant.");
        }

        // Create the role entity
        var role = new Role(tenantId, request.Name);

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            TenantId = role.TenantId,
            UserCount = 0,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task<RoleResponse?> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var role = await _context.Roles
            .Where(r => r.Id == id && r.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (role == null)
            return null;

        var userCount = await _context.Users
            .Where(u => u.TenantId == tenantId && u.Roles.Any(r => r.Id == id))
            .CountAsync(cancellationToken);

        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            TenantId = role.TenantId,
            UserCount = userCount,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task<IEnumerable<RoleResponse>> GetRolesByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var roles = await _context.Roles
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var roleResponses = new List<RoleResponse>();
        
        foreach (var role in roles)
        {
            var userCount = await _context.Users
                .Where(u => u.TenantId == tenantId && u.Roles.Any(r => r.Id == role.Id))
                .CountAsync(cancellationToken);

            roleResponses.Add(new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                TenantId = role.TenantId,
                UserCount = userCount,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            });
        }

        return roleResponses;
    }

    public async Task<RoleResponse> UpdateRoleAsync(int id, RoleRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var role = await _context.Roles
            .Where(r => r.Id == id && r.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (role == null)
        {
            throw new InvalidOperationException($"Role with ID {id} not found in current tenant.");
        }

        if (role.Name.ToLower() != request.Name.ToLower())
        {
            var nameExists = await _context.Roles
                .AnyAsync(r => r.Name.ToLower() == request.Name.ToLower() && r.TenantId == tenantId && r.Id != id, cancellationToken);

            if (nameExists)
            {
                throw new InvalidOperationException($"Role with name '{request.Name}' already exists in this tenant.");
            }

            role.UpdateName(request.Name);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var userCount = await _context.Users
            .Where(u => u.TenantId == tenantId && u.Roles.Any(r => r.Id == id))
            .CountAsync(cancellationToken);

        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            TenantId = role.TenantId,
            UserCount = userCount,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task<bool> DeleteRoleAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var role = await _context.Roles
            .Where(r => r.Id == id && r.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (role == null)
        {
            return false;
        }

        var userCount = await _context.Users
            .Where(u => u.TenantId == tenantId && u.Roles.Any(r => r.Id == id))
            .CountAsync(cancellationToken);

        if (userCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete role '{role.Name}' because it has {userCount} user(s) assigned. Please remove all users from this role first.");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task AssignRolesToUserAsync(int userId, AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Find the user
        var user = await _context.Users
            .Include(u => u.Roles)
            .Where(u => u.Id == userId && u.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found in current tenant.");
        }

        // Get all roles to assign (validate they exist and belong to current tenant)
        var rolesToAssign = await _context.Roles
            .Where(r => request.RoleIds.Contains(r.Id) && r.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        if (rolesToAssign.Count != request.RoleIds.Count)
        {
            throw new InvalidOperationException("One or more role IDs are invalid or don't belong to the current tenant.");
        }

        // Clear existing roles and assign new ones
        user.Roles.Clear();
        foreach (var role in rolesToAssign)
        {
            user.Roles.Add(role);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<RoleResponse>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var user = await _context.Users
            .Include(u => u.Roles)
            .Where(u => u.Id == userId && u.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found in current tenant.");
        }

        return user.Roles.Select(r => new RoleResponse
        {
            Id = r.Id,
            Name = r.Name,
            TenantId = r.TenantId,
            UserCount = 0,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        });
    }
}
