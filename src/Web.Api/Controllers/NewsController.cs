using Shared.DTOs.News;
using Application.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Web.Api.Controllers;

/// <summary>
/// Controller for managing news articles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;
    private readonly ILogger<NewsController> _logger;

    public NewsController(INewsService newsService, ILogger<NewsController> logger)
    {
        _newsService = newsService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new news article in the current tenant context.
    /// </summary>
    /// <param name="request">The news creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created news information.</returns>
    /// <response code="201">News created successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="500">An error occurred while creating the news.</response>
    [HttpPost]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(typeof(NewsResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NewsResponse>> CreateNews(
        [FromBody] NewsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var news = await _newsService.CreateNewsAsync(request, cancellationToken);

            _logger.LogInformation("News created successfully with ID {NewsId} for tenant {TenantId}",
                news.Id, news.TenantId);

            return CreatedAtAction(nameof(GetNewsById), new { id = news.Id }, news);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create news: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating news");
            return StatusCode(500, new { error = "An error occurred while creating the news." });
        }
    }

    /// <summary>
    /// Gets a news article by ID.
    /// </summary>
    /// <param name="id">The news ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The news information.</returns>
    /// <response code="200">News found.</response>
    /// <response code="404">News not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NewsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NewsResponse>> GetNewsById(int id, CancellationToken cancellationToken)
    {
        var news = await _newsService.GetNewsByIdAsync(id, cancellationToken);

        if (news == null)
        {
            return NotFound(new { error = $"News with ID {id} not found." });
        }

        return Ok(news);
    }

    /// <summary>
    /// Gets all news articles for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of news articles.</returns>
    /// <response code="200">News retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NewsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NewsResponse>>> GetAllNews(CancellationToken cancellationToken)
    {
        var news = await _newsService.GetAllNewsAsync(cancellationToken);
        return Ok(news);
    }

    /// <summary>
    /// Updates an existing news article.
    /// </summary>
    /// <param name="id">The news ID.</param>
    /// <param name="request">The news update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated news information.</returns>
    /// <response code="200">News updated successfully.</response>
    /// <response code="404">News not found.</response>
    /// <response code="400">Invalid request.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(typeof(NewsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NewsResponse>> UpdateNews(
        int id,
        [FromBody] NewsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var news = await _newsService.UpdateNewsAsync(id, request, cancellationToken);

            _logger.LogInformation("News updated successfully with ID {NewsId}", news.Id);

            return Ok(news);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update news with ID {NewsId}: {Message}", id, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating news with ID {NewsId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the news." });
        }
    }

    /// <summary>
    /// Deletes a news article.
    /// </summary>
    /// <param name="id">The news ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">News deleted successfully.</response>
    /// <response code="404">News not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "AdministradorBackoffice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNews(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _newsService.DeleteNewsAsync(id, cancellationToken);

            if (!result)
            {
                return NotFound(new { error = $"News with ID {id} not found." });
            }

            _logger.LogInformation("News deleted successfully with ID {NewsId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting news with ID {NewsId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the news." });
        }
    }
}
