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
        // Get all benefits for the tenant
        // In a more complex scenario, you might filter benefits based on user roles or assignments
        var benefits = await _context.Benefits
            .Include(b => b.BenefitType)
            .Include(b => b.Consumptions.Where(c => c.UserId == userId))
                .ThenInclude(c => c.Usages)
            .OrderBy(b => b.BenefitType.Name)
            .ToListAsync();

        return benefits.Select(b => new BenefitResponse
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
            TotalConsumed = b.TotalConsumed,
            Consumptions = b.Consumptions
                .Where(c => c.UserId == userId)
                .Select(c => new ConsumptionResponse
                {
                    Id = c.Id,
                    Amount = c.Amount,
                    CreatedAt = c.CreatedAt,
                    Usages = c.Usages.Select(u => new UsageResponse
                    {
                        Id = u.Id,
                        UsageDateTime = u.UsageDateTime
                    }).ToList()
                }).ToList()
        }).ToList();
    }

    public async Task<List<BenefitResponse>> GetAllBenefitsAsync()
    {
        var benefits = await _context.Benefits
            .Include(b => b.BenefitType)
            .Include(b => b.Consumptions)
                .ThenInclude(c => c.Usages)
            .OrderBy(b => b.BenefitType.Name)
            .ToListAsync();

        return benefits.Select(b => new BenefitResponse
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
            TotalConsumed = b.TotalConsumed,
            Consumptions = b.Consumptions.Select(c => new ConsumptionResponse
            {
                Id = c.Id,
                Amount = c.Amount,
                CreatedAt = c.CreatedAt,
                Usages = c.Usages.Select(u => new UsageResponse
                {
                    Id = u.Id,
                    UsageDateTime = u.UsageDateTime
                }).ToList()
            }).ToList()
        }).ToList();
    }

    public async Task<BenefitResponse?> GetBenefitByIdAsync(int benefitId)
    {
        var benefit = await _context.Benefits
            .Include(b => b.BenefitType)
            .Include(b => b.Consumptions)
                .ThenInclude(c => c.Usages)
            .Where(b => b.Id == benefitId)
            .FirstOrDefaultAsync();

        if (benefit == null)
            return null;

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
            TotalConsumed = benefit.TotalConsumed,
            Consumptions = benefit.Consumptions.Select(c => new ConsumptionResponse
            {
                Id = c.Id,
                Amount = c.Amount,
                CreatedAt = c.CreatedAt,
                Usages = c.Usages.Select(u => new UsageResponse
                {
                    Id = u.Id,
                    UsageDateTime = u.UsageDateTime
                }).ToList()
            }).ToList()
        };
    }
}

