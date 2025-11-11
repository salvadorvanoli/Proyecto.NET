using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Web.Api.Filters
{
    /// <summary>
    /// Authorization filter that validates multi-tenant isolation.
    /// Ensures users can only access resources from their own tenant.
    /// </summary>
    public class TenantAuthorizationFilter : IAuthorizationFilter
    {
        private readonly ILogger<TenantAuthorizationFilter> _logger;

        public TenantAuthorizationFilter(ILogger<TenantAuthorizationFilter> logger)
        {
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Skip validation for anonymous endpoints
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                return;
            }

            // Get TenantId from JWT token claims
            var tenantIdClaim = context.HttpContext.User.FindFirst("tenant_id");
            
            if (tenantIdClaim == null || !int.TryParse(tenantIdClaim.Value, out var tokenTenantId))
            {
                _logger.LogWarning("Unauthorized access attempt: JWT token does not contain valid TenantId claim");
                context.Result = new UnauthorizedObjectResult(new 
                { 
                    error = "Invalid authentication token: missing tenant information" 
                });
                return;
            }

            // Check if X-Tenant-Id header is present (should NOT be used for authenticated requests)
            if (context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenantId))
            {
                if (int.TryParse(headerTenantId.FirstOrDefault(), out var requestedTenantId))
                {
                    // Reject if header doesn't match token
                    if (requestedTenantId != tokenTenantId)
                    {
                        _logger.LogWarning(
                            "Security violation: User from tenant {TokenTenantId} attempted to access tenant {RequestedTenantId}",
                            tokenTenantId, requestedTenantId);
                        
                        context.Result = new ForbidResult();
                        return;
                    }
                }
            }

            // Store validated tenant ID in HttpContext for downstream use
            context.HttpContext.Items["ValidatedTenantId"] = tokenTenantId;
        }
    }
}
