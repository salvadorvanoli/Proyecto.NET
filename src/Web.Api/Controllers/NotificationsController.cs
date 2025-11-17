using Application.Notifications.DTOs;
using Application.Notifications.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing notifications.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all notifications for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of notifications.</returns>
    /// <response code="200">Notifications retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetAllNotifications(
        CancellationToken cancellationToken)
    {
        var notifications = await _notificationService.GetAllNotificationsAsync(cancellationToken);
        return Ok(notifications);
    }

    /// <summary>
    /// Gets all notifications for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of user notifications.</returns>
    /// <response code="200">Notifications retrieved successfully.</response>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetUserNotifications(
        int userId,
        CancellationToken cancellationToken)
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
        return Ok(notifications);
    }

    /// <summary>
    /// Gets a notification by ID.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The notification information.</returns>
    /// <response code="200">Notification found.</response>
    /// <response code="404">Notification not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponseDto>> GetNotificationById(
        int id,
        CancellationToken cancellationToken)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id, cancellationToken);

        if (notification == null)
        {
            return NotFound(new { error = $"Notification with ID {id} not found." });
        }

        return Ok(notification);
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated notification.</returns>
    /// <response code="200">Notification marked as read.</response>
    /// <response code="404">Notification not found.</response>
    [HttpPut("{id}/mark-as-read")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponseDto>> MarkAsRead(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await _notificationService.MarkAsReadAsync(id, cancellationToken);
            _logger.LogInformation("Notification {NotificationId} marked as read", id);
            return Ok(notification);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to mark notification as read: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return StatusCode(500, new { error = "An error occurred while marking the notification as read." });
        }
    }

    /// <summary>
    /// Marks all notifications for a user as read.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of notifications marked as read.</returns>
    /// <response code="200">Notifications marked as read.</response>
    [HttpPut("user/{userId}/mark-all-as-read")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> MarkAllAsRead(
        int userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var count = await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", count, userId);
            return Ok(new { count, message = $"{count} notifications marked as read." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while marking notifications as read." });
        }
    }

    /// <summary>
    /// Gets the count of unread notifications for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of unread notifications.</returns>
    /// <response code="200">Count retrieved successfully.</response>
    [HttpGet("user/{userId}/unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetUnreadCount(
        int userId,
        CancellationToken cancellationToken)
    {
        var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(new { count });
    }
}
