using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for tenant operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(ITenantService tenantService, ILogger<TenantsController> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the theme configuration for a specific tenant.
    /// </summary>
    /// <param name="id">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tenant theme configuration.</returns>
    /// <response code="200">Theme retrieved successfully.</response>
    /// <response code="404">Tenant not found.</response>
    [HttpGet("{id}/theme")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantTheme(int id, CancellationToken cancellationToken)
    {
        try
        {
            var theme = await _tenantService.GetTenantThemeAsync(id, cancellationToken);

            if (theme == null)
            {
                return NotFound(new { error = $"Tenant with ID {id} not found." });
            }

            return Ok(theme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving theme for tenant {TenantId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the tenant theme." });
        }
    }
}
