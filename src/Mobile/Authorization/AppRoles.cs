namespace Mobile.Authorization;

/// <summary>
/// Roles del sistema - deben coincidir con los roles del backend
/// </summary>
public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string Security = "Security";
    public const string Student = "Student";
    public const string Employee = "Employee";
    
    /// <summary>
    /// Verifica si un usuario tiene un rol específico
    /// </summary>
    public static bool HasRole(List<string> userRoles, string requiredRole)
    {
        return userRoles?.Contains(requiredRole) ?? false;
    }
    
    /// <summary>
    /// Verifica si un usuario tiene alguno de los roles especificados
    /// </summary>
    public static bool HasAnyRole(List<string> userRoles, params string[] requiredRoles)
    {
        if (userRoles == null || requiredRoles == null || requiredRoles.Length == 0)
            return false;
            
        return userRoles.Any(role => requiredRoles.Contains(role));
    }
    
    /// <summary>
    /// Verifica si un usuario es administrador
    /// </summary>
    public static bool IsAdministrator(List<string> userRoles)
    {
        return HasRole(userRoles, Administrator);
    }
    
    /// <summary>
    /// Verifica si un usuario es de seguridad
    /// </summary>
    public static bool IsSecurity(List<string> userRoles)
    {
        return HasRole(userRoles, Security);
    }
}
