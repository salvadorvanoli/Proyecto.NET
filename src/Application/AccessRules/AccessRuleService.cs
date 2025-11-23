using Shared.DTOs.AccessRules;
using Shared.DTOs;
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
    private readonly DbContext _dbContext;

    public AccessRuleService(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _dbContext = (DbContext)context; // Cast to access Entry() for shadow properties
    }

    public async Task<AccessRuleResponse?> GetAccessRuleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var accessRule = await _context.AccessRules
            .Include(ar => ar.Roles)
            .Include(ar => ar.ControlPoint)
            .Where(ar => ar.Id == id && ar.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (accessRule == null)
            return null;

        return MapToResponse(accessRule, new List<ControlPoint> { accessRule.ControlPoint });
    }

    public async Task<IEnumerable<AccessRuleResponse>> GetAccessRulesByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var accessRules = await _context.AccessRules
            .Include(ar => ar.Roles)
            .Include(ar => ar.ControlPoint)
            .Where(ar => ar.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return accessRules.Select(ar => MapToResponse(ar, new List<ControlPoint> { ar.ControlPoint })).ToList();
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
            .Include(ar => ar.ControlPoint)
            .Where(ar => ar.TenantId == tenantId && ar.Roles.Any(r => r.Id == roleId))
            .ToListAsync(cancellationToken);

        return accessRules.Select(ar => MapToResponse(ar, new List<ControlPoint> { ar.ControlPoint })).ToList();
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

        // Create one access rule for the first control point (one-to-many relationship)
        var firstControlPoint = controlPoints.First();
        var accessRule = new AccessRule(tenantId, firstControlPoint.Id);

        // Add roles
        foreach (var role in roles)
        {
            accessRule.AddRole(role);
        }

        _context.AccessRules.Add(accessRule);
        await _context.SaveChangesAsync(cancellationToken);

        // For additional control points, create separate access rules
        if (controlPoints.Count > 1)
        {
            foreach (var cp in controlPoints.Skip(1))
            {
                var additionalRule = new AccessRule(tenantId, cp.Id, timeRange, dateRange);
                foreach (var role in roles)
                {
                    additionalRule.AddRole(role);
                }
                _context.AccessRules.Add(additionalRule);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Reload to get updated data with navigation properties
        var createdRule = await _context.AccessRules
            .Include(ar => ar.Roles)
            .Include(ar => ar.ControlPoint)
            .FirstOrDefaultAsync(ar => ar.Id == accessRule.Id, cancellationToken);

        return MapToResponse(createdRule!, new List<ControlPoint> { createdRule!.ControlPoint });
    }

    public async Task<AccessRuleResponse> UpdateAccessRuleAsync(int id, AccessRuleRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var accessRule = await _context.AccessRules
            .Include(ar => ar.Roles)
            .Include(ar => ar.ControlPoint)
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

        // Validate control points exist and belong to tenant (one-to-many: only first CP is used)
        var controlPoints = await _context.ControlPoints
            .Where(cp => request.ControlPointIds.Contains(cp.Id) && cp.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        if (controlPoints.Count != request.ControlPointIds.Count)
        {
            throw new InvalidOperationException("One or more control points not found or do not belong to the current tenant.");
        }

        // Parse and set TimeRange
        TimeRange? timeRange = null;
        if (!string.IsNullOrWhiteSpace(request.StartTime) && !string.IsNullOrWhiteSpace(request.EndTime))
        {
            if (TimeOnly.TryParse(request.StartTime, out var startTime) && 
                TimeOnly.TryParse(request.EndTime, out var endTime))
            {
                timeRange = new TimeRange(startTime, endTime);
            }
        }
        
        // Parse and set DateRange
        DateRange? dateRange = null;
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            dateRange = new DateRange(DateOnly.FromDateTime(request.StartDate.Value), 
                                     DateOnly.FromDateTime(request.EndDate.Value));
        }
        
        // Update domain properties (ApplicationDbContext will sync shadow properties automatically)
        accessRule.UpdateTimeAndDateRanges(timeRange, dateRange);

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

        // Note: With one-to-many, we can't reassign ControlPointId after creation
        // The AccessRule is bound to its ControlPoint. To change it, delete and recreate.
        
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
        
        // Read shadow properties for TimeRange
        var timeRangeStartTime = _dbContext.Entry(accessRule).Property<TimeOnly?>("TimeRangeStartTime").CurrentValue;
        var timeRangeEndTime = _dbContext.Entry(accessRule).Property<TimeOnly?>("TimeRangeEndTime").CurrentValue;
        
        // Read shadow properties for ValidityPeriod
        var validityStartDate = _dbContext.Entry(accessRule).Property<DateOnly?>("ValidityStartDate").CurrentValue;
        var validityEndDate = _dbContext.Entry(accessRule).Property<DateOnly?>("ValidityEndDate").CurrentValue;
        
        return new AccessRuleResponse
        {
            Id = accessRule.Id,
            TenantId = accessRule.TenantId,
            StartTime = timeRangeStartTime?.ToString("HH:mm"),
            EndTime = timeRangeEndTime?.ToString("HH:mm"),
            StartDate = validityStartDate?.ToDateTime(TimeOnly.MinValue),
            EndDate = validityEndDate?.ToDateTime(TimeOnly.MinValue),
            RoleIds = accessRule.Roles.Select(r => r.Id).ToList(),
            RoleNames = accessRule.Roles.Select(r => r.Name).ToList(),
            ControlPointIds = controlPoints.Select(cp => cp.Id).ToList(),
            ControlPointNames = controlPoints.Select(cp => cp.Name).ToList(),
            CreatedAt = accessRule.CreatedAt,
            UpdatedAt = accessRule.UpdatedAt,
            IsActive = IsRuleActiveAt(accessRule, now),
            Is24x7 = !timeRangeStartTime.HasValue && !timeRangeEndTime.HasValue,
            IsPermanent = !validityStartDate.HasValue && !validityEndDate.HasValue
        };
    }

    private bool IsRuleActiveAt(AccessRule accessRule, DateTime dateTime)
    {
        // Read shadow properties
        var timeRangeStartTime = _dbContext.Entry(accessRule).Property<TimeOnly?>("TimeRangeStartTime").CurrentValue;
        var timeRangeEndTime = _dbContext.Entry(accessRule).Property<TimeOnly?>("TimeRangeEndTime").CurrentValue;
        var validityStartDate = _dbContext.Entry(accessRule).Property<DateOnly?>("ValidityStartDate").CurrentValue;
        var validityEndDate = _dbContext.Entry(accessRule).Property<DateOnly?>("ValidityEndDate").CurrentValue;

        // Check date validity
        if (validityStartDate.HasValue && validityEndDate.HasValue)
        {
            var dateOnly = DateOnly.FromDateTime(dateTime);
            if (dateOnly < validityStartDate.Value || dateOnly > validityEndDate.Value)
                return false;
        }

        // Check time range
        if (timeRangeStartTime.HasValue && timeRangeEndTime.HasValue)
        {
            var timeOnly = TimeOnly.FromDateTime(dateTime);
            var startTime = timeRangeStartTime.Value;
            var endTime = timeRangeEndTime.Value;
            
            // Handle ranges that cross midnight
            if (endTime < startTime)
            {
                return timeOnly >= startTime || timeOnly <= endTime;
            }
            else
            {
                return timeOnly >= startTime && timeOnly <= endTime;
            }
        }

        return true;
    }

    public async Task<List<AccessRuleDto>> GetAllActiveRulesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        
        // Get all access rules (simplified - no date filtering for now)
        var accessRules = await _context.AccessRules
            .Include(ar => ar.Roles)
            .Include(ar => ar.ControlPoint)
            .Where(ar => ar.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        // Get all users with their roles
        var users = await _context.Users
            .Include(u => u.Roles)
            .Where(u => u.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var rules = new List<AccessRuleDto>();

        foreach (var accessRule in accessRules)
        {
            if (accessRule.ControlPoint == null) continue; // Skip if no control point
            
            // Get role IDs from this access rule
            var ruleRoleIds = accessRule.Roles.Select(r => r.Id).ToHashSet();
            
            // Find users that have any of these roles
            var usersWithAccess = users.Where(u => u.Roles.Any(r => ruleRoleIds.Contains(r.Id)));

            foreach (var user in usersWithAccess)
            {
                // For now, allow all days (future: implement DaysOfWeek logic)
                string allowedDays = "0,1,2,3,4,5,6"; // All days

                // Determine time range
                string startTime = "00:00";
                string endTime = "23:59";
                
                // Read shadow properties for TimeRange
                var timeRangeStartTime = _dbContext.Entry(accessRule).Property<TimeOnly?>("TimeRangeStartTime").CurrentValue;
                var timeRangeEndTime = _dbContext.Entry(accessRule).Property<TimeOnly?>("TimeRangeEndTime").CurrentValue;
                
                if (timeRangeStartTime.HasValue && timeRangeEndTime.HasValue)
                {
                    var start = timeRangeStartTime.Value;
                    var end = timeRangeEndTime.Value;
                    startTime = $"{start.Hour:D2}:{start.Minute:D2}";
                    endTime = $"{end.Hour:D2}:{end.Minute:D2}";
                }

                rules.Add(new AccessRuleDto
                {
                    UserId = user.Id,
                    ControlPointId = accessRule.ControlPointId,
                    SpaceId = accessRule.ControlPoint.SpaceId,
                    AllowedDays = allowedDays,
                    StartTime = startTime,
                    EndTime = endTime,
                    IsActive = true
                });
            }
        }

        return rules;
    }
}
