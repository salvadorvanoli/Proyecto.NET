using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Shared.DTOs.News;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.News;

public class CreateModel : PageModel
{
    private readonly INewsApiService _newsApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(INewsApiService newsApiService, ILogger<CreateModel> logger)
    {
        _newsApiService = newsApiService;
        _logger = logger;
    }

    [BindProperty]
    public NewsRequest News { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        News.PublishDate = DateTime.Now;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var createdNews = await _newsApiService.CreateNewsAsync(News);

            TempData["SuccessMessage"] = $"Noticia '{createdNews.Title}' creada exitosamente.";
            return RedirectToPage("/News/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating news");
            ErrorMessage = "Error al crear la noticia. Por favor, intente nuevamente.";
            return Page();
        }
    }
}

