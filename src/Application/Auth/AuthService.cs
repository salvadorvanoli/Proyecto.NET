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

    public AuthService(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Find user by email
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        if (user == null)
        {
            return null;
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        // Return login response
        return new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            TenantId = user.TenantId,
            Roles = user.Roles.Select(r => r.Name).ToList()
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

