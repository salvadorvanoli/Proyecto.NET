namespace Application.Common.Interfaces;

/// <summary>
/// Provides the current tenant context for multi-tenant operations.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the current tenant ID from the request context.
    /// </summary>
    int GetCurrentTenantId();

    /// <summary>
    /// Sets the current tenant ID for the request context.
    /// </summary>
    void SetCurrentTenantId(int tenantId);
}

