using System.Net.Http.Json;
using Application.Notifications.DTOs;
using Web.FrontOffice.Services.Interfaces;

namespace Web.FrontOffice.Services.Api;

/// <summary>
/// Implementation of notification API service.
/// </summary>
public class NotificationApiService : INotificationApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationApiService> _logger;

    public NotificationApiService(HttpClient httpClient, ILogger<NotificationApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(int userId)
    {
        try
        {
            // Add X-Tenant-Id header (default tenant 1)
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var response = await _httpClient.GetAsync($"api/notifications/user/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch notifications with status code: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<NotificationResponseDto>();
            }

            var notifications = await response.Content.ReadFromJsonAsync<IEnumerable<NotificationResponseDto>>();
            return notifications ?? Enumerable.Empty<NotificationResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching notifications from API");
            return Enumerable.Empty<NotificationResponseDto>();
        }
    }

    public async Task<NotificationResponseDto?> MarkAsReadAsync(int notificationId)
    {
        try
        {
            // Add X-Tenant-Id header (default tenant 1)
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var response = await _httpClient.PutAsync($"api/notifications/{notificationId}/mark-as-read", null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to mark notification as read with status code: {StatusCode}", response.StatusCode);
                return null;
            }

            var notification = await response.Content.ReadFromJsonAsync<NotificationResponseDto>();
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return null;
        }
    }

    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        try
        {
            // Add X-Tenant-Id header (default tenant 1)
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var response = await _httpClient.PutAsync($"api/notifications/user/{userId}/mark-all-as-read", null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to mark all notifications as read with status code: {StatusCode}", response.StatusCode);
                return 0;
            }

            var result = await response.Content.ReadFromJsonAsync<MarkAllResult>();
            return result?.count ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return 0;
        }
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        try
        {
            // Add X-Tenant-Id header (default tenant 1)
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var response = await _httpClient.GetAsync($"api/notifications/user/{userId}/unread-count");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get unread count with status code: {StatusCode}", response.StatusCode);
                return 0;
            }

            var result = await response.Content.ReadFromJsonAsync<UnreadCountResult>();
            return result?.count ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return 0;
        }
    }

    private class MarkAllResult
    {
        public int count { get; set; }
        public string message { get; set; } = string.Empty;
    }

    private class UnreadCountResult
    {
        public int count { get; set; }
    }
}
