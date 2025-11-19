using Shared.DTOs.Benefits;
using Application.Common.Interfaces;
using Domain.DataTypes;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Benefits;

/// <summary>
/// Service for managing benefits.
/// </summary>
public class BenefitService : IBenefitService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public BenefitService(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Gets a benefit by ID.
    /// </summary>
    public async Task<BenefitResponse?> GetBenefitByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefit = await _context.Benefits
            .Include(b => b.BenefitType)
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId, cancellationToken);

        if (benefit == null)
            return null;

        return MapToResponse(benefit);
    }

    /// <summary>
    /// Gets all benefits for the current tenant.
    /// </summary>
    public async Task<IEnumerable<BenefitResponse>> GetBenefitsByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefits = await _context.Benefits
            .Include(b => b.BenefitType)
            .Where(b => b.TenantId == tenantId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return benefits.Select(MapToResponse);
    }

    /// <summary>
    /// Gets benefits filtered by benefit type.
    /// </summary>
    public async Task<IEnumerable<BenefitResponse>> GetBenefitsByTypeAsync(int benefitTypeId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefits = await _context.Benefits
            .Include(b => b.BenefitType)
            .Where(b => b.TenantId == tenantId && b.BenefitTypeId == benefitTypeId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return benefits.Select(MapToResponse);
    }

    /// <summary>
    /// Gets active benefits (valid and with available quotas).
    /// </summary>
    public async Task<IEnumerable<BenefitResponse>> GetActiveBenefitsAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefits = await _context.Benefits
            .Include(b => b.BenefitType)
            .Where(b => b.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return benefits
            .Where(b => b.CanBeConsumed)
            .Select(MapToResponse)
            .OrderByDescending(b => b.CreatedAt);
    }

    /// <summary>
    /// Creates a new benefit.
    /// </summary>
    public async Task<BenefitResponse> CreateBenefitAsync(BenefitRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Validate benefit type exists in the tenant
        var benefitTypeExists = await _context.BenefitTypes
            .AnyAsync(bt => bt.Id == request.BenefitTypeId && bt.TenantId == tenantId, cancellationToken);

        if (!benefitTypeExists)
            throw new InvalidOperationException("The specified benefit type does not exist in this tenant.");

        // Parse validity period if provided
        DateRange? validityPeriod = null;
        if (!string.IsNullOrWhiteSpace(request.StartDate) && !string.IsNullOrWhiteSpace(request.EndDate))
        {
            if (!DateOnly.TryParse(request.StartDate, out var startDate))
                throw new ArgumentException("Invalid start date format.", nameof(request.StartDate));

            if (!DateOnly.TryParse(request.EndDate, out var endDate))
                throw new ArgumentException("Invalid end date format.", nameof(request.EndDate));

            validityPeriod = new DateRange(startDate, endDate);
        }

        var benefit = new Benefit(tenantId, request.BenefitTypeId, request.Quotas, validityPeriod);

        _context.Benefits.Add(benefit);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with benefit type
        var savedBenefit = await _context.Benefits
            .Include(b => b.BenefitType)
            .FirstAsync(b => b.Id == benefit.Id, cancellationToken);

        return MapToResponse(savedBenefit);
    }

    /// <summary>
    /// Updates an existing benefit.
    /// </summary>
    public async Task<BenefitResponse> UpdateBenefitAsync(int id, BenefitRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefit = await _context.Benefits
            .Include(b => b.BenefitType)
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId, cancellationToken);

        if (benefit == null)
            throw new InvalidOperationException("Benefit not found.");

        // Validate benefit type exists in the tenant if it's being changed
        if (benefit.BenefitTypeId != request.BenefitTypeId)
        {
            var benefitTypeExists = await _context.BenefitTypes
                .AnyAsync(bt => bt.Id == request.BenefitTypeId && bt.TenantId == tenantId, cancellationToken);

            if (!benefitTypeExists)
                throw new InvalidOperationException("The specified benefit type does not exist in this tenant.");
        }

        // Parse validity period if provided
        DateRange? validityPeriod = null;
        if (!string.IsNullOrWhiteSpace(request.StartDate) && !string.IsNullOrWhiteSpace(request.EndDate))
        {
            if (!DateOnly.TryParse(request.StartDate, out var startDate))
                throw new ArgumentException("Invalid start date format.", nameof(request.StartDate));

            if (!DateOnly.TryParse(request.EndDate, out var endDate))
                throw new ArgumentException("Invalid end date format.", nameof(request.EndDate));

            validityPeriod = new DateRange(startDate, endDate);
        }

        // Update benefit using domain methods
        var quotaDifference = request.Quotas - benefit.Quotas;
        if (quotaDifference > 0)
        {
            benefit.AddQuotas(quotaDifference);
        }
        else if (quotaDifference < 0)
        {
            benefit.ConsumeQuotas(Math.Abs(quotaDifference));
        }

        benefit.UpdateValidityPeriod(validityPeriod);

        await _context.SaveChangesAsync(cancellationToken);

        // Reload with updated benefit type if changed
        if (benefit.BenefitTypeId != request.BenefitTypeId)
        {
            var updatedBenefit = await _context.Benefits
                .Include(b => b.BenefitType)
                .FirstAsync(b => b.Id == id, cancellationToken);
            return MapToResponse(updatedBenefit);
        }

        return MapToResponse(benefit);
    }

    /// <summary>
    /// Deletes a benefit.
    /// </summary>
    public async Task<bool> DeleteBenefitAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefit = await _context.Benefits
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId, cancellationToken);

        if (benefit == null)
            return false;

        // Note: In a real scenario, you might want to check for Usage records that reference this benefit
        // For now, we'll allow deletion as the domain model doesn't have a direct Consumption -> Benefit relationship

        _context.Benefits.Remove(benefit);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Consumes a benefit for a user, creating Usage and Consumption records.
    /// </summary>
    public async Task<ConsumeBenefitResponse> ConsumeBenefitAsync(int userId, ConsumeBenefitRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Validate user exists in the tenant
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == userId && u.TenantId == tenantId, cancellationToken);

        if (!userExists)
            throw new InvalidOperationException("Usuario no encontrado.");

        // Get the benefit with its type
        var benefit = await _context.Benefits
            .Include(b => b.BenefitType)
            .FirstOrDefaultAsync(b => b.Id == request.BenefitId && b.TenantId == tenantId, cancellationToken);

        if (benefit == null)
            throw new InvalidOperationException("Beneficio no encontrado.");

        // Validate the benefit can be consumed
        if (!benefit.IsValid)
            throw new InvalidOperationException("El beneficio no está vigente.");

        if (!benefit.HasAvailableQuotas)
            throw new InvalidOperationException("El beneficio no tiene cuotas disponibles.");

        if (request.Quantity > benefit.Quotas)
            throw new InvalidOperationException($"No hay suficientes cuotas disponibles. Cuotas disponibles: {benefit.Quotas}");

        // Create Usage record
        var usage = new Usage(tenantId, benefit.Id, userId, request.Quantity);
        _context.Usages.Add(usage);
        await _context.SaveChangesAsync(cancellationToken);

        // Create Consumption record
        var consumptionDateTime = DateTime.UtcNow;
        var consumption = new Consumption(tenantId, request.Quantity, consumptionDateTime, usage.Id);
        _context.Consumptions.Add(consumption);

        // Consume quotas from the benefit
        benefit.ConsumeQuotas(request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return new ConsumeBenefitResponse
        {
            UsageId = usage.Id,
            ConsumptionId = consumption.Id,
            BenefitId = benefit.Id,
            BenefitTypeName = benefit.BenefitType.Name,
            QuantityConsumed = request.Quantity,
            RemainingQuotas = benefit.Quotas,
            ConsumptionDateTime = consumptionDateTime,
            Message = $"Beneficio '{benefit.BenefitType.Name}' consumido exitosamente."
        };
    }

    /// <summary>
    /// Maps a Benefit entity to a BenefitResponse DTO.
    /// </summary>
    private static BenefitResponse MapToResponse(Benefit benefit)
    {
        return new BenefitResponse
        {
            Id = benefit.Id,
            TenantId = benefit.TenantId,
            BenefitTypeId = benefit.BenefitTypeId,
            BenefitTypeName = benefit.BenefitType.Name,
            Quotas = benefit.Quotas,
            StartDate = benefit.ValidityPeriod?.StartDate.ToString("yyyy-MM-dd"),
            EndDate = benefit.ValidityPeriod?.EndDate.ToString("yyyy-MM-dd"),
            IsValid = benefit.IsValid,
            HasAvailableQuotas = benefit.HasAvailableQuotas,
            CanBeConsumed = benefit.CanBeConsumed,
            IsPermanent = benefit.ValidityPeriod == null,
            CreatedAt = benefit.CreatedAt,
            UpdatedAt = benefit.UpdatedAt
        };
    }
}
