using Application.Common.Interfaces;
using Application.ControlPoints.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.ControlPoints.Services;

/// <summary>
/// Implementation of control point service for managing control point operations.
/// </summary>
public class ControlPointService : IControlPointService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ControlPointService(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<ControlPointResponse> CreateControlPointAsync(CreateControlPointRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Verify tenant exists
        var tenantExists = await _context.Tenants
            .AnyAsync(t => t.Id == tenantId, cancellationToken);

        if (!tenantExists)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} does not exist.");
        }

        // Verify space exists and belongs to the same tenant
        var space = await _context.Spaces
            .Include(s => s.SpaceType)
            .Where(s => s.Id == request.SpaceId && s.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (space == null)
        {
            throw new InvalidOperationException($"Space with ID {request.SpaceId} not found in current tenant.");
        }

        // Check if control point with same name already exists in this space
        var existingControlPoint = await _context.ControlPoints
            .Where(cp => cp.SpaceId == request.SpaceId && cp.Name.ToLower() == request.Name.ToLower())
            .FirstOrDefaultAsync(cancellationToken);

        if (existingControlPoint != null)
        {
            throw new InvalidOperationException($"Control point with name '{request.Name}' already exists in space '{space.Name}'.");
        }

        // Create the control point entity
        var controlPoint = new ControlPoint(tenantId, request.Name, request.SpaceId);

        _context.ControlPoints.Add(controlPoint);
        await _context.SaveChangesAsync(cancellationToken);

        return new ControlPointResponse
        {
            Id = controlPoint.Id,
            Name = controlPoint.Name,
            SpaceId = controlPoint.SpaceId,
            SpaceName = space.Name,
            SpaceTypeName = space.SpaceType.Name,
            TenantId = controlPoint.TenantId,
            AccessRuleCount = 0,
            AccessEventCount = 0,
            CreatedAt = controlPoint.CreatedAt,
            UpdatedAt = controlPoint.UpdatedAt
        };
    }

    public async Task<ControlPointResponse?> GetControlPointByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var controlPoint = await _context.ControlPoints
            .Include(cp => cp.Space)
            .ThenInclude(s => s.SpaceType)
            .Include(cp => cp.AccessRules)
            .Include(cp => cp.AccessEvents)
            .Where(cp => cp.Id == id && cp.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (controlPoint == null)
            return null;

        return new ControlPointResponse
        {
            Id = controlPoint.Id,
            Name = controlPoint.Name,
            SpaceId = controlPoint.SpaceId,
            SpaceName = controlPoint.Space.Name,
            SpaceTypeName = controlPoint.Space.SpaceType.Name,
            TenantId = controlPoint.TenantId,
            AccessRuleCount = controlPoint.AccessRules.Count,
            AccessEventCount = controlPoint.AccessEvents.Count,
            CreatedAt = controlPoint.CreatedAt,
            UpdatedAt = controlPoint.UpdatedAt
        };
    }

    public async Task<IEnumerable<ControlPointResponse>> GetControlPointsByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var controlPoints = await _context.ControlPoints
            .Include(cp => cp.Space)
            .ThenInclude(s => s.SpaceType)
            .Include(cp => cp.AccessRules)
            .Include(cp => cp.AccessEvents)
            .Where(cp => cp.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return controlPoints.Select(controlPoint => new ControlPointResponse
        {
            Id = controlPoint.Id,
            Name = controlPoint.Name,
            SpaceId = controlPoint.SpaceId,
            SpaceName = controlPoint.Space.Name,
            SpaceTypeName = controlPoint.Space.SpaceType.Name,
            TenantId = controlPoint.TenantId,
            AccessRuleCount = controlPoint.AccessRules.Count,
            AccessEventCount = controlPoint.AccessEvents.Count,
            CreatedAt = controlPoint.CreatedAt,
            UpdatedAt = controlPoint.UpdatedAt
        }).ToList();
    }

    public async Task<ControlPointResponse> UpdateControlPointAsync(int id, UpdateControlPointRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var controlPoint = await _context.ControlPoints
            .Include(cp => cp.Space)
            .ThenInclude(s => s.SpaceType)
            .Where(cp => cp.Id == id && cp.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (controlPoint == null)
        {
            throw new InvalidOperationException($"Control point with ID {id} not found in current tenant.");
        }

        // Verify new space exists and belongs to the same tenant (if changed)
        if (controlPoint.SpaceId != request.SpaceId)
        {
            var space = await _context.Spaces
                .Include(s => s.SpaceType)
                .Where(s => s.Id == request.SpaceId && s.TenantId == tenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (space == null)
            {
                throw new InvalidOperationException($"Space with ID {request.SpaceId} not found in current tenant.");
            }

            controlPoint.MoveToSpace(request.SpaceId);
        }

        // Check if new name conflicts with existing control point in the target space
        if (controlPoint.Name.ToLower() != request.Name.ToLower())
        {
            var nameExists = await _context.ControlPoints
                .AnyAsync(cp => cp.Name.ToLower() == request.Name.ToLower() 
                    && cp.SpaceId == request.SpaceId 
                    && cp.TenantId == tenantId 
                    && cp.Id != id, cancellationToken);

            if (nameExists)
            {
                throw new InvalidOperationException($"Control point with name '{request.Name}' already exists in the target space.");
            }

            controlPoint.UpdateName(request.Name);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Reload space and collections
        var updatedControlPoint = await _context.ControlPoints
            .Include(cp => cp.Space)
            .ThenInclude(s => s.SpaceType)
            .Include(cp => cp.AccessRules)
            .Include(cp => cp.AccessEvents)
            .Where(cp => cp.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return new ControlPointResponse
        {
            Id = updatedControlPoint!.Id,
            Name = updatedControlPoint.Name,
            SpaceId = updatedControlPoint.SpaceId,
            SpaceName = updatedControlPoint.Space.Name,
            SpaceTypeName = updatedControlPoint.Space.SpaceType.Name,
            TenantId = updatedControlPoint.TenantId,
            AccessRuleCount = updatedControlPoint.AccessRules.Count,
            AccessEventCount = updatedControlPoint.AccessEvents.Count,
            CreatedAt = updatedControlPoint.CreatedAt,
            UpdatedAt = controlPoint.UpdatedAt
        };
    }

    public async Task<bool> DeleteControlPointAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var controlPoint = await _context.ControlPoints
            .Include(cp => cp.AccessRules)
            .Include(cp => cp.AccessEvents)
            .Where(cp => cp.Id == id && cp.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (controlPoint == null)
        {
            return false;
        }

        if (controlPoint.AccessRules.Any())
        {
            throw new InvalidOperationException($"Cannot delete control point '{controlPoint.Name}' because it has {controlPoint.AccessRules.Count} access rule(s) assigned. Please remove all access rules from this control point first.");
        }

        if (controlPoint.AccessEvents.Any())
        {
            throw new InvalidOperationException($"Cannot delete control point '{controlPoint.Name}' because it has {controlPoint.AccessEvents.Count} access event(s) recorded. Control points with historical data cannot be deleted.");
        }

        _context.ControlPoints.Remove(controlPoint);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
