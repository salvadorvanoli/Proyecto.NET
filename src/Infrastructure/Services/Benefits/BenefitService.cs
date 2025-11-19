using Application.Benefits.DTOs;
using Application.Benefits.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Benefits;

/// <summary>
/// Implementation of the benefit service.
/// </summary>
public class BenefitService : IBenefitService
{
    private readonly ApplicationDbContext _context;

    public BenefitService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BenefitResponse>> GetUserBenefitsAsync(int userId)
    {
        // Get the user's tenant ID
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return new List<BenefitResponse>();

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
            
            return new BenefitResponse
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

    public async Task<List<BenefitResponse>> GetAllBenefitsAsync()
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
            
            return new BenefitResponse
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

    public async Task<BenefitResponse?> GetBenefitByIdAsync(int benefitId)
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

        return new BenefitResponse
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
}

