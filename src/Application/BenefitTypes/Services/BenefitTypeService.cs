using Application.BenefitTypes.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.BenefitTypes.Services;

/// <summary>
/// Service for benefit type management.
/// </summary>
public class BenefitTypeService : IBenefitTypeService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public BenefitTypeService(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<BenefitTypeResponse?> GetBenefitTypeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefitType = await _context.BenefitTypes
            .Where(bt => bt.Id == id && bt.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (benefitType == null)
            return null;

        var benefitCount = await _context.Benefits
            .Where(b => b.BenefitTypeId == id && b.TenantId == tenantId)
            .CountAsync(cancellationToken);

        return new BenefitTypeResponse
        {
            Id = benefitType.Id,
            Name = benefitType.Name,
            Description = benefitType.Description,
            TenantId = benefitType.TenantId,
            BenefitCount = benefitCount,
            CreatedAt = benefitType.CreatedAt,
            UpdatedAt = benefitType.UpdatedAt
        };
    }

    public async Task<IEnumerable<BenefitTypeResponse>> GetBenefitTypesByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefitTypes = await _context.BenefitTypes
            .Where(bt => bt.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var benefitTypesWithCounts = new List<BenefitTypeResponse>();

        foreach (var benefitType in benefitTypes)
        {
            var benefitCount = await _context.Benefits
                .Where(b => b.BenefitTypeId == benefitType.Id && b.TenantId == tenantId)
                .CountAsync(cancellationToken);

            benefitTypesWithCounts.Add(new BenefitTypeResponse
            {
                Id = benefitType.Id,
                Name = benefitType.Name,
                Description = benefitType.Description,
                TenantId = benefitType.TenantId,
                BenefitCount = benefitCount,
                CreatedAt = benefitType.CreatedAt,
                UpdatedAt = benefitType.UpdatedAt
            });
        }

        return benefitTypesWithCounts;
    }

    public async Task<BenefitTypeResponse> CreateBenefitTypeAsync(CreateBenefitTypeRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Check if benefit type with same name already exists
        var existingBenefitType = await _context.BenefitTypes
            .Where(bt => bt.Name.ToLower() == request.Name.ToLower() && bt.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingBenefitType != null)
        {
            throw new InvalidOperationException($"A benefit type with name '{request.Name}' already exists.");
        }

        var benefitType = new BenefitType(tenantId, request.Name, request.Description);

        _context.BenefitTypes.Add(benefitType);
        await _context.SaveChangesAsync(cancellationToken);

        return new BenefitTypeResponse
        {
            Id = benefitType.Id,
            Name = benefitType.Name,
            Description = benefitType.Description,
            TenantId = benefitType.TenantId,
            BenefitCount = 0,
            CreatedAt = benefitType.CreatedAt,
            UpdatedAt = benefitType.UpdatedAt
        };
    }

    public async Task<BenefitTypeResponse> UpdateBenefitTypeAsync(int id, UpdateBenefitTypeRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefitType = await _context.BenefitTypes
            .Where(bt => bt.Id == id && bt.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (benefitType == null)
        {
            throw new InvalidOperationException($"Benefit type with ID {id} not found.");
        }

        // Check if another benefit type with same name already exists
        var existingBenefitType = await _context.BenefitTypes
            .Where(bt => bt.Name.ToLower() == request.Name.ToLower() && bt.TenantId == tenantId && bt.Id != id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingBenefitType != null)
        {
            throw new InvalidOperationException($"A benefit type with name '{request.Name}' already exists.");
        }

        benefitType.UpdateInformation(request.Name, request.Description);
        await _context.SaveChangesAsync(cancellationToken);

        var benefitCount = await _context.Benefits
            .Where(b => b.BenefitTypeId == id && b.TenantId == tenantId)
            .CountAsync(cancellationToken);

        return new BenefitTypeResponse
        {
            Id = benefitType.Id,
            Name = benefitType.Name,
            Description = benefitType.Description,
            TenantId = benefitType.TenantId,
            BenefitCount = benefitCount,
            CreatedAt = benefitType.CreatedAt,
            UpdatedAt = benefitType.UpdatedAt
        };
    }

    public async Task<bool> DeleteBenefitTypeAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefitType = await _context.BenefitTypes
            .Where(bt => bt.Id == id && bt.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (benefitType == null)
        {
            return false;
        }

        // Check if benefit type has associated benefits
        var benefitCount = await _context.Benefits
            .Where(b => b.BenefitTypeId == id && b.TenantId == tenantId)
            .CountAsync(cancellationToken);

        if (benefitCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete benefit type '{benefitType.Name}' because it has {benefitCount} benefit(s) assigned. Please remove all benefits from this benefit type first.");
        }

        _context.BenefitTypes.Remove(benefitType);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
