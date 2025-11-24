using Application.Benefits.DTOs;
using Application.Benefits.Services;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.Benefits;

namespace Infrastructure.Services.Benefits;

/// <summary>
/// Implementation of the benefit service.
/// </summary>
public class BenefitService : IBenefitService
{
    private readonly IApplicationDbContext _context;

    public BenefitService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Application.Benefits.DTOs.BenefitResponse>> GetUserBenefitsAsync(int userId)
    {
        // Get the user's tenant ID
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return new List<Application.Benefits.DTOs.BenefitResponse>();

        var tenantId = user.TenantId;

        // Get all benefits for the user's tenant
        var benefits = await _context.Benefits
            .Include(b => b.BenefitType)
            .Where(b => b.TenantId == tenantId)
            .OrderBy(b => b.BenefitType.Name)
            .ToListAsync();

        var benefitIds = benefits.Select(b => b.Id).ToList();
        
        // Get usages for this user and these benefits
        var usages = await _context.Usages
            .Include(u => u.Consumptions)
            .Where(u => u.UserId == userId && benefitIds.Contains(u.BenefitId))
            .ToListAsync();

        return benefits.Select(b =>
        {
            var benefitUsages = usages.Where(u => u.BenefitId == b.Id).ToList();
            var consumptions = benefitUsages.SelectMany(u => u.Consumptions).ToList();
            
            return new Application.Benefits.DTOs.BenefitResponse
            {
                Id = b.Id,
                BenefitType = new BenefitTypeResponse
                {
                    Id = b.BenefitType.Id,
                    Name = b.BenefitType.Name,
                    Description = b.BenefitType.Description
                },
                ValidityPeriod = b.ValidityPeriod.HasValue ? new DateRangeResponse
                {
                    StartDate = b.ValidityPeriod.Value.StartDate,
                    EndDate = b.ValidityPeriod.Value.EndDate
                } : null,
                Quotas = b.Quotas,
                IsValid = b.IsValid,
                HasAvailableQuotas = b.HasAvailableQuotas,
                CanBeConsumed = b.CanBeConsumed,
                TotalConsumed = consumptions.Sum(c => c.Amount),
                Consumptions = consumptions.Select(c => new ConsumptionResponse
                {
                    Id = c.Id,
                    Amount = c.Amount,
                    CreatedAt = c.CreatedAt,
                    Usages = new List<UsageResponse>()
                }).ToList()
            };
        }).ToList();
    }

    public async Task<List<Application.Benefits.DTOs.BenefitResponse>> GetAllBenefitsAsync()
    {
        var benefits = await _context.Benefits
            .Include(b => b.BenefitType)
            .OrderBy(b => b.BenefitType.Name)
            .ToListAsync();

        var benefitIds = benefits.Select(b => b.Id).ToList();
        
        // Get all usages for these benefits
        var usages = await _context.Usages
            .Include(u => u.Consumptions)
            .Where(u => benefitIds.Contains(u.BenefitId))
            .ToListAsync();

        return benefits.Select(b =>
        {
            var benefitUsages = usages.Where(u => u.BenefitId == b.Id).ToList();
            var consumptions = benefitUsages.SelectMany(u => u.Consumptions).ToList();
            
            return new Application.Benefits.DTOs.BenefitResponse
            {
                Id = b.Id,
                BenefitType = new BenefitTypeResponse
                {
                    Id = b.BenefitType.Id,
                    Name = b.BenefitType.Name,
                    Description = b.BenefitType.Description
                },
                ValidityPeriod = b.ValidityPeriod.HasValue ? new DateRangeResponse
                {
                    StartDate = b.ValidityPeriod.Value.StartDate,
                    EndDate = b.ValidityPeriod.Value.EndDate
                } : null,
                Quotas = b.Quotas,
                IsValid = b.IsValid,
                HasAvailableQuotas = b.HasAvailableQuotas,
                CanBeConsumed = b.CanBeConsumed,
                TotalConsumed = consumptions.Sum(c => c.Amount),
                Consumptions = consumptions.Select(c => new ConsumptionResponse
                {
                    Id = c.Id,
                    Amount = c.Amount,
                    CreatedAt = c.CreatedAt,
                    Usages = new List<UsageResponse>()
                }).ToList()
            };
        }).ToList();
    }

    public async Task<Application.Benefits.DTOs.BenefitResponse?> GetBenefitByIdAsync(int benefitId)
    {
        var benefit = await _context.Benefits
            .Include(b => b.BenefitType)
            .Where(b => b.Id == benefitId)
            .FirstOrDefaultAsync();

        if (benefit == null)
            return null;

        // Get usages and consumptions for this benefit
        var usages = await _context.Usages
            .Include(u => u.Consumptions)
            .Where(u => u.BenefitId == benefitId)
            .ToListAsync();

        var consumptions = usages.SelectMany(u => u.Consumptions).ToList();

        return new Application.Benefits.DTOs.BenefitResponse
        {
            Id = benefit.Id,
            BenefitType = new BenefitTypeResponse
            {
                Id = benefit.BenefitType.Id,
                Name = benefit.BenefitType.Name,
                Description = benefit.BenefitType.Description
            },
            ValidityPeriod = benefit.ValidityPeriod.HasValue ? new DateRangeResponse
            {
                StartDate = benefit.ValidityPeriod.Value.StartDate,
                EndDate = benefit.ValidityPeriod.Value.EndDate
            } : null,
            Quotas = benefit.Quotas,
            IsValid = benefit.IsValid,
            HasAvailableQuotas = benefit.HasAvailableQuotas,
            CanBeConsumed = benefit.CanBeConsumed,
            TotalConsumed = consumptions.Sum(c => c.Amount),
            Consumptions = consumptions.Select(c => new ConsumptionResponse
            {
                Id = c.Id,
                Amount = c.Amount,
                CreatedAt = c.CreatedAt,
                Usages = new List<UsageResponse>()
            }).ToList()
        };
    }

    public async Task<List<BenefitWithHistoryResponse>> GetBenefitsWithHistoryAsync(int userId)
    {
        // Get the user's tenant ID
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return new List<BenefitWithHistoryResponse>();

        var tenantId = user.TenantId;

        // Get usages for this user with their benefits and consumptions
        var usages = await _context.Usages
            .Include(u => u.Benefit)
                .ThenInclude(b => b.BenefitType)
            .Include(u => u.Consumptions)
            .Where(u => u.UserId == userId && u.Benefit.TenantId == tenantId && u.Benefit.Active)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return usages.Select(u => new BenefitWithHistoryResponse
        {
            BenefitId = u.BenefitId,
            UsageId = u.Id,
            TenantId = u.TenantId,
            BenefitTypeId = u.Benefit.BenefitTypeId,
            BenefitTypeName = u.Benefit.BenefitType.Name,
            Quantity = u.Quantity,
            StartDate = u.Benefit.ValidityPeriod?.StartDate.ToString("yyyy-MM-dd"),
            EndDate = u.Benefit.ValidityPeriod?.EndDate.ToString("yyyy-MM-dd"),
            IsValid = u.Benefit.IsValid,
            CanBeConsumed = u.HasAvailableQuantity && u.Benefit.IsValid,
            IsPermanent = u.Benefit.ValidityPeriod == null,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt,
            Consumptions = u.Consumptions
                .OrderByDescending(c => c.ConsumptionDateTime)
                .Select(c => new ConsumptionHistoryResponse
                {
                    Id = c.Id,
                    Amount = c.Amount,
                    ConsumptionDateTime = c.ConsumptionDateTime,
                    CreatedAt = c.CreatedAt
                })
                .ToList()
        }).ToList();
    }
}

