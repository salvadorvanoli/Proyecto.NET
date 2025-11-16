using Application.Common.Interfaces;
using Shared.DTOs.News;
using Microsoft.EntityFrameworkCore;
using DomainNews = Domain.Entities.News;
using Domain.Entities;

namespace Application.News;

/// <summary>
/// Implementation of news service for managing news operations.
/// </summary>
public class NewsService : INewsService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public NewsService(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<NewsResponse> CreateNewsAsync(NewsRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Verify tenant exists
        var tenantExists = await _context.Tenants
            .AnyAsync(t => t.Id == tenantId, cancellationToken);

        if (!tenantExists)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} does not exist.");
        }

        // Create the news entity
        var news = new DomainNews(
            tenantId,
            request.Title,
            request.Content,
            request.PublishDate,
            request.ImageUrl
        );

        _context.News.Add(news);
        await _context.SaveChangesAsync(cancellationToken);

        // Create notifications for all users in the tenant
        var users = await _context.Users
            .Where(u => u.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            var notification = Notification.CreateNow(
                tenantId,
                $"Nueva noticia: {news.Title}",
                $"Se ha publicado una nueva noticia. {news.Content.Substring(0, Math.Min(100, news.Content.Length))}...",
                user.Id
            );

            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(news);
    }

    public async Task<NewsResponse?> GetNewsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var news = await _context.News
            .Where(n => n.Id == id && n.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        return news == null ? null : MapToResponse(news);
    }

    public async Task<IEnumerable<NewsResponse>> GetNewsByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var newsList = await _context.News
            .Where(n => n.TenantId == tenantId)
            .OrderByDescending(n => n.PublishDate)
            .ToListAsync(cancellationToken);

        return newsList.Select(MapToResponse);
    }

    public async Task<NewsResponse> UpdateNewsAsync(int id, NewsRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var news = await _context.News
            .Where(n => n.Id == id && n.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (news == null)
        {
            throw new InvalidOperationException($"News with ID {id} not found.");
        }

        news.UpdateContent(request.Title, request.Content, request.ImageUrl);
        news.UpdatePublishDate(request.PublishDate);

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(news);
    }

    public async Task<bool> DeleteNewsAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var news = await _context.News
            .Where(n => n.Id == id && n.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (news == null)
        {
            return false;
        }

        _context.News.Remove(news);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static NewsResponse MapToResponse(DomainNews news)
    {
        return new NewsResponse
        {
            Id = news.Id,
            Title = news.Title,
            Content = news.Content,
            PublishDate = news.PublishDate,
            ImageUrl = news.ImageUrl,
            TenantId = news.TenantId,
            CreatedAt = news.CreatedAt,
            UpdatedAt = news.UpdatedAt
        };
    }
}
