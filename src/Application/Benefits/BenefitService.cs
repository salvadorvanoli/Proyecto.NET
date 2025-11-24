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
    /// Gets all benefits for a specific user.
    /// </summary>
    public async Task<List<BenefitResponse>> GetUserBenefitsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefits = await _context.Benefits
            .Include(b => b.BenefitType)
            .Where(b => b.TenantId == tenantId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return benefits.Select(MapToResponse).ToList();
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
            .Where(b => b.TenantId == tenantId && b.Active)
            .ToListAsync(cancellationToken);

        return benefits
            .Where(b => b.CanBeConsumed)
            .Select(MapToResponse)
            .OrderByDescending(b => b.CreatedAt);
    }

    /// <summary>
    /// Gets available benefits that a user can claim (shows Quotas).
    /// Returns benefits that have available quotas and the user hasn't claimed yet.
    /// </summary>
    public async Task<IEnumerable<AvailableBenefitResponse>> GetAvailableBenefitsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Get all benefit IDs that user has already claimed
        var claimedBenefitIds = await _context.Usages
            .Where(u => u.UserId == userId && u.Benefit.TenantId == tenantId)
            .Select(u => u.BenefitId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get benefits that user hasn't claimed and have available quotas
        var benefits = await _context.Benefits
            .Include(b => b.BenefitType)
            .Where(b => b.TenantId == tenantId 
                && b.Active
                && !claimedBenefitIds.Contains(b.Id)
                && b.Quotas > 0)  // Filter by quotas > 0 in query
            .ToListAsync(cancellationToken);

        // Additional filtering for validity
        return benefits
            .Where(b => b.IsValid)  // Check validity period
            .Select(MapToAvailableBenefitResponse)
            .OrderByDescending(b => b.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Gets redeemable benefits for a user (shows Quantity from Usage).
    /// Returns benefits that the user has claimed and still has quantity to consume.
    /// </summary>
    public async Task<IEnumerable<RedeemableBenefitResponse>> GetRedeemableBenefitsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var usages = await _context.Usages
            .Include(u => u.Benefit)
                .ThenInclude(b => b.BenefitType)
            .Where(u => u.UserId == userId && u.Benefit.TenantId == tenantId && u.Benefit.Active)
            .ToListAsync(cancellationToken);

        return usages
            .Where(u => u.HasAvailableQuantity && u.Benefit.IsValid)
            .Select(MapToRedeemableBenefitResponse)
            .OrderByDescending(b => b.CreatedAt);
    }

    /// <summary>
    /// Gets redeemable benefits with consumption history for a user.
    /// </summary>
    public async Task<List<BenefitWithHistoryResponse>> GetBenefitsWithHistoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var usages = await _context.Usages
            .Include(u => u.Benefit)
                .ThenInclude(b => b.BenefitType)
            .Include(u => u.Consumptions)
            .Where(u => u.UserId == userId && u.Benefit.TenantId == tenantId && u.Benefit.Active)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

        return usages
            .Select(u => new BenefitWithHistoryResponse
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
            })
            .ToList();
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

        var benefit = new Benefit(tenantId, request.BenefitTypeId, request.Quotas, request.Quantity, validityPeriod);

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
        benefit.UpdateQuantity(request.Quantity);

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
    /// Claims a benefit for a user, creating a Usage record.
    /// </summary>
    public async Task<ClaimBenefitResponse> ClaimBenefitAsync(ClaimBenefitRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Validate benefit exists and belongs to tenant
        var benefit = await _context.Benefits
            .FirstOrDefaultAsync(b => b.Id == request.BenefitId && b.TenantId == tenantId, cancellationToken);

        if (benefit == null)
            throw new InvalidOperationException($"El beneficio con ID {request.BenefitId} no existe.");

        // Validate benefit is active
        if (!benefit.Active)
            throw new InvalidOperationException("El beneficio está inactivo y no se puede reclamar.");

        // Validate user exists and belongs to tenant
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == request.UserId && u.TenantId == tenantId, cancellationToken);

        if (!userExists)
            throw new InvalidOperationException($"El usuario con ID {request.UserId} no existe.");

        // Check if benefit has available quotas
        if (benefit.Quotas <= 0)
            throw new InvalidOperationException("El beneficio no tiene cupos disponibles.");

        // Check if user already has an active Usage for this benefit
        var existingUsage = await _context.Usages
            .FirstOrDefaultAsync(u => u.BenefitId == request.BenefitId 
                && u.UserId == request.UserId 
                && u.TenantId == tenantId, 
                cancellationToken);

        if (existingUsage != null)
            throw new InvalidOperationException("El usuario ya ha reclamado este beneficio.");

        // Create new Usage with the benefit's quantity
        var usage = new Usage(tenantId, benefit, request.UserId);

        _context.Usages.Add(usage);

        // Decrement benefit quotas
        benefit.ConsumeQuotas(1);

        await _context.SaveChangesAsync(cancellationToken);

        return new ClaimBenefitResponse
        {
            UsageId = usage.Id,
            BenefitId = benefit.Id,
            UserId = request.UserId,
            UsageQuantity = usage.Quantity,
            RemainingBenefitQuotas = benefit.Quotas,
            ClaimedAt = usage.CreatedAt,
            Message = $"Beneficio reclamado exitosamente. Tienes {usage.Quantity} consumiciones disponibles."
        };
    }

    /// <summary>
    /// Deletes a benefit (soft delete - marks as inactive).
    /// </summary>
    public async Task<bool> DeleteBenefitAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var benefit = await _context.Benefits
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId, cancellationToken);

        if (benefit == null)
            return false;

        // Soft delete: mark as inactive instead of removing from database
        benefit.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Redeems a benefit for a user (consumes 1 quantity from existing Usage).
    /// </summary>
    public async Task<RedeemBenefitResponse> RedeemBenefitAsync(RedeemBenefitRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Validate benefit exists and belongs to tenant
        var benefit = await _context.Benefits
            .FirstOrDefaultAsync(b => b.Id == request.BenefitId && b.TenantId == tenantId, cancellationToken);

        if (benefit == null)
            throw new InvalidOperationException($"El beneficio con ID {request.BenefitId} no existe.");

        // Validate benefit is active
        if (!benefit.Active)
            throw new InvalidOperationException("El beneficio está inactivo y no se puede canjear.");

        // Validate user exists and belongs to tenant
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == request.UserId && u.TenantId == tenantId, cancellationToken);

        if (!userExists)
            throw new InvalidOperationException($"El usuario con ID {request.UserId} no existe.");

        // Find existing usage with available quantity for this user and benefit
        var usage = await _context.Usages
            .Include(u => u.Consumptions)
            .Where(u => u.UserId == request.UserId && u.BenefitId == request.BenefitId && u.TenantId == tenantId)
            .OrderByDescending(u => u.CreatedAt)
            .FirstOrDefaultAsync(u => u.Quantity > 0, cancellationToken);

        if (usage == null)
            throw new InvalidOperationException("No tienes un beneficio reclamado con consumiciones disponibles. Debes reclamar el beneficio primero.");

        // Verify usage has available quantity
        if (usage.Quantity <= 0)
            throw new InvalidOperationException("No tienes consumiciones disponibles para este beneficio.");

        // Decrement usage quantity
        usage.DecrementQuantity(1);

        // Create Consumption record
        var consumption = new Consumption(tenantId, 1, DateTime.UtcNow, usage.Id);
        _context.Consumptions.Add(consumption);

        await _context.SaveChangesAsync(cancellationToken);

        return new RedeemBenefitResponse
        {
            UsageId = usage.Id,
            ConsumptionId = consumption.Id,
            BenefitId = benefit.Id,
            UserId = request.UserId,
            RemainingUsageQuantity = usage.Quantity,
            RemainingBenefitQuotas = benefit.Quotas,
            RedeemedAt = consumption.ConsumptionDateTime,
            IsNewUsage = false,
            Message = $"Beneficio consumido exitosamente. Quedan {usage.Quantity} consumiciones disponibles."
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
            Quantity = benefit.Quantity,
            StartDate = benefit.ValidityPeriod?.StartDate.ToString("yyyy-MM-dd"),
            EndDate = benefit.ValidityPeriod?.EndDate.ToString("yyyy-MM-dd"),
            IsValid = benefit.IsValid,
            HasAvailableQuotas = benefit.HasAvailableQuotas,
            CanBeConsumed = benefit.CanBeConsumed,
            IsPermanent = benefit.ValidityPeriod == null,
            Active = benefit.Active,
            CreatedAt = benefit.CreatedAt,
            UpdatedAt = benefit.UpdatedAt
        };
    }

    /// <summary>
    /// Maps a Benefit entity to an AvailableBenefitResponse DTO (for claiming).
    /// </summary>
    private static AvailableBenefitResponse MapToAvailableBenefitResponse(Benefit benefit)
    {
        return new AvailableBenefitResponse
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
            IsPermanent = benefit.ValidityPeriod == null,
            CreatedAt = benefit.CreatedAt,
            UpdatedAt = benefit.UpdatedAt
        };
    }

    /// <summary>
    /// Maps a Usage entity to a RedeemableBenefitResponse DTO (for redeeming).
    /// </summary>
    private static RedeemableBenefitResponse MapToRedeemableBenefitResponse(Usage usage)
    {
        return new RedeemableBenefitResponse
        {
            BenefitId = usage.BenefitId,
            UsageId = usage.Id,
            TenantId = usage.TenantId,
            BenefitTypeId = usage.Benefit.BenefitTypeId,
            BenefitTypeName = usage.Benefit.BenefitType.Name,
            Quantity = usage.Quantity,
            StartDate = usage.Benefit.ValidityPeriod?.StartDate.ToString("yyyy-MM-dd"),
            EndDate = usage.Benefit.ValidityPeriod?.EndDate.ToString("yyyy-MM-dd"),
            IsValid = usage.Benefit.IsValid,
            CanBeConsumed = usage.HasAvailableQuantity && usage.Benefit.IsValid,
            IsPermanent = usage.Benefit.ValidityPeriod == null,
            CreatedAt = usage.CreatedAt,
            UpdatedAt = usage.UpdatedAt
        };
    }
}

