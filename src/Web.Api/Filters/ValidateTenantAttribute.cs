using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Filters
{
    /// <summary>
    /// Attribute to apply tenant authorization to specific controllers or actions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ValidateTenantAttribute : ServiceFilterAttribute
    {
        public ValidateTenantAttribute() : base(typeof(TenantAuthorizationFilter))
        {
        }
    }
}
