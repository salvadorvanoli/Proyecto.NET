using Application.AccessEvents.DTOs;
using Application.AccessEvents.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.AccessEvents;

/// <summary>
/// Implementation of the access event service.
/// </summary>
public class AccessEventService : IAccessEventService
{
    private readonly ApplicationDbContext _context;

    public AccessEventService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccessEventResponse>> GetUserAccessEventsAsync(int userId)
    {
        var events = await _context.AccessEvents
            .Include(e => e.ControlPoint)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EventDateTime)
            .ToListAsync();

        return events.Select(e => new AccessEventResponse
        {
            Id = e.Id,
            EventDateTime = e.EventDateTime,
            Result = e.Result.ToString(),
            ControlPoint = new ControlPointResponse
            {
                Id = e.ControlPoint.Id,
                Name = e.ControlPoint.Name
            },
            UserId = e.UserId
        }).ToList();
    }

    public async Task<List<AccessEventResponse>> GetAllAccessEventsAsync()
    {
        var events = await _context.AccessEvents
            .Include(e => e.ControlPoint)
            .OrderByDescending(e => e.EventDateTime)
            .ToListAsync();

        return events.Select(e => new AccessEventResponse
        {
            Id = e.Id,
            EventDateTime = e.EventDateTime,
            Result = e.Result.ToString(),
            ControlPoint = new ControlPointResponse
            {
                Id = e.ControlPoint.Id,
                Name = e.ControlPoint.Name
            },
            UserId = e.UserId
        }).ToList();
    }

    public async Task<AccessEventResponse?> GetAccessEventByIdAsync(int eventId)
    {
        var accessEvent = await _context.AccessEvents
            .Include(e => e.ControlPoint)
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync();

        if (accessEvent == null)
            return null;

        return new AccessEventResponse
        {
            Id = accessEvent.Id,
            EventDateTime = accessEvent.EventDateTime,
            Result = accessEvent.Result.ToString(),
            ControlPoint = new ControlPointResponse
            {
                Id = accessEvent.ControlPoint.Id,
                Name = accessEvent.ControlPoint.Name
            },
            UserId = accessEvent.UserId
        };
    }
}
