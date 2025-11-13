using Application.Common.Interfaces;
using Shared.DTOs.Spaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Spaces;

/// <summary>
/// Implementation of space service for managing space operations.
/// </summary>
public class SpaceService : ISpaceService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public SpaceService(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<SpaceResponse> CreateSpaceAsync(SpaceRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Verify tenant exists
        var tenantExists = await _context.Tenants
            .AnyAsync(t => t.Id == tenantId, cancellationToken);

        if (!tenantExists)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} does not exist.");
        }

        // Verify space type exists and belongs to the same tenant
        var spaceType = await _context.SpaceTypes
            .Where(st => st.Id == request.SpaceTypeId && st.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (spaceType == null)
        {
            throw new InvalidOperationException($"Space type with ID {request.SpaceTypeId} not found in current tenant.");
        }

        // Check if space with same name already exists in this tenant
        var existingSpace = await _context.Spaces
            .Where(s => s.TenantId == tenantId && s.Name.ToLower() == request.Name.ToLower())
            .FirstOrDefaultAsync(cancellationToken);

        if (existingSpace != null)
        {
            throw new InvalidOperationException($"Space with name '{request.Name}' already exists in this tenant.");
        }

        // Create the space entity
        var space = new Space(tenantId, request.Name, request.SpaceTypeId);

        _context.Spaces.Add(space);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload to get navigation properties
        spaceType = await _context.SpaceTypes
            .Where(st => st.Id == space.SpaceTypeId)
            .FirstOrDefaultAsync(cancellationToken);

        return new SpaceResponse
        {
            Id = space.Id,
            Name = space.Name,
            SpaceTypeId = space.SpaceTypeId,
            SpaceTypeName = spaceType?.Name ?? string.Empty,
            TenantId = space.TenantId,
            ControlPointCount = 0,
            CreatedAt = space.CreatedAt,
            UpdatedAt = space.UpdatedAt
        };
    }

    public async Task<SpaceResponse?> GetSpaceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var space = await _context.Spaces
            .Include(s => s.SpaceType)
            .Where(s => s.Id == id && s.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (space == null)
            return null;

        var controlPointCount = await _context.ControlPoints
            .Where(cp => cp.SpaceId == id && cp.TenantId == tenantId)
            .CountAsync(cancellationToken);

        return new SpaceResponse
        {
            Id = space.Id,
            Name = space.Name,
            SpaceTypeId = space.SpaceTypeId,
            SpaceTypeName = space.SpaceType.Name,
            TenantId = space.TenantId,
            ControlPointCount = controlPointCount,
            CreatedAt = space.CreatedAt,
            UpdatedAt = space.UpdatedAt
        };
    }

    public async Task<IEnumerable<SpaceResponse>> GetSpacesByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var spaces = await _context.Spaces
            .Include(s => s.SpaceType)
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var spaceResponses = new List<SpaceResponse>();
        
        foreach (var space in spaces)
        {
            var controlPointCount = await _context.ControlPoints
                .Where(cp => cp.SpaceId == space.Id && cp.TenantId == tenantId)
                .CountAsync(cancellationToken);

            spaceResponses.Add(new SpaceResponse
            {
                Id = space.Id,
                Name = space.Name,
                SpaceTypeId = space.SpaceTypeId,
                SpaceTypeName = space.SpaceType.Name,
                TenantId = space.TenantId,
                ControlPointCount = controlPointCount,
                CreatedAt = space.CreatedAt,
                UpdatedAt = space.UpdatedAt
            });
        }

        return spaceResponses;
    }

    public async Task<SpaceResponse> UpdateSpaceAsync(int id, SpaceRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var space = await _context.Spaces
            .Include(s => s.SpaceType)
            .Where(s => s.Id == id && s.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (space == null)
        {
            throw new InvalidOperationException($"Space with ID {id} not found in current tenant.");
        }

        // Verify new space type exists and belongs to the same tenant (if changed)
        if (space.SpaceTypeId != request.SpaceTypeId)
        {
            var spaceType = await _context.SpaceTypes
                .Where(st => st.Id == request.SpaceTypeId && st.TenantId == tenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (spaceType == null)
            {
                throw new InvalidOperationException($"Space type with ID {request.SpaceTypeId} not found in current tenant.");
            }

            space.ChangeSpaceType(request.SpaceTypeId);
        }

        // Check if new name conflicts with existing space
        if (space.Name.ToLower() != request.Name.ToLower())
        {
            var nameExists = await _context.Spaces
                .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower() && s.TenantId == tenantId && s.Id != id, cancellationToken);

            if (nameExists)
            {
                throw new InvalidOperationException($"Space with name '{request.Name}' already exists in this tenant.");
            }

            space.UpdateInformation(request.Name);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Reload space type if it was changed
        var updatedSpaceType = await _context.SpaceTypes
            .Where(st => st.Id == space.SpaceTypeId)
            .FirstOrDefaultAsync(cancellationToken);

        var controlPointCount = await _context.ControlPoints
            .Where(cp => cp.SpaceId == id && cp.TenantId == tenantId)
            .CountAsync(cancellationToken);

        return new SpaceResponse
        {
            Id = space.Id,
            Name = space.Name,
            SpaceTypeId = space.SpaceTypeId,
            SpaceTypeName = updatedSpaceType?.Name ?? string.Empty,
            TenantId = space.TenantId,
            ControlPointCount = controlPointCount,
            CreatedAt = space.CreatedAt,
            UpdatedAt = space.UpdatedAt
        };
    }

    public async Task<bool> DeleteSpaceAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var space = await _context.Spaces
            .Where(s => s.Id == id && s.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (space == null)
        {
            return false;
        }

        var controlPointCount = await _context.ControlPoints
            .Where(cp => cp.SpaceId == id && cp.TenantId == tenantId)
            .CountAsync(cancellationToken);

        if (controlPointCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete space '{space.Name}' because it has {controlPointCount} control point(s) assigned. Please remove all control points from this space first.");
        }

        _context.Spaces.Remove(space);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
