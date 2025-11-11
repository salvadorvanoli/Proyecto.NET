using System.Collections.Generic;

namespace Application.Common.Interfaces
{
    /// <summary>
    /// Service responsible for generating JWT tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token for the specified user information.
        /// IMPORTANT: Includes TenantId to ensure multi-tenant isolation.
        /// </summary>
        string GenerateToken(int userId, string email, int tenantId, IEnumerable<string> roles);

        /// <summary>
        /// Gets token lifetime in minutes as configured.
        /// </summary>
        int GetTokenLifetimeMinutes();
    }
}
