using Shared.DTOs.AccessRules;
using Application.Common.Interfaces;
using Domain.DataTypes;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.AccessRules;

/// <summary>
/// Service for access rule management.
/// </summary>
public class AccessRuleService : IAccessRuleService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public AccessRuleService(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<AccessRuleResponse?> GetAccessRuleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var accessRule = await _context.AccessRules
            .Include(ar => ar.Roles)
            .Where(ar => ar.Id == id && ar.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (accessRule == null)
            return null;

        // Get control points that have this access rule
        var controlPoints = await _context.ControlPoints
            .Include(cp => cp.AccessRules)
            .Where(cp => cp.TenantId == tenantId && cp.AccessRules.Any(ar => ar.Id == id))
            .ToListAsync(cancellationToken);

        return MapToResponse(accessRule, controlPoints);
    }

    public async Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var accessRules = await _context.AccessRules
            .Include(ar => ar.Roles)
            .Where(ar => ar.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        // Get all control points for this tenant
        var allControlPoints = await _context.ControlPoints
            .Include(cp => cp.AccessRules)
            .Where(cp => cp.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return accessRules.Select(ar =>
        {
            var controlPoints = allControlPoints.Where(cp => cp.AccessRules.Any(rule => rule.Id == ar.Id)).ToList();
            return MapToResponse(ar, controlPoints);
        }).ToList();
    }

    public async Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByControlPointAsync(int controlPointId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var controlPoint = await _context.ControlPoints
            .Include(cp => cp.AccessRules)
            .ThenInclude(ar => ar.Roles)
            .Where(cp => cp.Id == controlPointId && cp.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (controlPoint == null)
            return Enumerable.Empty<AccessRuleResponse>();

        return controlPoint.AccessRules.Select(ar => MapToResponse(ar, new List<ControlPoint> { controlPoint })).ToList();
    }

    public async Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var accessRules = await _context.AccessRules
            .Include(ar => ar.Roles)
            .Where(ar => ar.TenantId == tenantId && ar.Roles.Any(r => r.Id == roleId))
            .ToListAsync(cancellationToken);

        var allControlPoints = await _context.ControlPoints
            .Include(cp => cp.AccessRules)
            .Where(cp => cp.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return accessRules.Select(ar =>
        {
            var controlPoints = allControlPoints.Where(cp => cp.AccessRules.Any(rule => rule.Id == ar.Id)).ToList();
            return MapToResponse(ar, controlPoints);
        }).ToList();
    }

    public async Task<AccessRuleResponse> CreateAccessRuleAsync(AccessRuleRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Validate roles exist and belong to tenant
        var roles = await _context.Roles
            .Where(r => request.RoleIds.Contains(r.Id) && r.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        if (roles.Count != request.RoleIds.Count)
        {
            throw new InvalidOperationException("One or more roles not found or do not belong to the current tenant.");
        }

        // Validate control points exist and belong to tenant
        var controlPoints = await _context.ControlPoints
            .Where(cp => request.ControlPointIds.Contains(cp.Id) && cp.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        if (controlPoints.Count != request.ControlPointIds.Count)
        {
            throw new InvalidOperationException("One or more control points not found or do not belong to the current tenant.");
        }

        // Create time range if provided
        TimeRange? timeRange = null;
        if (!string.IsNullOrWhiteSpace(request.StartTime) && !string.IsNullOrWhiteSpace(request.EndTime))
        {
            timeRange = new TimeRange(request.StartTime, request.EndTime);
        }

        // Create date range if provided
        DateRange? dateRange = null;
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            dateRange = new DateRange(request.StartDate.Value, request.EndDate.Value);
        }

        // Create the access rule
        var accessRule = new AccessRule(tenantId, timeRange, dateRange);

        // Add roles
        foreach (var role in roles)
        {
            accessRule.AddRole(role);
        }

        _context.AccessRules.Add(accessRule);
        await _context.SaveChangesAsync(cancellationToken);

        // Add to control points
        foreach (var controlPoint in controlPoints)
        {
            controlPoint.AccessRules.Add(accessRule);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Reload to get updated data
        var createdRule = await GetAccessRuleByIdAsync(accessRule.Id, cancellationToken);
        return createdRule!;
    }

    public async Task<AccessRuleResponse> UpdateAccessRuleAsync(int id, AccessRuleRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var accessRule = await _context.AccessRules
            .Include(ar => ar.Roles)
            .Where(ar => ar.Id == id && ar.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (accessRule == null)
        {
            throw new InvalidOperationException($"Access rule with ID {id} not found.");
        }

        // Validate roles exist and belong to tenant
        var roles = await _context.Roles
            .Where(r => request.RoleIds.Contains(r.Id) && r.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        if (roles.Count != request.RoleIds.Count)
        {
            throw new InvalidOperationException("One or more roles not found or do not belong to the current tenant.");
        }

        // Validate control points exist and belong to tenant
        var controlPoints = await _context.ControlPoints
            .Include(cp => cp.AccessRules)
            .Where(cp => request.ControlPointIds.Contains(cp.Id) && cp.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        if (controlPoints.Count != request.ControlPointIds.Count)
        {
            throw new InvalidOperationException("One or more control points not found or do not belong to the current tenant.");
        }

        // Update time range
        TimeRange? timeRange = null;
        if (!string.IsNullOrWhiteSpace(request.StartTime) && !string.IsNullOrWhiteSpace(request.EndTime))
        {
            timeRange = new TimeRange(request.StartTime, request.EndTime);
        }
        accessRule.UpdateTimeRange(timeRange);

        // Update date range
        DateRange? dateRange = null;
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            dateRange = new DateRange(request.StartDate.Value, request.EndDate.Value);
        }
        accessRule.UpdateValidityPeriod(dateRange);

        // Update roles - remove old ones and add new ones
        var currentRoles = accessRule.Roles.ToList();
        foreach (var role in currentRoles)
        {
            accessRule.RemoveRole(role);
        }
        foreach (var role in roles)
        {
            accessRule.AddRole(role);
        }

        // Update control points - remove from old ones and add to new ones
        var allControlPoints = await _context.ControlPoints
            .Include(cp => cp.AccessRules)
            .Where(cp => cp.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach (var cp in allControlPoints)
        {
            var hasRule = cp.AccessRules.Any(ar => ar.Id == id);
            var shouldHaveRule = request.ControlPointIds.Contains(cp.Id);

            if (hasRule && !shouldHaveRule)
            {
                cp.AccessRules.Remove(accessRule);
            }
            else if (!hasRule && shouldHaveRule)
            {
                cp.AccessRules.Add(accessRule);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Reload to get updated data
        var updatedRule = await GetAccessRuleByIdAsync(id, cancellationToken);
        return updatedRule!;
    }

    public async Task<bool> DeleteAccessRuleAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var accessRule = await _context.AccessRules
            .Where(ar => ar.Id == id && ar.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (accessRule == null)
        {
            return false;
        }

        // Remove from all control points
        var controlPoints = await _context.ControlPoints
            .Include(cp => cp.AccessRules)
            .Where(cp => cp.TenantId == tenantId && cp.AccessRules.Any(ar => ar.Id == id))
            .ToListAsync(cancellationToken);

        foreach (var cp in controlPoints)
        {
            cp.AccessRules.Remove(accessRule);
        }

        _context.AccessRules.Remove(accessRule);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private AccessRuleResponse MapToResponse(AccessRule accessRule, List<ControlPoint> controlPoints)
    {
        var now = DateTime.Now;
        
        return new AccessRuleResponse
        {
            Id = accessRule.Id,
            TenantId = accessRule.TenantId,
            StartTime = accessRule.TimeRange?.StartTime.ToString("HH:mm"),
            EndTime = accessRule.TimeRange?.EndTime.ToString("HH:mm"),
            StartDate = accessRule.ValidityPeriod?.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = accessRule.ValidityPeriod?.EndDate.ToDateTime(TimeOnly.MinValue),
            RoleIds = accessRule.Roles.Select(r => r.Id).ToList(),
            RoleNames = accessRule.Roles.Select(r => r.Name).ToList(),
            ControlPointIds = controlPoints.Select(cp => cp.Id).ToList(),
            ControlPointNames = controlPoints.Select(cp => cp.Name).ToList(),
            CreatedAt = accessRule.CreatedAt,
            UpdatedAt = accessRule.UpdatedAt,
            IsActive = accessRule.IsActiveAt(now),
            Is24x7 = !accessRule.TimeRange.HasValue,
            IsPermanent = !accessRule.ValidityPeriod.HasValue
        };
    }
}
