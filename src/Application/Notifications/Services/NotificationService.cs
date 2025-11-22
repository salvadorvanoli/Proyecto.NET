using Application.Common.Interfaces;
using Application.Notifications.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Services;

/// <summary>
/// Implementation of notification service.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public NotificationService(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var notifications = await _context.Notifications
            .Include(n => n.User)
            .Where(n => n.UserId == userId && n.TenantId == tenantId)
            .OrderByDescending(n => n.SentDateTime)
            .ToListAsync(cancellationToken);

        return notifications.Select(MapToResponse);
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var notifications = await _context.Notifications
            .Include(n => n.User)
            .Where(n => n.TenantId == tenantId)
            .OrderByDescending(n => n.SentDateTime)
            .ToListAsync(cancellationToken);

        return notifications.Select(MapToResponse);
    }

    public async Task<NotificationResponseDto?> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var notification = await _context.Notifications
            .Include(n => n.User)
            .Where(n => n.Id == id && n.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        return notification == null ? null : MapToResponse(notification);
    }

    public async Task<NotificationResponseDto> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var notification = await _context.Notifications
            .Include(n => n.User)
            .Where(n => n.Id == notificationId && n.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (notification == null)
        {
            throw new InvalidOperationException($"Notification with ID {notificationId} not found.");
        }

        notification.MarkAsRead();
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(notification);
    }

    public async Task<int> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && n.TenantId == tenantId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return notifications.Count;
    }

    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        return await _context.Notifications
            .Where(n => n.UserId == userId && n.TenantId == tenantId && !n.IsRead)
            .CountAsync(cancellationToken);
    }

    private static NotificationResponseDto MapToResponse(Domain.Entities.Notification notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            SentDateTime = notification.SentDateTime,
            IsRead = notification.IsRead,
            UserId = notification.UserId,
            UserEmail = notification.User?.Email ?? string.Empty,
            UserFullName = notification.User?.FullName ?? string.Empty,
            TenantId = notification.TenantId,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt
        };
    }
}
