using Application.AccessEvents.DTOs;
using Application.Common.Interfaces;
using Shared.DTOs.AccessEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.AccessEvents;

/// <summary>
/// Service for validating user access to control points based on credentials, roles, and access rules.
/// </summary>
public class AccessValidationService : IAccessValidationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AccessValidationService> _logger;

    public AccessValidationService(
        IApplicationDbContext context,
        ILogger<AccessValidationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AccessValidationResult> ValidateAccessAsync(
        int userId, 
        int controlPointId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating access for user {UserId} to control point {ControlPointId}", 
            userId, controlPointId);

        // DEBUG: Ver cuántos usuarios hay en total
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        _logger.LogWarning("DEBUG: Total users in database: {Count}", totalUsers);

        // DEBUG: Ver si el usuario 1 existe sin includes
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        _logger.LogWarning("DEBUG: User {UserId} exists: {Exists}", userId, userExists);

        // 1. Verificar que el usuario existe
        var user = await _context.Users
            .Include(u => u.Roles)
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return new AccessValidationResult
            {
                IsGranted = false,
                Reason = "Usuario no encontrado",
                UserName = "Usuario Desconocido",
                ControlPointName = "Punto de Control",
                SpaceName = string.Empty
            };
        }

        _logger.LogInformation("🔍 User found: Id={UserId}, Email={Email}, TenantId={TenantId}, FullName={FullName}", 
            user.Id, user.Email, user.TenantId, user.PersonalData.FullName);

        var userName = user.PersonalData.FullName;
        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = user.Email;
        }

        // 2. Verificar que el control point existe
        var controlPoint = await _context.ControlPoints
            .Include(cp => cp.Space)
            .FirstOrDefaultAsync(cp => cp.Id == controlPointId, cancellationToken);

        if (controlPoint == null)
        {
            _logger.LogWarning("Control point {ControlPointId} not found", controlPointId);
            return AccessValidationResultExtensions.Denied(user.Id, userName, "Punto de Control Desconocido", string.Empty, "Punto de control no encontrado");
        }

        _logger.LogInformation("🔍 ControlPoint found: Id={ControlPointId}, Name={Name}, TenantId={TenantId}", 
            controlPoint.Id, controlPoint.Name, controlPoint.TenantId);

        // 3. Verificar que el usuario pertenece al mismo tenant
        if (user.TenantId != controlPoint.TenantId)
        {
            _logger.LogWarning("⚠️ TENANT MISMATCH: User TenantId={UserTenantId} vs ControlPoint TenantId={ControlPointTenantId}", 
                user.TenantId, controlPoint.TenantId);
            return AccessValidationResultExtensions.Denied(user.Id, userName, controlPoint.Name, controlPoint.Space?.Name ?? string.Empty, "Punto de acceso no válido");
        }

        // 4. Cargar las AccessRules para este ControlPoint con sus Roles
        var accessRules = await _context.AccessRules
            .Include(ar => ar.Roles)
            .Where(ar => ar.ControlPointId == controlPointId)
            .ToListAsync(cancellationToken);

        // 4. Verificar que el usuario tiene una credencial activa
        if (!user.HasActiveCredential)
        {
            _logger.LogWarning("User {UserId} has no active credential", userId);
            return AccessValidationResultExtensions.Denied(user.Id, userName, controlPoint.Name, controlPoint.Space?.Name ?? string.Empty, "Credencial inactiva o inexistente");
        }

        // 5. Verificar que hay reglas de acceso para este control point
        if (!accessRules.Any())
        {
            _logger.LogWarning("Control point {ControlPointId} has no access rules", controlPointId);
            return AccessValidationResultExtensions.Denied(user.Id, userName, controlPoint.Name, controlPoint.Space?.Name ?? string.Empty, "No hay reglas de acceso configuradas");
        }

        // 6. Obtener los roles del usuario
        var userRoles = user.Roles.ToList();
        if (!userRoles.Any())
        {
            _logger.LogWarning("User {UserId} has no roles assigned", userId);
            return AccessValidationResultExtensions.Denied(user.Id, userName, controlPoint.Name, controlPoint.Space?.Name ?? string.Empty, "Usuario sin roles asignados");
        }

        // 7. Validar acceso según las reglas
        var now = DateTime.UtcNow;
        
        foreach (var accessRule in accessRules)
        {
            // Verificar si la regla está activa en este momento (horario y fecha)
            if (!accessRule.IsActiveAt(now))
            {
                _logger.LogDebug("Access rule {RuleId} is not active at {DateTime}", accessRule.Id, now);
                continue;
            }

            // Verificar si el usuario tiene alguno de los roles permitidos
            if (accessRule.AllowsAccess(userRoles))
            {
                _logger.LogInformation("Access granted for user {UserId} to control point {ControlPointId} via rule {RuleId}", 
                    userId, controlPointId, accessRule.Id);
                
                var roleNames = string.Join(", ", userRoles.Select(r => r.Name));
                return AccessValidationResultExtensions.Granted(
                    user.Id,
                    userName, 
                    controlPoint.Name,
                    controlPoint.Space?.Name ?? string.Empty,
                    $"Acceso autorizado - Roles: {roleNames}");
            }
        }

        // 8. Si llegamos aquí, ninguna regla permitió el acceso
        var activeRulesCount = controlPoint.AccessRules.Count(ar => ar.IsActiveAt(now));
        
        if (activeRulesCount == 0)
        {
            _logger.LogWarning("User {UserId} denied access to control point {ControlPointId} - No active rules at this time", 
                userId, controlPointId);
            return AccessValidationResultExtensions.Denied(user.Id, userName, controlPoint.Name, controlPoint.Space?.Name ?? string.Empty, "Fuera del horario permitido");
        }

        _logger.LogWarning("User {UserId} denied access to control point {ControlPointId} - User roles do not match any active rule", 
            userId, controlPointId);
        return AccessValidationResultExtensions.Denied(user.Id, userName, controlPoint.Name, controlPoint.Space?.Name ?? string.Empty, "Sin permisos para esta área");
    }

    public async Task<AccessValidationResult> ValidateAccessByCredentialAsync(
        int credentialId,
        int controlPointId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 ValidateAccessByCredentialAsync - CredentialId: {CredentialId}, ControlPointId: {ControlPointId}", 
            credentialId, controlPointId);

        // 1. Buscar la credencial y obtener el usuario asociado
        var credential = await _context.Credentials
            .Include(c => c.User)
                .ThenInclude(u => u.Roles)
            .FirstOrDefaultAsync(c => c.Id == credentialId, cancellationToken);

        if (credential == null)
        {
            _logger.LogWarning("❌ Credential {CredentialId} not found", credentialId);
            return new AccessValidationResult
            {
                IsGranted = false,
                Reason = "Credencial no encontrada",
                UserName = "Usuario Desconocido",
                ControlPointName = "Punto de Control",
                SpaceName = string.Empty
            };
        }

        if (!credential.IsActive)
        {
            _logger.LogWarning("❌ Credential {CredentialId} is INACTIVE", credentialId);
            return AccessValidationResultExtensions.Denied(
                credential.UserId,
                credential.User.FullName,
                "Punto de Control",
                string.Empty,
                "Credencial inactiva");
        }

        _logger.LogInformation("✅ Credential found and ACTIVE - UserId: {UserId}, UserName: {UserName}, TenantId: {TenantId}", 
            credential.UserId, credential.User?.FullName, credential.User?.TenantId);

        // 2. Usar el userId de la credencial para validar
        return await ValidateAccessAsync(credential.UserId, controlPointId, cancellationToken);
    }
}
