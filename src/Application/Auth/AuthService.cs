using Shared.DTOs.Auth;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth;

/// <summary>
/// Service for authentication operations.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService? _tokenService;

    public AuthService(IApplicationDbContext context, IPasswordHasher passwordHasher, ITokenService? tokenService = null)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return await LoginAsync(request, customTokenLifetimeMinutes: null, cancellationToken);
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, int? customTokenLifetimeMinutes, CancellationToken cancellationToken = default)
    {
        // Find user by email (case-insensitive)
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (user == null)
        {
            return null;
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        // Generate JWT token
        string? token = null;
        DateTime? expiresAt = null;

        try
        {
            var lifetimeMinutes = customTokenLifetimeMinutes ?? _tokenService?.GetTokenLifetimeMinutes();
            token = _tokenService?.GenerateToken(user.Id, user.Email, user.TenantId, user.Roles.Select(r => r.Name), customTokenLifetimeMinutes) ?? null;
            if (token != null && lifetimeMinutes.HasValue)
            {
                expiresAt = DateTime.UtcNow.AddMinutes(lifetimeMinutes.Value);
            }
        }
        catch
        {
            // If token generation fails, continue returning user info without token
        }

        // Return login response
        return new LoginResponse
        {
            UserId = user.Id,
            CredentialId = user.CredentialId,
            Email = user.Email,
            FullName = user.FullName,
            TenantId = user.TenantId,
            Roles = user.Roles.Select(r => r.Name).ToList(),
            Token = token,
            ExpiresAtUtc = expiresAt
        };
    }

    public async Task<bool> UserHasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user?.HasRole(roleName) ?? false;
    }
}

