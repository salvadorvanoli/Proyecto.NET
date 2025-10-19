using Application.Common.Interfaces;
using Application.Users.DTOs;
using Domain.DataTypes;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Services;

/// <summary>
/// Implementation of user service for managing user operations.
/// </summary>
public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        // Get current tenant ID
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Verify tenant exists
        var tenantExists = await _context.Tenants
            .AnyAsync(t => t.Id == tenantId, cancellationToken);

        if (!tenantExists)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} does not exist.");
        }

        // Check if user with email already exists in this tenant
        var existingUser = await _context.Users
            .Where(u => u.TenantId == tenantId && u.Email == request.Email.ToLower())
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email '{request.Email}' already exists in this tenant.");
        }

        // Hash the password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create PersonalData value object with only the fields it supports
        var birthDate = DateOnly.FromDateTime(request.DateOfBirth);
        var personalData = new PersonalData(
            request.FirstName,
            request.LastName,
            birthDate
        );

        // Create the user entity
        var user = new User(tenantId, request.Email, passwordHash, personalData);

        // Add to database
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Return response
        return MapToResponse(user);
    }

    public async Task<UserResponse?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var user = await _context.Users
            .Where(u => u.Id == id && u.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        return user != null ? MapToResponse(user) : null;
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var user = await _context.Users
            .Where(u => u.Email == email.ToLower() && u.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        return user != null ? MapToResponse(user) : null;
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .ToListAsync(cancellationToken);

        return users.Select(MapToResponse);
    }

    public async Task<IEnumerable<UserResponse>> GetUsersByTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var users = await _context.Users
            .Where(u => u.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return users.Select(MapToResponse);
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.PersonalData.FirstName,
            LastName = user.PersonalData.LastName,
            FullName = user.FullName,
            DateOfBirth = user.PersonalData.BirthDate.ToDateTime(TimeOnly.MinValue),
            TenantId = user.TenantId,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
