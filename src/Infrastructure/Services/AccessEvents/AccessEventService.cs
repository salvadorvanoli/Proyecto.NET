using Application.AccessEvents.DTOs;
using Application.AccessEvents.Services;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.AccessEvents;

/// <summary>
/// Implementation of the access event service.
/// </summary>
public class AccessEventService : IAccessEventService
{
    private readonly IApplicationDbContext _context;

    public AccessEventService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccessEventResponse>> GetUserAccessEventsAsync(int userId)
    {
        var events = await _context.AccessEvents
            .Include(e => e.ControlPoint)
                .ThenInclude(cp => cp.Space)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EventDateTime)
            .ToListAsync();

        return events.Select(MapToResponse).ToList();
    }

    public async Task<List<AccessEventResponse>> GetAllAccessEventsAsync()
    {
        var events = await _context.AccessEvents
            .Include(e => e.ControlPoint)
                .ThenInclude(cp => cp.Space)
            .OrderByDescending(e => e.EventDateTime)
            .ToListAsync();

        return events.Select(MapToResponse).ToList();
    }

    public async Task<AccessEventResponse?> GetAccessEventByIdAsync(int eventId)
    {
        var accessEvent = await _context.AccessEvents
            .Include(e => e.ControlPoint)
                .ThenInclude(cp => cp.Space)
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync();

        return accessEvent == null ? null : MapToResponse(accessEvent);
    }

    public async Task<AccessEventResponse> CreateAccessEventAsync(CreateAccessEventRequest request)
    {
        if (!Enum.TryParse<AccessResult>(request.Result, true, out var accessResult))
        {
            throw new ArgumentException($"Invalid access result: {request.Result}. Must be 'Granted' or 'Denied'.");
        }

        var controlPoint = await _context.ControlPoints.FindAsync(request.ControlPointId);
        if (controlPoint == null)
        {
            throw new KeyNotFoundException($"Control point with ID {request.ControlPointId} not found.");
        }

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
        }

        var eventDateTime = request.EventDateTime ?? DateTime.UtcNow;
        
        var accessEvent = new AccessEvent(
            tenantId: user.TenantId,
            eventDateTime: eventDateTime,
            result: accessResult,
            controlPointId: request.ControlPointId,
            userId: request.UserId
        );

        _context.AccessEvents.Add(accessEvent);
        await _context.SaveChangesAsync();

        var createdEvent = await _context.AccessEvents
            .Include(e => e.ControlPoint)
                .ThenInclude(cp => cp.Space)
            .FirstOrDefaultAsync(e => e.Id == accessEvent.Id);

        return MapToResponse(createdEvent!);
    }

    private static AccessEventResponse MapToResponse(AccessEvent accessEvent)
    {
        // Asegurar que EventDateTime se especifique como UTC
        var eventDateTimeUtc = accessEvent.EventDateTime.Kind == DateTimeKind.Utc
            ? accessEvent.EventDateTime
            : DateTime.SpecifyKind(accessEvent.EventDateTime, DateTimeKind.Utc);
        
        return new AccessEventResponse
        {
            Id = accessEvent.Id,
            EventDateTime = eventDateTimeUtc,
            Result = accessEvent.Result.ToString(),
            ControlPoint = new ControlPointResponse
            {
                Id = accessEvent.ControlPoint.Id,
                Name = accessEvent.ControlPoint.Name,
                Space = new SpaceResponse
                {
                    Id = accessEvent.ControlPoint.Space.Id,
                    Name = accessEvent.ControlPoint.Space.Name
                }
            },
            UserId = accessEvent.UserId
        };
    }
}
