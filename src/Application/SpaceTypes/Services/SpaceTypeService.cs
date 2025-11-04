using Application.Common.Interfaces;
using Application.SpaceTypes.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.SpaceTypes.Services;

/// <summary>
/// Implementation of space type service for managing space type operations.
/// </summary>
public class SpaceTypeService : ISpaceTypeService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public SpaceTypeService(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<SpaceTypeResponse> CreateSpaceTypeAsync(CreateSpaceTypeRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Verify tenant exists
        var tenantExists = await _context.Tenants
            .AnyAsync(t => t.Id == tenantId, cancellationToken);

        if (!tenantExists)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} does not exist.");
        }

        // Check if space type with same name already exists in this tenant
        var existingSpaceType = await _context.SpaceTypes
            .Where(st => st.TenantId == tenantId && st.Name.ToLower() == request.Name.ToLower())
            .FirstOrDefaultAsync(cancellationToken);

        if (existingSpaceType != null)
        {
            throw new InvalidOperationException($"Space type with name '{request.Name}' already exists in this tenant.");
        }

        // Create the space type entity
        var spaceType = new SpaceType(tenantId, request.Name);

        _context.SpaceTypes.Add(spaceType);
        await _context.SaveChangesAsync(cancellationToken);

        return new SpaceTypeResponse
        {
            Id = spaceType.Id,
            Name = spaceType.Name,
            TenantId = spaceType.TenantId,
            SpaceCount = 0,
            CreatedAt = spaceType.CreatedAt,
            UpdatedAt = spaceType.UpdatedAt
        };
    }

    public async Task<SpaceTypeResponse?> GetSpaceTypeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var spaceType = await _context.SpaceTypes
            .Where(st => st.Id == id && st.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (spaceType == null)
            return null;

        var spaceCount = await _context.Spaces
            .Where(s => s.TenantId == tenantId && s.SpaceType != null && s.SpaceType.Id == id)
            .CountAsync(cancellationToken);

        return new SpaceTypeResponse
        {
            Id = spaceType.Id,
            Name = spaceType.Name,
            TenantId = spaceType.TenantId,
            SpaceCount = spaceCount,
            CreatedAt = spaceType.CreatedAt,
            UpdatedAt = spaceType.UpdatedAt
        };
    }

    public async Task<IEnumerable<SpaceTypeResponse>> GetSpaceTypesByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var spaceTypes = await _context.SpaceTypes
            .Where(st => st.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var spaceTypeResponses = new List<SpaceTypeResponse>();
        
        foreach (var spaceType in spaceTypes)
        {
            var spaceCount = await _context.Spaces
                .Where(s => s.TenantId == tenantId && s.SpaceType != null && s.SpaceType.Id == spaceType.Id)
                .CountAsync(cancellationToken);

            spaceTypeResponses.Add(new SpaceTypeResponse
            {
                Id = spaceType.Id,
                Name = spaceType.Name,
                TenantId = spaceType.TenantId,
                SpaceCount = spaceCount,
                CreatedAt = spaceType.CreatedAt,
                UpdatedAt = spaceType.UpdatedAt
            });
        }

        return spaceTypeResponses;
    }

    public async Task<SpaceTypeResponse> UpdateSpaceTypeAsync(int id, UpdateSpaceTypeRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var spaceType = await _context.SpaceTypes
            .Where(st => st.Id == id && st.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (spaceType == null)
        {
            throw new InvalidOperationException($"Space type with ID {id} not found in current tenant.");
        }

        if (spaceType.Name.ToLower() != request.Name.ToLower())
        {
            var nameExists = await _context.SpaceTypes
                .AnyAsync(st => st.Name.ToLower() == request.Name.ToLower() && st.TenantId == tenantId && st.Id != id, cancellationToken);

            if (nameExists)
            {
                throw new InvalidOperationException($"Space type with name '{request.Name}' already exists in this tenant.");
            }

            spaceType.UpdateName(request.Name);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var spaceCount = await _context.Spaces
            .Where(s => s.TenantId == tenantId && s.SpaceType != null && s.SpaceType.Id == id)
            .CountAsync(cancellationToken);

        return new SpaceTypeResponse
        {
            Id = spaceType.Id,
            Name = spaceType.Name,
            TenantId = spaceType.TenantId,
            SpaceCount = spaceCount,
            CreatedAt = spaceType.CreatedAt,
            UpdatedAt = spaceType.UpdatedAt
        };
    }

    public async Task<bool> DeleteSpaceTypeAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var spaceType = await _context.SpaceTypes
            .Where(st => st.Id == id && st.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (spaceType == null)
        {
            return false;
        }

        var spaceCount = await _context.Spaces
            .Where(s => s.TenantId == tenantId && s.SpaceType != null && s.SpaceType.Id == id)
            .CountAsync(cancellationToken);

        if (spaceCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete space type '{spaceType.Name}' because it has {spaceCount} space(s) assigned. Please remove all spaces from this space type first.");
        }

        _context.SpaceTypes.Remove(spaceType);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
